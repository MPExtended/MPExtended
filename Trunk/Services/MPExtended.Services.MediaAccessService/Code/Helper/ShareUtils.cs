using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using System.Xml;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
    public class ShareUtils
    {/// <summary>
        /// Gets a list of all video shares from MediaPortal.xml
        /// 
        /// Why they are stored there and in such a retarded way is beyond me... ^^
        /// </summary>
        /// <returns>List of all shares containing video</returns>
        public static List<WebShare> GetAllShares(String _shareType)
        {
            List<WebShare> shares = new List<WebShare>();
            XmlDocument document = new XmlDocument();
            document.Load(Utils.GetMpConfigPath());
            XmlNodeList sections = document.GetElementsByTagName("section");
            foreach (XmlNode node in sections)
            {
                if (node.Attributes["name"] != null && node.Attributes["name"].Value == _shareType)
                {
                    XmlNodeList childNodes = node.ChildNodes;
                    String extensions = null;
                    foreach (XmlNode node2 in childNodes)
                    {
                        if (node2.Attributes["name"] != null && node2.Attributes["name"].Value.Equals("extensions"))
                        {
                            extensions = node2.InnerText;
                        }
                        ParseShareNode(node2, shares);
                    }

                    if (extensions != null)
                    {
                        String[] extList = ParseExtensions(extensions);

                        foreach (WebShare v in shares)
                        {
                            v.Extensions = extList;
                        }
                    }
                    break;
                }
            }

            return shares;
        }

        private static string[] ParseExtensions(string _extensionsString)
        {
            String[] extList = _extensionsString.Split(',');

            return extList;
        }

        private static void ParseShareNode(XmlNode _node, List<WebShare> _shares)
        {
            if (_node.Attributes["name"] != null)
            {
                String name = _node.Attributes["name"].Value;

                if (name.StartsWith("sharename"))
                {
                    int index = CheckAndCreateListItem(name, "sharename", _shares);
                    _shares[index].ShareId = index;
                    _shares[index].Name = _node.InnerText;
                }

                if (name.StartsWith("sharepath"))
                {
                    int index = CheckAndCreateListItem(name, "sharepath", _shares);
                    _shares[index].Path = _node.InnerText;
                }

                if (name.StartsWith("pincode"))
                {
                    int index = CheckAndCreateListItem(name, "pincode", _shares);
                    _shares[index].PinCode = _node.InnerText;
                }

                if (name.StartsWith("sharetype"))
                {
                    int index = CheckAndCreateListItem(name, "sharetype", _shares);
                    _shares[index].IsFtp = _node.InnerText.Equals("yes");
                }

                if (name.StartsWith("shareserver"))
                {
                    int index = CheckAndCreateListItem(name, "shareserver", _shares);
                    _shares[index].FtpServer = _node.InnerText;
                }

                if (name.StartsWith("sharelogin"))
                {
                    int index = CheckAndCreateListItem(name, "sharelogin", _shares);
                    _shares[index].FtpLogin = _node.InnerText;
                }

                if (name.StartsWith("sharepassword"))
                {
                    int index = CheckAndCreateListItem(name, "sharepassword", _shares);
                    _shares[index].FtpPassword = _node.InnerText;
                }

                if (name.StartsWith("shareport"))
                {
                    int index = CheckAndCreateListItem(name, "shareport", _shares);
                    _shares[index].FtpPort = Int32.Parse(_node.InnerText);
                }

                if (name.StartsWith("shareremotepath"))
                {
                    int index = CheckAndCreateListItem(name, "shareremotepath", _shares);
                    _shares[index].FtpPath = _node.InnerText;
                }
            }
        }

        private static int CheckAndCreateListItem(string _value, string _propertyname, List<WebShare> _shares)
        {
            int index = Int32.Parse(_value.Replace(_propertyname, ""));
            if (_shares.Count <= index)
            {
                _shares.Add(new WebShare());
            }

            return index;
        }
    }
}
