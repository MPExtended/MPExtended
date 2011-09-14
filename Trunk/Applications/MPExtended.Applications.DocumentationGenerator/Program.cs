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
using System.IO;
using System.Reflection;

namespace MPExtended.Applications.DocumentationGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", "..");

            // let's start with MAS
            Assembly mas = Assembly.LoadFrom(Path.Combine(rootpath, "Services", "MPExtended.Services.MediaAccessService.Interfaces", "bin", "Debug", "MPExtended.Services.MediaAccessService.Interfaces.dll"));
            Generator g = new Generator()
            {
                Assembly = mas,
                API = mas.GetType("MPExtended.Services.MediaAccessService.Interfaces.IMediaAccessService"),
                Output = new StreamWriter(@"C:\Users\Oxan\Downloads\api-doc-out.html")
            };
            g.Generate();

            // finish
            Console.ReadLine();
        }
    }
}
