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
using System.Diagnostics;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.Development.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            bool bla = MPEServices.HasMASConnection;
            /*
            Console.WriteLine("MPExtended performance test tool");
            Console.WriteLine("");
            Console.WriteLine("First Test");
            Console.WriteLine("GetItemCount based on .Net");
            try
            {
                MPEServices.NetPipeMediaAccessService.GetAllVideos();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Console.WriteLine(MPEServices.NetPipeMediaAccessService.GetMovieCount());
            watch.Stop();
            Console.WriteLine("Time: " + watch.ElapsedMilliseconds);
            Console.WriteLine("GetItemCount based on SQL");
            watch.Start();
            Console.WriteLine(MPEServices.NetPipeMediaAccessService.GetMusicTracksCount());
            watch.Stop();
            Console.WriteLine("Time: " + watch.ElapsedMilliseconds);
            Console.Read();
            */
        }
    }
}
