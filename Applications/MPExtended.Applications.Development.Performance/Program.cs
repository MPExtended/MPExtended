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
            Stopwatch watch = new Stopwatch();
            MPEServices.MAS.GetServiceDescription();

            // load all movies
            //int count = MPEServices.MAS.GetMovieCount(null).Count;
            int count = 8000;
            Console.WriteLine("Loading all movies, 1000 at a time...");
            watch.Start();
            for (int i = 0; i < count; )
            {
                int limit = Math.Min(i + 1000, count);
                var list = MPEServices.MAS.GetMoviesBasicByRange(null, i, limit);
                i = limit;
                Console.WriteLine("Finished iteration, {0}ms elapsed", watch.ElapsedMilliseconds);
                break;
            }
            watch.Stop();
            Console.WriteLine("That took {0} milliseconds", watch.ElapsedMilliseconds);

            // return
            Console.ReadKey();
        }
    }
}
