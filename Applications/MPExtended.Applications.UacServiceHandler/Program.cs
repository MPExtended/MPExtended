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
using System.Linq;
using System.Text;

namespace MPExtended.Applications.UacServiceHandler
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                switch (GetArgument(args, "/command"))
                {
                    case "service":
                        new WindowsServiceHandler("MPExtended Service").Execute(GetArgument(args, "/action"));
                        break;
                    case "webmphosting":
                        new WindowsServiceHandler("MPExtended WebMediaPortal").Execute(GetArgument(args, "/action"));
                        break;
                    default:
                        DieWithUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static string GetArgument(string[] args, string name, string defaultValue)
        {
            int i = 0;
            foreach (string arg in args)
            {
                if (arg == name && args.Length != i + 1)
                {
                    return args[i + 1];
                }
                else if (arg.StartsWith(name + ":") && arg.Length > name.Length + 1)
                {
                    return arg.Substring(name.Length + 1);
                }
            }

            if (defaultValue != null)
            {
                return defaultValue;
            }

            DieWithUsage();
            return ""; // impossible
        }

        private static string GetArgument(string[] args, string name)
        {
            return GetArgument(args, name, null);
        }

        private static void DieWithUsage()
        {
            Console.WriteLine("Usage: UacServiceHelper.exe /command:(service) [/action:(start|stop|restart)]");
            Environment.Exit(1);
        }
    }
}
