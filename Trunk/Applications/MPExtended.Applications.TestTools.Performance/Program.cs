using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Applications.TestTools.Performance
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("MPExtended performance test tool");
                        Console.WriteLine("");
                        Console.WriteLine("First Test");
                        Console.WriteLine("GetItemCount based on .Net");
                        try
                        {
                            MPEServices.NetPipeMediaAccessService.GetAllVideos();
                        }
                        catch (Exception ex)
                        { }
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
        }
    }
}
