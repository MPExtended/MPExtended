#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace MPExtended.Libraries.Client
{
    public class WCFHeader<TContent>
    {
        public string HeaderNamespace { get; set; }
        public string Name { get; set; }
        public TContent Content { get; set; }

        public WCFHeader(string ns, string name, TContent content)
        {
            HeaderNamespace = ns;
            Name = name;
            Content = content;
        }

        public WCFHeader(string name, TContent content)
            : this ("http://mpextended.github.com/", name, content)
        {
        }

        public WCFHeader(string name)
            : this(name, default(TContent))
        {
        }
    }

    public class WCFClient
    {
        public static OperationContextScope EnterOperationScope(object client)
        {
            return new OperationContextScope((IContextChannel)client);
        }

        public static TResult GetHeader<TResult>(WCFHeader<TResult> header)
        {
            return OperationContext.Current.IncomingMessageHeaders.GetHeader<TResult>(header.Name, header.HeaderNamespace);
        }

        public static TResult GetHeader<TResult>(WCFHeader<TResult> header, TResult defaultValue)
        {
            try
            {
                return GetHeader(header);
            }
            catch(MessageHeaderException)
            {
                return defaultValue;
            }
        }

        public static TResult GetHeader<TResult>(string header)
        {
            return GetHeader(new WCFHeader<TResult>(header));
        }

        public static TResult GetHeader<TResult>(string header, TResult defaultValue)
        {
            return GetHeader(new WCFHeader<TResult>(header), defaultValue);
        }

        public static void SetHeader<TContent>(WCFHeader<TContent> wcfHeader)
        {
            MessageHeader<TContent> header = new System.ServiceModel.MessageHeader<TContent>(wcfHeader.Content);
            MessageHeader untyped = header.GetUntypedHeader(wcfHeader.Name, wcfHeader.HeaderNamespace);
            OperationContext.Current.OutgoingMessageHeaders.Add(untyped);
        }

        public static void SetHeader<TContent>(string header, TContent content)
        {
            SetHeader(new WCFHeader<TContent>(header, content));
        }
    }
}
