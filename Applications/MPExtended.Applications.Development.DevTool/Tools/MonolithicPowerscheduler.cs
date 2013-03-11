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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.Tools
{
    internal class MonolithicPowerscheduler : IDevTool
    {
        public string Name { get { return "Monolithic PowerScheduler exporter"; } }
        public TextWriter OutputStream { get; set; }

        public void Run()
        {
            // "C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /target:library /v2 /out:MPExtended.PowerScheduler.dll MPExtended.Applications.PowerSchedulerPlugin.dll MPExtended.Services.MetaService.Interfaces.dll MPExtended.Services.Common.Interfaces.dll
            string ilmerge = FindILMerge();
            if (ilmerge == null)
                return;

            string arguments = String.Format(
                @"/target:library /v2 ""/out:{0}/MPExtended.PowerScheduler.dll"" ""{0}/MPExtended.Applications.PowerSchedulerPlugin.dll"" ""{0}/MPExtended.Services.MetaService.Interfaces.dll"" ""{0}/MPExtended.Services.Common.Interfaces.dll""",
                Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.PowerSchedulerPlugin", "bin", Installation.GetSourceBuildDirectoryName())
            );
            var proc = Process.Start(ilmerge, arguments);
            proc.WaitForExit();

            if (proc.ExitCode == 0)
            {
                OutputStream.WriteLine("Done!");
            }
            else
            {
                OutputStream.WriteLine("Failed!");
            }
        }

        private string FindILMerge()
        {
            string progx86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "ILMerge", "ILMerge.exe");
            if (File.Exists(progx86))
                return progx86;

            string prog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "ILMerge", "ILMerge.exe");
            if (File.Exists(prog))
                return prog;

            Console.WriteLine("Couldn't find ILMerge - aborting");
            return null;
        }
    }
}
