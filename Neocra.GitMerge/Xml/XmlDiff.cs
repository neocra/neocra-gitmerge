using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Neocra.GitMerge.Xml
{

    public class XmlDiff : Diff
    {
        public List<XmlDiff>? Children { get; }
        private static int prefixid = 0;

        public XObject Value { get; }
        public string ParentPath { get; }
        
        /// <summary>
        /// Gets index of the node in children with the same name in case of XElement
        /// </summary>
        public int Index { get; }
        
        /// <summary>
        /// Gets index of the new node in children with the same name in case of XElement
        /// </summary>
        public int MoveIndex { get; }

        public string ElementPath(XmlNamespaceManager xmlNamespaceResolver, int index)
        {
            return GetXPathOfElement(xmlNamespaceResolver, this.Value, this.ParentPath, index);
        }
        
        private static string GetXPathOfElement(XmlNamespaceManager xmlNamespaceResolver, XObject ancestor, string path, int index) =>
            ancestor switch
            {
                XElement element => GetXPathOfElement(xmlNamespaceResolver, element, path, index),
                XAttribute attribute => path + "/@" + attribute.Name.LocalName,
                _ => path
            };

        public static string GetXPathOfElement(XmlNamespaceManager xmlNamespaceResolver, XElement ancestor, string path, int index)
        {
            if (string.IsNullOrEmpty(ancestor.Name.NamespaceName))
            {
                return $"{path}/{ancestor.Name.LocalName}[{index + 1}]";
            }

            var prefix = xmlNamespaceResolver.LookupPrefix(ancestor.Name.NamespaceName);
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = $"p{prefixid++}";
                xmlNamespaceResolver.AddNamespace(prefix, ancestor.Name.NamespaceName);
            }

            return $"{path}/{prefix}:{ancestor.Name.LocalName}[{index + 1}]";
        }

        public XmlDiff(DiffMode mode, XNode value, string parentPath, int index, int indexOfChild)
            : base(mode, indexOfChild, 0)
        {
            this.Value = value;
            this.ParentPath = parentPath;
            this.Index = index;
        }

        public XmlDiff(DiffMode mode, XNode value, string parentPath, int index, int indexOfChild, int moveIndex, int moveIndexOfChild)
            : base(mode, indexOfChild, moveIndexOfChild)
        {
            this.Value = value;
            this.ParentPath = parentPath;
            this.Index = index;
            this.MoveIndex = moveIndex;
            this.MoveIndexOfChild = moveIndexOfChild;
        }
        
        public XmlDiff(DiffMode mode, XAttribute value, string parentPath)
            : base(mode, 0, 0)
        {
            this.Value = value;
            this.ParentPath = parentPath;
        }

        public XmlDiff(DiffMode mode, int index, XObject value, string parentPath, List<XmlDiff> children) 
            : base(mode, index, 0)
        {
            this.ParentPath = parentPath;
            this.Children = children;
            this.Value = value;
        }

        public string ToString(XmlNamespaceManager xmlNamespaceResolver, int index, int moveIndex, int indexChild, int moveIndexChild)
        {
            return this.Mode switch
            {
                DiffMode.Add => $"+ ({this.Value.GetType().Name}) {this.ElementPath(xmlNamespaceResolver, index)} ({index}) {this.Value}",
                DiffMode.Delete => $"- ({this.Value.GetType().Name}) {this.ElementPath(xmlNamespaceResolver, index)} ({index}) ({indexChild})  {this.Value}",
                DiffMode.Update => $"U ({this.Value.GetType().Name}) {this.ElementPath(xmlNamespaceResolver, index)} ({index}) {this.Value}",
                DiffMode.Move => $"M ({this.Value.GetType().Name}) {this.ElementPath(xmlNamespaceResolver, index)} ({index} => {moveIndex}) ({indexChild} => {moveIndexChild}) {this.Value}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public override string ToString()
        {
            var manager = new XmlNamespaceManager(new NameTable());
            return this.Mode switch
            {
                DiffMode.Add => $"+ ({this.Value.GetType().Name}) {this.ElementPath(manager, Index)} ({Index}) {this.Value}",
                DiffMode.Delete => $"- ({this.Value.GetType().Name}) {this.ElementPath(manager, Index)} ({Index}) ({IndexOfChild})  {this.Value}",
                DiffMode.Update => $"U ({this.Value.GetType().Name}) {this.ElementPath(manager, Index)} ({Index}) {this.Value}",
                DiffMode.Move => $"M ({this.Value.GetType().Name}) {this.ElementPath(manager, Index)} ({Index} => {MoveIndex}) ({IndexOfChild} => {MoveIndexOfChild}) {this.Value}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}