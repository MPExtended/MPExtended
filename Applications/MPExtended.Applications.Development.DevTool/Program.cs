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
using System.IO;
using System.Linq;
using System.Text;

namespace MPExtended.Applications.Development.DevTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tools = new IDevTool[] {
                new InterfaceCheck(),
                new DocGen.DocDevTool(),
            };

            string line = "help";
            do
            {
                switch (line)
                {
                    case "help":
                    case "h":
                        Console.WriteLine("exit: Quit");
                        Console.WriteLine("help: Print this help");
                        int i = 0;
                        foreach (var tool in tools)
                        {
                            Console.WriteLine("{0}: {1}", i++, tool.Name);
                        }
                        break;

                    case "exit":
                    case "quit":
                    case "e":
                    case "q":
                        return;

                    default:
                        try
                        {
                            IDevTool tool = tools.ElementAt(Int32.Parse(line));
                            tool.OutputStream = (TextWriter)Console.Out;
                            tool.InputStream = (TextReader)Console.In;
                            tool.Run();
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("Tool {0} not found", Int32.Parse(line));
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid command");
                        }
                        break;
                }

                Console.WriteLine("");
                Console.Write("DevTool> ");
                line = Console.ReadLine().Trim();
            } while (line != "q" && line != "quit");
        }
    }
}
