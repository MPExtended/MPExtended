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
using System.Reflection;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.DocGen
{
    internal abstract class Generator
    {
        public Assembly Assembly { get; set; }
        public TextWriter Output { get; set; }
        public TextWriter UserStream { get; set; }
        protected Type JsonAPI { get; set; }
        protected Type StreamAPI { get; set; }
        protected IEnumerable<Type> Enumerations { get; set; }

        protected virtual int GenerateSortOrder(string methodName)
        {
            return 1;
        }

        protected virtual Dictionary<int, string> GetHeadings()
        {
            return new Dictionary<int, string>()
            {
                { 1, "General" },
            };
        }

        protected virtual string MapName(MethodInfo method, string typename)
        {
            var namemap = new Dictionary<string, string>()
            {
                { "Int32", "int" },
                { "Int64", "long" },
                { "String", "string" },
                { "Boolean", "bool" },
                { "Double", "float" },
                { "Single", "float" },
                { "Decimal", "float" },
                { "WebDictionary`1", "WebDictionary" },
            };
            if(namemap.ContainsKey(typename))
                return namemap[typename];
            return typename;
        }

        public void Generate()
        {
            // header
            Output.WriteLine(
                "<p>This page contains the API documentation for this MPExtended service, as automatically generated on {0} for version {1} (build {2}). " +
                "Please do not edit, as your changes will be overwritten.</p>",
                DateTime.Now.ToString("dd MMM yyy HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                VersionUtil.GetVersion(Assembly),
                VersionUtil.GetBuildVersion(Assembly));

            UserStream.WriteLine("Generating documentation for assembly {0}", Assembly.GetName().Name);

            // get all items 
            IEnumerable<DocGenItem> typesToDocument = new List<DocGenItem>();
            if (JsonAPI != null)
                typesToDocument = JsonAPI.GetMethods().Select(x => new DocGenItem()
                {
                    URLPrefix = "/json",
                    Reflected = x,
                    Name = x.Name,
                    Order = GenerateSortOrder(x.Name)
                });
            if(StreamAPI != null)
                typesToDocument = typesToDocument.Union(StreamAPI.GetMethods().Select(x => new DocGenItem()
                {
                    URLPrefix = "/stream",
                    Reflected = x,
                    Name = x.Name,
                    Order = GenerateSortOrder(x.Name)
                }));
            if (Enumerations != null)
                typesToDocument = typesToDocument.Union(Enumerations.Select(x => new DocGenItem()
                {
                    URLPrefix = "",
                    Reflected = x,
                    Name = x.Name,
                    Order = GenerateSortOrder(x.Name),
                }));

            // sort all types
            typesToDocument = typesToDocument
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name);

            // print navigation
            int lastOrder = -1;
            UserStream.WriteLine("=> Generating documentation header");
            Output.WriteLine("<h3>Navigation</h3>");
            foreach (var item in typesToDocument)
            {
                if (lastOrder != item.Order)
                {
                    if (lastOrder != -1) Output.WriteLine("</ul>");
                    Output.WriteLine("<h4>{0}</h4><ul>", GetHeadings()[item.Order]);
                    lastOrder = item.Order;
                }
                Output.WriteLine("<li><a href=\"#{0}\">{0}</a></li>", item.Name);
            }
            Output.WriteLine("</ul>");

            // generate all documentation
            lastOrder = -1;
            foreach (var item in typesToDocument)
            {
                if (lastOrder != item.Order)
                {
                    Output.WriteLine(String.Format("<h3>{0}</h3>", GetHeadings()[item.Order]));
                    lastOrder = item.Order;
                }

                if (item.Reflected is MethodInfo)
                {
                    GenerateMethodDocumentation(item.Reflected as MethodInfo, item.URLPrefix);
                }
                else if (item.Reflected is Type)
                {
                    GenerateEnumDocumentation(item.Reflected as Type);
                }
            }
            UserStream.WriteLine("=> Done");

            Output.Flush();
            Output.Close();
        }

        protected void GenerateMethodDocumentation(MethodInfo method, string urlprefix)
        {
            // general method
            UserStream.WriteLine("=> Generating documentation for method {0}", method.Name);
            Output.WriteLine(String.Format("<h4 name=\"{0}\">{0}</h4><dl>", method.Name));
            Output.WriteLine(String.Format("<dt>URL</dt><dd>{0}/{1}</dd>", urlprefix, method.Name));

            // parameters
            Output.WriteLine("<dt>Parameters</dt><dd>");
            GenerateParameterDocumentation(method);
            Output.WriteLine("</dd>");

            // return type
            Type itemType = null;
            if (method.ReturnType.Name == "Stream")
            {
                Output.WriteLine(String.Format("<dt>Returns</dt><dd>Raw data"));
            }
            else if (IsListType(method.ReturnType, out itemType))
            {
                Output.WriteLine(String.Format("<dt>Returns</dt><dd>List of <strong>{0}</strong><br />", MapName(method, itemType.Name)));
            }
            else
            {
                Output.WriteLine(String.Format("<dt>Returns</dt><dd><strong>{0}</strong><br />", MapName(method, itemType.Name)));
            }
            if (itemType != null && !IsUndocumentableType(itemType))
            {
                GenerateReturnDocumentation(method, itemType);
            }
            Output.WriteLine("</dd>");
            Output.WriteLine("</dl>");
        }

        protected void GenerateParameterDocumentation(MethodInfo method)
        {
            if (method.GetParameters().Count() == 0)
            {
                Output.WriteLine("<em>none</em>");
                return;
            }

            Output.WriteLine("<ul>");
            foreach (var param in method.GetParameters())
            {
                if (param.IsOptional && param.RawDefaultValue == null)
                {
                    Output.WriteLine("<li><strong>{0}</strong> (nullable {1})", param.Name, GenerateTypeNameLink(method, param.ParameterType));
                }
                else if (Nullable.GetUnderlyingType(param.ParameterType) != null)
                {
                    Type realType = Nullable.GetUnderlyingType(param.ParameterType);
                    Output.WriteLine("<li><strong>{0}</strong> (nullable {1})", param.Name, GenerateTypeNameLink(method, realType));
                }
                else
                {
                    Output.WriteLine("<li><strong>{0}</strong> ({1})", param.Name, GenerateTypeNameLink(method, param.ParameterType));
                }
            }
            Output.WriteLine("</ul>");
        }

        protected void GenerateReturnDocumentation(MethodInfo method, Type type)
        {
            UserStream.WriteLine("==> Generating documentation for type {0}", type.Name);
            Output.WriteLine("<ul>");
            foreach (var property in type.GetProperties())
            {
                Type otype;
                bool isList = IsListType(property.PropertyType, out otype);
                if (property.PropertyType.Name.StartsWith("Web") || (isList && otype.Name.StartsWith("Web")))
                {
                    Output.WriteLine(String.Format("<li><strong>{0}</strong> ({1}):", property.Name, GenerateTypeNameLink(method, property.PropertyType)));
                    GenerateReturnDocumentation(method, otype);
                    Output.WriteLine(String.Format("</li>"));
                }
                else
                {
                    Output.WriteLine(String.Format("<li><strong>{0}</strong> ({1})</li>", property.Name, GenerateTypeNameLink(method, property.PropertyType)));
                }
            }
            Output.WriteLine("</ul>");
        }

        protected void GenerateEnumDocumentation(Type type)
        {
            UserStream.WriteLine("=> Generating documentation for type {0}", type.Name);
            Output.WriteLine("<h4 name=\"{0}\">{0} (enumeration)</h4>", type.Name);
            Output.WriteLine("<dl><dt>Values</dt><dd><ul>");
            foreach (var val in type.GetEnumValues())
            {
                Output.WriteLine("<li>{0}: {1}</li>", type.GetEnumName(val), (int)val);
            }
            Output.WriteLine("</ul></dt></dl>");
        }

        protected string GenerateTypeNameLink(MethodInfo method, Type type)
        {
            Type elementType;
            string typename = IsListType(type, out elementType) ? "list of " + MapName(method, elementType.Name) : MapName(method, type.Name);
            return typename == elementType.Name && type.Assembly.GetName().Name != "MPExtended.Services.Common.Interfaces" && !IsUndocumentableType(type)
                ? "<a href=\"#" + typename + "\">" + typename + "</a>"
                : typename;
        }

        protected bool IsListType(Type type, out Type elementType)
        {
            // i've a feeling .NET already has a method for this
            bool isList = type.Name != "String" && !type.Name.Contains("WebDictionary") &&
                (type.GetMethods().Count(x => x.Name == "GetEnumerator") > 0 || 
                    type.GetInterfaces().Count(x => x.Name == "IEnumerable") > 0);
            elementType = null;
            if (isList)
            {
                if (type.GetElementType() != null)
                    elementType = type.GetElementType();
                if (elementType == null && type.GetGenericArguments().Count() > 0)
                    elementType = type.GetGenericArguments().First();
            }
            else
            {
                elementType = type;
            }

            return isList;
        }

        protected bool IsUndocumentableType(Type type)
        {
            if (type == typeof(string) || type == typeof(char) || type == typeof(Stream) || type == typeof(DateTime) || type.Name.Contains("WebDictionary"))
            {
                return true;
            }

            return false;
        }
    }
}
