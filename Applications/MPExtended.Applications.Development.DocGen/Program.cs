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
using System.IO;
using System.Reflection;

namespace MPExtended.Applications.Development.DocGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", "..");

            // let's start with MAS
            Assembly mas = Assembly.LoadFrom(Path.Combine(rootpath, "Services", "MPExtended.Services.MediaAccessService.Interfaces", "bin", "Debug", "MPExtended.Services.MediaAccessService.Interfaces.dll"));
            Generator g1 = new MASGenerator(mas)
            {
                Output = new StreamWriter(@"C:\Users\Oxan\Downloads\api-doc-mas.txt")
            };
            g1.Generate();

            // continue with WSS
            Assembly wss = Assembly.LoadFrom(Path.Combine(rootpath, "Services", "MPExtended.Services.StreamingService.Interfaces", "bin", "Debug", "MPExtended.Services.StreamingService.Interfaces.dll"));
            Generator g2 = new WSSGenerator(wss)
            {
                Output = new StreamWriter(@"C:\Users\Oxan\Downloads\api-doc-wss.txt")
            };
            g2.Generate();

            // and finish with TAS
            Assembly tas = Assembly.LoadFrom(Path.Combine(rootpath, "Services", "MPExtended.Services.TVAccessService.Interfaces", "bin", "Debug", "MPExtended.Services.TVAccessService.Interfaces.dll"));
            Generator g3 = new TASGenerator(tas)
            {
                Output = new StreamWriter(@"C:\Users\Oxan\Downloads\api-doc-tas.txt")
            };
            g3.Generate();

            // finish
            Console.ReadKey();
        }
    }
}
