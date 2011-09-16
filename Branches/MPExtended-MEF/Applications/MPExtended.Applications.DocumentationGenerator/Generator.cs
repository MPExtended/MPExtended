#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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

namespace MPExtended.Applications.DocumentationGenerator
{
    internal class Generator
    {
        private Dictionary<string, string> namemap = new Dictionary<string, string>()
        {
            { "Int32", "int" },
            { "Int64", "long" },
            { "String", "string" },
            { "Boolean", "bool" },
            { "Double", "float" },
        };

        public Assembly Assembly { get; set; }
        public Type API { get; set; }
        public TextWriter Output { get; set; }

        public void Generate()
        {
            Console.WriteLine("Generating documentation for assembly {0}", Assembly.GetName().Name);
            Console.WriteLine("=> Generating documentation for class {0}", API.Name);
            var methods = API.GetMethods().OrderBy(x => GenerateSortOrder(x.Name)).ThenBy(x => x.Name);
            foreach (var method in methods)
            {
                Console.WriteLine("==> Generating documentation for method {0}", method.Name);
                Output.WriteLine(String.Format("<h4>{0}</h4><dl>", method.Name));
                Output.WriteLine(String.Format("<dt>URL</dt><dd>/{0}</dd>", method.Name));
                
                // parameters
                Output.WriteLine("<dt>Parameters</dt><dd>");
                GenerateParameterDocumentation(method);
                Output.WriteLine("</dd>");

                // return type
                Type itemType;
                if (IsListType(method.ReturnType, out itemType))
                {
                    Output.WriteLine(String.Format("<dt>Returns</dt><dd>List of <strong>{0}</strong><br />", itemType.Name));
                }
                else
                {
                    Output.WriteLine(String.Format("<dt>Returns</dt><dd><strong>{0}</strong><br />", itemType.Name));
                }
                GenerateReturnDocumentation(itemType);
                Output.WriteLine("</dd>");

                Output.WriteLine("</dl>");
            }
            Console.WriteLine("=> Done");

            Output.Flush();
            Output.Close();
        }

        private void GenerateParameterDocumentation(MethodInfo method)
        {
            if (method.GetParameters().Count() == 0)
            {
                Output.WriteLine("<em>none</em>");
                return;
            }

            Output.WriteLine("<ul>");
            foreach (var param in method.GetParameters())
            {
                Output.WriteLine("<li><strong>{0}</strong> ({1})", param.Name, MapName(param.ParameterType.Name));
            }
            Output.WriteLine("</ul>");
        }

        private void GenerateReturnDocumentation(Type type)
        {
            Console.WriteLine("===> Generating documentation for type {0}", type.Name);
            Output.WriteLine("<ul>");
            foreach (var property in type.GetProperties())
            {
                Type elementType;
                string typename = IsListType(property.PropertyType, out elementType) ? "list of " + MapName(elementType.Name) : property.PropertyType.Name;
                Output.WriteLine(String.Format("<li><strong>{0}</strong> ({1})</li>", property.Name, MapName(typename)));
            }
            Output.WriteLine("</ul>");
        }

        private bool IsListType(Type type, out Type elementType)
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

        private string MapName(string input)
        {
            if (namemap.ContainsKey(input))
                return namemap[input];

            return input;
        }

        private int GenerateSortOrder(string methodName)
        {
            if (methodName.Contains("Movie")) return 2;
            if (methodName.Contains("Music")) return 3;
            if (methodName.Contains("Picture")) return 4;
            if (methodName.Contains("TV")) return 5;
            if (methodName.Contains("FileSystem")) return 6;
            return 1; // show unknown at first
        }
    }
}
