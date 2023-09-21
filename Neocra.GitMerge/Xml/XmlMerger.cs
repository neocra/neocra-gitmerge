using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Xml
{

    public class XmlMerger : IMerger
    {
        internal readonly ILogger<XmlMerger> logger;
        private readonly XmlDataAccess xmlDataAccess;
        private readonly DiffTools diffTools;

        public XmlMerger(ILogger<XmlMerger> logger, XmlDataAccess xmlDataAccess, DiffTools diffTools)
        {
            this.logger = logger;
            this.xmlDataAccess = xmlDataAccess;
            this.diffTools = diffTools;
        }
        
        public MergeStatus Merge(MergeSettings opts)
        {
            var ancestor = this.xmlDataAccess.ReadXmlFile(opts.Ancestor);
            var current = this.xmlDataAccess.ReadXmlFile(opts.Current);
            var other = this.xmlDataAccess.ReadXmlFile(opts.Other);

            this.logger.LogDebug("=== Ancestor");
            this.logger.LogDebug(ancestor.ToString());
            this.logger.LogDebug("=== Current");
            this.logger.LogDebug(current.ToString());
            this.logger.LogDebug("=== Other");
            this.logger.LogDebug(other.ToString());

            var xmlNamespaceResolver = new XmlNamespaceManager(new NameTable());

            if (ancestor.Root == null || current.Root == null || other.Root == null)
            {
                throw new NotSupportedException();
            }
            
            var diffCurrent = this.Diff(xmlNamespaceResolver, ancestor.Root, current.Root, "", 0)
                .Cast<XmlDiff>()
                .ToList();
            var diffOther = this.Diff(xmlNamespaceResolver, ancestor.Root, other.Root, "", 0)
                .Cast<XmlDiff>()
                .ToList();

            this.logger.LogInformation("Diff in current branch " + diffCurrent.Count);
            this.logger.LogInformation("Diff in other branch " + diffOther.Count);
            if (DetectConflict(diffCurrent, diffOther))
            {
                return MergeStatus.Conflict;
            }
            
            this.ApplyDiff(diffCurrent.Union(diffOther).ToList(), ancestor, xmlNamespaceResolver);

            this.xmlDataAccess.SaveXmlDocument(opts, ancestor);

            return MergeStatus.Good;
        }

        protected virtual bool DetectConflict(List<XmlDiff> diffsCurrent, List<XmlDiff> diffsOther)
        {
            foreach (var diffCurrent in diffsCurrent.ToList())
            {
                var diffOther = diffsOther
                .FirstOrDefault(d => d.ParentPath == diffCurrent.ParentPath
                 && d.Index == diffCurrent.Index
                 && d.IndexOfChild == diffCurrent.IndexOfChild);

                if (diffOther == null)
                {
                    continue;
                }

                logger.LogInformation($"Has conflict on ({diffCurrent}) and ({diffOther}) ?");
                
                if (ResolveConflict(diffsCurrent, diffsOther, diffCurrent, diffOther)) return true;
            }

            return false;
        }

        protected virtual bool ResolveConflict(List<XmlDiff> diffsCurrent, List<XmlDiff> diffsOther, XmlDiff diffCurrent, XmlDiff? diffOther)
        {
            if (diffOther == null)
            {
                throw new NotSupportedException();
            }
            
            switch (diffCurrent.Mode, diffOther.Mode)
            {
                case (DiffMode.Delete, DiffMode.Update):
                    return false;
                case (DiffMode.Update, DiffMode.Update):
                    if (diffCurrent.Children != null && diffOther.Children != null)
                    {
                        return DetectConflict(diffCurrent.Children, diffOther.Children);
                    }

                    if (diffCurrent.Value is XText current && diffOther.Value is XText other)
                    {
                        if (current.Value == other.Value)
                        {
                            diffsOther.Remove(diffOther);
                            return false;
                        }
                    }
                    
                    return true;
                case (DiffMode.Delete, DiffMode.Delete):
                    diffsOther.Remove(diffOther);
                    break;
                case (DiffMode.Add, DiffMode.Add):
                    break;
                case (DiffMode.Move, DiffMode.Add):
                    break;
                case (DiffMode.Update, DiffMode.Add):
                case (DiffMode.Add, DiffMode.Update):
                    break;
                case (DiffMode.Move, DiffMode.Move):
                    if (diffCurrent.Index != diffOther.Index
                        || diffCurrent.IndexOfChild != diffOther.IndexOfChild
                        || diffCurrent.MoveIndex != diffOther.MoveIndex
                        || diffCurrent.MoveIndexOfChild != diffOther.MoveIndexOfChild)
                    {
                        return true;
                    }
                    break;
                case var value:
                    throw NotSupportedExceptions.Value(value);
            }

            return false;
        }


        public virtual string ProviderCode => "xml";
        
        private void ApplyDiff(List<XmlDiff> diffOther, XDocument current, XmlNamespaceManager xmlNamespaceResolver)
        {
            var currentRoot = current.Root;

            if (currentRoot == null)
            {
                throw new NotSupportedException();
            }
            
            this.ApplyDiff2(diffOther, xmlNamespaceResolver, currentRoot);
        }

        private void ApplyDiff2(List<XmlDiff> diffOther, XmlNamespaceManager xmlNamespaceResolver, XElement parent)
        {
            var move = (from d in diffOther
                where d.Mode == DiffMode.Move
                select d).ToDictionary(d => d.ElementPath(xmlNamespaceResolver, d.Index), d => d.ElementPath(xmlNamespaceResolver, d.MoveIndex));

            var moveIndex = new Dictionary<int, int>();
            var moveIndexOfChild = new Dictionary<int, int>();

            foreach (var diff in diffOther
                .OrderBy(d => d.ParentPath)
                .ThenBy(d => d.IndexOfChild))
            {
                var index = diff.Index;
                var indexOfChild = diff.IndexOfChild;
                index = moveIndex.ContainsKey(index) ? moveIndex[index] : index;
                indexOfChild = moveIndexOfChild.ContainsKey(indexOfChild) ? moveIndexOfChild[indexOfChild] : indexOfChild;
                
                var countIndex = GetCountIndexOfElement(diffOther, diff, index);
                var countIndexOfChild = GetCountIndexOfNode(diffOther, diff, indexOfChild);
                var countMoveIndex = GetCountIndexOfElement(diffOther, diff, diff.MoveIndex);
                var countMoveIndexOfChild = GetCountIndexOfNode(diffOther, diff, diff.MoveIndexOfChild);

                var diffIndex = index + countIndex;

                var diffIndexOfChild = indexOfChild + countIndexOfChild;

                this.logger.LogInformation(diff.ToString(
                    xmlNamespaceResolver,
                    diffIndex,
                    diff.MoveIndex + countMoveIndex,
                    diffIndexOfChild,
                    diff.MoveIndexOfChild + countMoveIndexOfChild));
                
                switch (diff)
                {
                    case { Mode: DiffMode.Add, Value: var value }:
                        ApplyAdd(parent, diffIndexOfChild, value);
                        break;
                    case { Mode: DiffMode.Update, Value: XAttribute xAttribute }:
                    {
                        parent
                            .Attributes()
                            .First(a => a.Name == xAttribute.Name)
                            .SetValue(xAttribute.Value);
                        break;
                    }
                    case { Mode: DiffMode.Move, Value: XElement xElement }:
                    {
                        var pathSelectElement = parent.Nodes()
                            .OfType<XElement>()
                            .Where(n => n.Name == xElement.Name)
                            .ElementAt(diffIndex);
                        pathSelectElement
                            .Remove();

                        var xNodes = parent.Nodes();
                        if (xNodes.Count() > diff.MoveIndexOfChild)
                        {
                            xNodes.ElementAt(diff.MoveIndexOfChild)
                                .AddBeforeSelf(pathSelectElement);
                        }
                        else
                        {
                            parent.Add(pathSelectElement);
                        }

                        moveIndex.Add(index, diff.MoveIndex);
                        moveIndexOfChild.Add(indexOfChild, diff.MoveIndexOfChild);
                        break;
                    }
                    case { Mode: DiffMode.Delete, Value: XElement xElement }:
                        parent.Nodes()
                            .OfType<XElement>()
                            .Where(n => n.Name == xElement.Name)
                            .ElementAt(diffIndex)
                            .Remove();
                        break;
                    case { Mode: DiffMode.Delete, Value: XText }:
                        parent
                            .Nodes().ElementAt(diffIndexOfChild).Remove();
                        break;
                    case { Mode: DiffMode.Delete, Value: XAttribute xAttribute }:
                        parent
                            .Attributes()
                            .First(a => a.Name == xAttribute.Name)
                            .Remove();
                        break;

                    case { Mode: DiffMode.Update, Value: XElement xElement }:
                        var element = parent.Nodes()
                            .OfType<XElement>()
                            .Where(n => n.Name == xElement.Name)
                            .ElementAt(diffIndexOfChild);
                        if (diff.Children != null)
                        {
                            this.ApplyDiff2(diff.Children, xmlNamespaceResolver, element);
                        }
                        break;
                    case {Mode: DiffMode.Update, Value: XText text}:
                        parent
                            .Nodes().ElementAt(diffIndexOfChild).ReplaceWith(text);
                        break;
                    case var val:
                        throw NotSupportedExceptions.Value(val);
                }
            }
        }

        private static void ApplyAdd(XElement parent, int diffIndexOfChild, XObject obj)
        {
            var xNodes = parent.Nodes();
            if (xNodes.Count() > diffIndexOfChild && diffIndexOfChild >= 0)
            {
                xNodes.ElementAt(diffIndexOfChild)
                    .AddBeforeSelf(obj);
            }
            else
            {
                parent.Add(obj);
            }
        }

        private static int GetCountIndexOfElement(List<XmlDiff> diffOther, XmlDiff xmlDiff, int diffIndex)
        {
            if (xmlDiff.Value is XElement e2)
            {
                return (from d in diffOther
                    where d.ParentPath == xmlDiff.ParentPath
                        && d.Index < diffIndex
                        && d.Value is XElement e && e.Name.LocalName == e2.Name.LocalName
                        && (d.Mode == DiffMode.Add || d.Mode == DiffMode.Delete || d.Mode == DiffMode.Move)
                    select d).Sum(d =>
                {
                    if (d.Mode == DiffMode.Move)
                    {
                        var val = 0;
                        if (d.Index < diffIndex)
                        {
                            val--;
                        }
                        if (d.MoveIndex < diffIndex)
                        {
                            val++;
                        }

                        return val;
                    }
                    if (d.Mode == DiffMode.Add)
                        return 1;
                    else
                        return -1;
                });
            }

            return GetCountIndexOfNode(diffOther, xmlDiff, diffIndex);
        }
        
        private static int GetCountIndexOfNode(List<XmlDiff> diffOther, XmlDiff xmlDiff, int diffIndex)
        {
            return (from d in diffOther
                where d.ParentPath == xmlDiff.ParentPath
                    && d.IndexOfChild < diffIndex
                    && (d.Mode == DiffMode.Add || d.Mode == DiffMode.Delete || d.Mode == DiffMode.Move)
                select d).Sum(d =>
            {
                if (d.Mode == DiffMode.Move)
                {
                    return 0;
                }

                if (d.Mode == DiffMode.Add)
                {
                    return 0;
                }
                
                return -1;
            });
        }

        public IEnumerable<XmlDiff> Diff(XmlNamespaceManager xmlNamespaceResolver, XElement ancestor, XElement current, string parentPath, int index)
        {
            var children = new List<XmlDiff>();
            var path = XmlDiff.GetXPathOfElement(xmlNamespaceResolver, ancestor, parentPath, index);
            if (ancestor.Name != current.Name)
            {
                yield return new XmlDiff(DiffMode.Delete, current, parentPath, index, index);
                yield return new XmlDiff(DiffMode.Add, ancestor, parentPath, index, index);
            }
            else
            {
                foreach (var diff1 in GetDiffForAttributes(ancestor, current, path))
                {
                    yield return diff1;
                }
                
                foreach (var diff2 in this.diffTools.GetDiffOfChildrenFusion(
                     new XmlDiffToolsConfig(path, this, xmlNamespaceResolver),
                     ancestor.Nodes().ToList(),
                     current.Nodes().ToList()))
                {
                    yield return (XmlDiff)diff2;
                }
            }
        }

        private static IEnumerable<XmlDiff> GetDiffForAttributes(XElement ancestor, XElement current, string path)
        {
            var attributeAncestor = ancestor.Attributes().ToList();
            var attributeCurrent = current.Attributes().ToList();

            foreach (var attribute in attributeAncestor)
            {
                var currentAttribute = attributeCurrent
                    .FirstOrDefault(a => a.Name == attribute.Name);
                if (currentAttribute != null)
                {
                    if (currentAttribute.Value != attribute.Value)
                    {
                        yield return new XmlDiff(DiffMode.Update, 
                            currentAttribute,
                            path);
                    }
                }
                else
                {
                    yield return new XmlDiff(DiffMode.Delete, attribute, path);
                }
            }

            foreach (var attribute in attributeCurrent)
            {
                if (attributeAncestor.All(a => a.Name != attribute.Name))
                {
                    yield return new XmlDiff(DiffMode.Add, attribute, path);
                }
            }
        }
    }
}