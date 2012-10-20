using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPExtended.Libraries.VLCWrapper
{
    class Program
    {
        static String version = "0.3.0";
        static String userAgent = "VLC Wrapper for MPExtended";
        static String httpUserAgent = "VLCWrapper/" + version;
        static int logInterval = 500;
        static GlobalState globalState = GlobalState.Null;

        enum GlobalState
        {
            Null,
            Started,
            Playing,
            Finished,
            Error
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void VlcEventHandlerDelegate(ref libvlc_event_t libvlc_event, IntPtr userData);

        static void EventCallback(ref libvlc_event_t libvlc_event, IntPtr userData) 
        {
            globalState = (GlobalState)userData;
            Console.WriteLine("S {0}", globalState.ToString().ToLower());
        }

        static void RegisterEvent(IntPtr eventManager, libvlc_event_e eventType, IntPtr type)
        {
            VlcEventHandlerDelegate callback = EventCallback; 
            NativeMethods.libvlc_event_attach(eventManager, eventType, Marshal.GetFunctionPointerForDelegate(callback), type);
        }

        static int Main(string[] args)
        {
            // init arguments
            if(args.Length < 2) 
            {
                Console.WriteLine("Usage: vlcwrapper <input> <soutstring> [optional vlc arguments]");
                return 1;
            }

            String path = args[0];
            String sout = args[1];

            Console.WriteLine("I VLCWrapper version {0}, using VLC {1}", version, Marshal.PtrToStringAnsi(NativeMethods.libvlc_get_version()));
            Console.WriteLine("A path {0}", path);
            Console.WriteLine("A config {0}", sout);

            // arguments (you shouldn't need these in normal usage; but we don't support all arguments by ourself yet)
            String[] vlcArgv = new String[args.Length - 2];
            for (int i = 2; i < args.Length; i++)
                vlcArgv[i - 2] = args[i];
   
            for(int i = 0; i < vlcArgv.Length; i++)
                Console.WriteLine("A cmd {0} {1}", i, vlcArgv[i]);

            // init vlc
            IntPtr vlc = NativeMethods.libvlc_new(vlcArgv.Length, vlcArgv);
            NativeMethods.libvlc_set_user_agent(vlc, Encoding.UTF8.GetBytes(userAgent), Encoding.UTF8.GetBytes(httpUserAgent));

            // create media and set sout string
            IntPtr media = NativeMethods.libvlc_media_new_path(vlc, Encoding.UTF8.GetBytes(path));
            String soutOption = String.Format("sout={0}", sout);
            NativeMethods.libvlc_media_add_option(media, Encoding.UTF8.GetBytes(soutOption));

            // create player and listen for events
            IntPtr player = NativeMethods.libvlc_media_player_new_from_media(media);
            IntPtr eventManager = NativeMethods.libvlc_media_player_event_manager(player);
            RegisterEvent(eventManager, libvlc_event_e.libvlc_MediaPlayerPlaying, (IntPtr)GlobalState.Playing);
            RegisterEvent(eventManager, libvlc_event_e.libvlc_MediaPlayerEncounteredError, (IntPtr)GlobalState.Error);
            RegisterEvent(eventManager, libvlc_event_e.libvlc_MediaPlayerEndReached, (IntPtr)GlobalState.Finished);
   
            // start playing it
            NativeMethods.libvlc_media_player_play(player);
   
            // wait till it's started or stopped because of an error
            while(globalState != GlobalState.Error && globalState != GlobalState.Playing)
                Thread.Sleep(100);

            // let it play till it has finished, while printing status information
            while(globalState == GlobalState.Playing) 
            {
                Console.Out.WriteLine("P {0}", NativeMethods.libvlc_media_player_get_time(player));
                Thread.Sleep(logInterval);
            }
   
            // stop playing
            NativeMethods.libvlc_media_player_stop(player);
   
            // release objects in memory
            NativeMethods.libvlc_media_release(media);
            NativeMethods.libvlc_media_player_release(player);
            NativeMethods.libvlc_release(vlc);
            
            return 0;
        }
    }
}
