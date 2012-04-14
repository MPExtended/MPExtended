#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
        public static TResult CallWithHeader<TResult, THeader>(string headerNamespace, string headerName, THeader headerContent, object client, Func<TResult> method)
        {
            using (var scope = new OperationContextScope((IContextChannel)client))
            {
                MessageHeader<THeader> header = new System.ServiceModel.MessageHeader<THeader>(headerContent);
                MessageHeader untyped = header.GetUntypedHeader(headerName, headerNamespace);
                OperationContext.Current.OutgoingMessageHeaders.Add(untyped);

                return method.Invoke();
            }
        }

        public static TResult CallWithHeader<TResult, THeader>(WCFHeader<THeader> header, object client, Func<TResult> method)
        {
            return CallWithHeader<TResult, THeader>(header.HeaderNamespace, header.Name, header.Content, client, method);
        }

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
    }
}
