#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // init
            if (Installation.GetFileLayoutType() != FileLayoutType.Source)
            {
                Console.WriteLine("DevTool only works from a source tree");
                Environment.Exit(1);
            }



            // command line operating modes
            if (args.Length >= 2 && args[0] == "/noquestions")
            {
                OperateNonInteractive(args);
            }
            else
            {
                OperateInteractive();
            }
        }

        private static IDevTool[] ListTools()
        {
            return new IDevTool[] {
                new Tools.InterfaceCheck(),
                new DocGen.DocDevTool(),
                new Tools.WixFSGenerator(),
                new Tools.MyGengoImporter(),
                new Tools.InstallLayoutExporter(),
                new Tools.MonolithicPowerscheduler()
            };
        }

        private static void OperateNonInteractive(string[] args)
        {
            foreach (var tool in ListTools())
            {
                if (tool.GetType().Name == args[1])
                {
                    if (tool is IQuestioningDevTool)
                    {
                        int arg = 2;
                        (tool as IDevTool).OutputStream = Console.Out;
                        (tool as IQuestioningDevTool).Answers = new Dictionary<string, string>();
                        foreach (var question in (tool as IQuestioningDevTool).Questions)
                        {
                            (tool as IQuestioningDevTool).Answers[question.Name] = args[arg++];
                        }
                    }

                    tool.Run();
                    return;
                }
            }

            Console.WriteLine("Couldn't find tool {0}", args[1]);
        }

        private static void OperateInteractive()
        {
            // list our tools
            var tools = ListTools();

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
                            if (tool is IQuestioningDevTool)
                            {
                                (tool as IQuestioningDevTool).Answers = new Dictionary<string, string>();
                                foreach (var question in (tool as IQuestioningDevTool).Questions)
                                {
                                    Console.Write(question.Text);
                                    (tool as IQuestioningDevTool).Answers[question.Name] = Console.ReadLine();
                                }
                            }
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
