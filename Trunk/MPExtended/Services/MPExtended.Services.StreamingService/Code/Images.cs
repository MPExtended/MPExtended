using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class Images
    {
        public static Stream ExtractImage(string source, int startPosition, int? maxWidth, int? maxHeight)
        {
            if (!File.Exists(source))
            {
                Log.Warn("ExtractImage: File " + source + " doesn't exist or is not accessible");
                return null;
            }
            // calculate size
            string ffmpegResize = "";
            if (maxWidth != null && maxHeight != null)
            {
                decimal resolution = MediaInfoWrapper.GetMediaInfo(source).VideoStreams.First().DisplayAspectRatio;
                ffmpegResize = "-s " + Resolution.Calculate(resolution, new Resolution(maxWidth.Value, maxHeight.Value)).ToString();
            }

            // execute it
            string tempFile = Path.GetTempFileName(); // FIXME: maybe we need to clean this up?
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = String.Format("-ss {0} -vframes 1 -i \"{1}\" {2} -f image2 {3}", startPosition, source, ffmpegResize, tempFile);
            info.FileName = Profiles.GetTranscoderProfiles().First().Transcoder; // FIXME
            info.UseShellExecute = false;
            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();

            return new FileStream(tempFile, FileMode.Open, FileAccess.Read);
        }
    }
}
