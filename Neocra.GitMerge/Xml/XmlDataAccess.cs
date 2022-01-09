using System.Xml;
using System.Xml.Linq;

namespace Neocra.GitMerge.Xml
{
    public class XmlDataAccess
    {
        public XDocument ReadXmlFile(string file)
        {
            return XDocument.Load(file, LoadOptions.PreserveWhitespace);
        }
        
        public void SaveXmlDocument(MergeOptions opts, XDocument current)
        {
            var xmlTextWriter = XmlWriter.Create(opts.Current,
                new XmlWriterSettings
                {
                    OmitXmlDeclaration = current.Declaration == null,
                });
            current.Save(xmlTextWriter);
            xmlTextWriter.Flush();
            xmlTextWriter.Close();
        }
    }
}