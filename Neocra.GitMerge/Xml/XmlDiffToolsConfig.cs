using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Xml
{
    public class XmlDiffToolsConfig : DiffToolsConfig<XNode, XmlDiff>
    {
        private readonly string path;
        private readonly XmlMerger xmlMerger;
        private readonly XmlNamespaceManager xmlNamespaceResolver;

        public XmlDiffToolsConfig(string path, XmlMerger xmlMerger, XmlNamespaceManager xmlNamespaceResolver)
        {
            this.path = path;
            this.xmlMerger = xmlMerger;
            this.xmlNamespaceResolver = xmlNamespaceResolver;
        }

        public override int Distance(XmlDiff delete, XmlDiff add)
        {
            return StringTools.Compute(add.Value.ToString() ?? string.Empty, delete.Value.ToString() ?? string.Empty);
        }

        public override bool CanFusion(XmlDiff delete, XmlDiff add)
        {
            return (add.Value is XElement be2
                && delete.Value is XElement ae2
                && be2.Name.LocalName == ae2.Name.LocalName) ||
                (add.Value is XText te1 && delete.Value is XText te2 && add.Index == delete.Index);
        }

        public override XmlDiff CreateMove(XmlDiff delete, XmlDiff add)
        {
            return new XmlDiff(DiffMode.Move,
                (XElement)delete.Value,
                delete.ParentPath,
                delete.Index,
                delete.IndexOfChild,
                add.Index,
                add.IndexOfChild);
        }

        public override bool IsElementEquals(XNode a, XNode b)
        {
            return a.ToString() == b.ToString();
        }
        
        public override XmlDiff CreateDiff(DiffMode mode, List<XNode> elements, int index)
        {
            var indexNode = GetIndexOfNode(elements, elements[index]);

            return new XmlDiff(mode, elements[index], this.path, indexNode, index);
        }
        
        private static int GetIndexOfNode<T>(List<T> nodes, T node)
            where T : XNode
        {
            if (node is XElement element)
            {
                return nodes
                    .OfType<XElement>().ToList()
                    .Where(e => e.Name.LocalName == element.Name.LocalName)
                    .ToList().GetIndexOf(element, (e1, e2) => e1 == e2) ?? -1;
            }

            return nodes.GetIndexOf(node, (e1, e2) => e1 == e2) ?? -1;
        }

        // public override IEnumerable<Diff> MakeARecursiveDiffs(XmlDiff delete, XmlDiff add)
        // {
        //     var children = this.xmlMerger.Diff(this.xmlNamespaceResolver, delete.Value as XElement, add.Value as XElement, delete.ParentPath, delete.Index)
        //         .ToList();
        //     
        //     return children;
        // }

        public override Diff? MakeARecursive(XmlDiff delete, XmlDiff add)
        {
            if (delete.Value is XElement && add.Value is XElement)
            {
                var children = this.xmlMerger.Diff(this.xmlNamespaceResolver, (XElement)delete.Value, (XElement)add.Value, delete.ParentPath, delete.Index)
                    .ToList();

                if (children.Count > 0)
                {
                    return new XmlDiff(DiffMode.Update, delete.Index, delete.Value, this.path, children);
                }

                return null;
            }
            else if (delete.Value is XText && add.Value is XText text)
            {
                return new XmlDiff(DiffMode.Update, text, this.path, delete.Index, delete.IndexOfChild);
            }

            throw NotSupportedExceptions.Value((delete, add));
        }
    }
}