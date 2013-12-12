#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Collections.ObjectModel;
using Microsoft.ServiceModel.Samples;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Runtime;

namespace MPExtended.Libraries.Service.WCF
{
    public class CustomWebHttpBinding : WebHttpBinding
    {
        public CustomWebHttpBinding()
            : base()
        {
            //Log.Debug(GetType().FullName + "::ctor");
        }

        public CustomWebHttpBinding(string configurationName)
            : base(configurationName)
        {
            //Log.Debug(GetType().FullName + "::ctor with " + configurationName);
        }

        public CustomWebHttpBinding(WebHttpSecurityMode securityMode)
            : base(securityMode)
        {
            //Log.Debug(GetType().FullName + "::ctor with " + securityMode);
        }

        private void ShowMessageEncodingDetails(MessageEncodingBindingElement element)
        {
            string encodingInfoStr = "messageVersion=" + element.MessageVersion;
            if (element is WebMessageEncodingBindingElement)
            {
                WebMessageEncodingBindingElement webElement = (WebMessageEncodingBindingElement)element;
                encodingInfoStr += ", writeEncoding=" + webElement.WriteEncoding;
                if (webElement.ContentTypeMapper != null)
                {
                    encodingInfoStr += ",contentTypeMapper=" + webElement.ContentTypeMapper.GetType().FullName;
                }
                if (webElement.ReaderQuotas != null)
                {
                    encodingInfoStr += ",readerQuotas=" + webElement.ReaderQuotas.GetType().FullName;
                }
            }
            Log.Debug(GetType().FullName + "::ShowMessageEncodingDetails message encoding binding element " + element.GetType().FullName + " (" + encodingInfoStr + ")");
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = base.CreateBindingElements();

            BindingElementCollection customElements = new BindingElementCollection();
            foreach (BindingElement element in elements)
            {
                BindingElement customElement = element;
                if (element is MessageEncodingBindingElement)
                {
                    MessageEncodingBindingElement encodingElement = (MessageEncodingBindingElement)element;
                    //ShowMessageEncodingDetails(encodingElement);

                    //Log.Debug(GetType().FullName + "::CreateBindingElements wrapping message encoding binding element in gzip");
                    customElement = new GZipMessageEncodingBindingElement(encodingElement);

                    /*
                    if (element is WebMessageEncodingBindingElement)
                    {
                        WebMessageEncodingBindingElement webElement = (WebMessageEncodingBindingElement)element;
                        ReaderQuotas.CopyTo(webElement.ReaderQuotas);

                        ShowMessageEncodingDetails(encodingElement);
                    }
                    */
                }

                customElements.Add(customElement);
            }

            return customElements.Clone();
        }
    }
}
