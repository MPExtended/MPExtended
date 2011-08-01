using System.Xml;

namespace WebMediaPortal.Code
{
    public static class ExtensionMethods
    {
        public static XmlNode GetFirstNode(this XmlNodeList list, string nodeName)
        {
            foreach (XmlNode node in list)
            {
                if (node.Name == nodeName) return node;
            }
            return null;
        }
    }
}