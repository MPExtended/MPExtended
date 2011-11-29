#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;

namespace MPExtended.Applications.Development.DevTool.DocGen
{
    internal abstract class Generator
    {
        public Assembly Assembly { get; set; }
        public TextWriter Output { get; set; }
        public TextWriter UserStream { get; set; }
        protected Type JsonAPI { get; set; }
        protected Type StreamAPI { get; set; }
        protected IEnumerable<Type> Enums { get; set; }

        protected abstract int GenerateSortOrder(string methodName);
        protected abstract Dictionary<int, string> GetHeadings();

        protected virtual void CustomGenerate()
        {
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
                VersionUtil.GetVersion(JsonAPI.Assembly),
                VersionUtil.GetBuildVersion(JsonAPI.Assembly));

            UserStream.WriteLine("Generating documentation for assembly {0}", Assembly.GetName().Name);
            CustomGenerate();

            // get all items 
            var itemstogen = JsonAPI.GetMethods().Select(x => new DocGenItem()
            {
                URLPrefix = "/json",
                Reflected = x,
                Name = x.Name,
                Order = GenerateSortOrder(x.Name)
            });
            if(Enums != null)
                itemstogen = itemstogen.Union(Enums.Select(x => new DocGenItem()
                    {
                        URLPrefix = "",
                        Reflected = x,
                        Name = x.Name,
                        Order = GenerateSortOrder(x.Name),
                    }));
            if(StreamAPI != null)
                itemstogen = itemstogen.Union(StreamAPI.GetMethods().Select(x => new DocGenItem()
                {
                    URLPrefix = "/stream",
                    Reflected = x,
                    Name = x.Name,
                    Order = GenerateSortOrder(x.Name)
                }));

            itemstogen = itemstogen
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name);
            int lastOrder = -1;

            // navigation
            UserStream.WriteLine("=> Generating documentation header");
            Output.WriteLine("<h3>Navigation</h3>");
            foreach (var item in itemstogen)
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

            // method docs
            foreach (var item in itemstogen)
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
                Output.WriteLine(String.Format("<dt>Returns</dt><dd>List of <strong>{0}</strong><br />", itemType.Name));
            }
            else
            {
                Output.WriteLine(String.Format("<dt>Returns</dt><dd><strong>{0}</strong><br />", itemType.Name));
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
                else if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type realType = param.ParameterType.GetGenericArguments().First();
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
            return typename == elementType.Name ? "<a href=\"#" + typename + "\">" + typename + "</a>" : typename;
        }

        protected bool IsListType(Type type, out Type elementType)
        {
            // i've a feeling .NET already has a method for this
            bool isList = type.Name != "String" && 
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
            if (type == typeof(string) || type == typeof(char) || type == typeof(Stream))
            {
                return true;
            }

            return false;
        }
    }
}
