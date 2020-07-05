#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Logging;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
  internal static class Images
  {
    private static ImageCache cache = new ImageCache();

    public static Stream ExtractImage(MediaSource source, long position, string format)
    {
      return Images.ExtractImage(source, position, null, null, null, format);
    }

    public static Stream ExtractImage(MediaSource source, long position, int? maxWidth, int? maxHeight, string borders, string format)
    {
      if (!source.Exists)
      {
        Log.Warn("ExtractImage: Source {0} (resolved to path {1}) doesn't exists", source.GetDebugName(), source.GetPath());
        WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
        return Stream.Null;
      }

      if (!source.SupportsDirectAccess)
      {
        Log.Warn("ExtractImage: Extracting images from remote sources isn't supported yet", source.MediaType, source.Id);
        WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotImplemented);
        return Stream.Null;
      }

      // get temporary filename
      string filename = String.Format("extract_{0}_{1}.jpg", source.GetUniqueIdentifier(), position);
      string tempFile = cache.GetPath(filename);

      // maybe it exists in cache, return that then
      if (cache.Contains(filename))
      {
        // if source is newer, cached image needs to be recreated
        if (source.GetFileInfo().LastModifiedTime > cache.GetLastModifiedTime(tempFile))
        {
          cache.Invalidate(filename);
        }
        else
        {
          return StreamPostprocessedImage(new ImageMediaSource(tempFile), maxWidth, maxHeight, borders, format);
        }
      }

      // We need to impersonate to access the network drive and check if the thumbnail already exists. However, because
      // impersonation is lost when starting a process, we can't start ffmpeg from in here. We need to check whether the 
      // file is accessible from outside the impersonation context again, and start the ffmpeg process as a different user 
      // if that is the case.
      string fullPath;
      using (var context = source.CreateNetworkContext())
      {
        fullPath = context.RewritePath(source.GetPath());

        // stream a pre-existing thumbnail, if possible
        if (source.MediaType == WebMediaType.Recording && Path.GetExtension(fullPath).ToLower() == ".ts")
        {
          var thumbnailFileInfo = new FileInfo(Path.ChangeExtension(fullPath, ".jpg"));
          if (thumbnailFileInfo.Exists && thumbnailFileInfo.Length > 0)
            return StreamPostprocessedImage(new ImageMediaSource(thumbnailFileInfo.FullName), maxWidth, maxHeight, borders, format);
        }
      }

      // finally, extract the image with ffmpeg if everything else has failed
      bool extractResult = ExecuteFFMpegExtraction(source, fullPath, position, tempFile);
      if (!extractResult)
      {
        WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
        return Stream.Null;
      }

      return StreamPostprocessedImage(new ImageMediaSource(tempFile), maxWidth, maxHeight, borders, format);
    }

    private static bool ExecuteFFMpegExtraction(MediaSource source, string path, long position, string tempFile)
    {
      var info = new ProcessStartInfo();
      info.Arguments = String.Format("-ss {0} -i \"{1}\" -vframes 1 -vf \"yadif,scale=ih*dar:ih\" -f image2 \"{2}\"", position, path, tempFile);
      info.FileName = Configuration.StreamingProfiles.FFMpegPath;
      info.CreateNoWindow = true;
      info.UseShellExecute = false;
      info.RedirectStandardError = Log.IsEnabled(LogLevel.Trace);
      info.RedirectStandardOutput = Log.IsEnabled(LogLevel.Trace);

      TranscoderProcess proc = new TranscoderProcess();
      proc.StartInfo = info;
      if (source.NeedsImpersonation)
        proc.StartAsUser(Configuration.Services.NetworkImpersonation.Domain, Configuration.Services.NetworkImpersonation.Username, Configuration.Services.NetworkImpersonation.GetPassword());
      else
        proc.Start();

      if (Log.IsEnabled(LogLevel.Trace))
      {
        Log.Trace("ExtractImage: executing path={0} arguments={1}", info.FileName, info.Arguments);
        StreamCopy.AsyncStreamRead(proc.StandardError, l => Log.Trace("ExtractImage: stderr: {0}", l));
        StreamCopy.AsyncStreamRead(proc.StandardOutput, l => Log.Trace("ExtractImage: stdout: {0}", l));
      }
      proc.WaitForExit();

      if (!File.Exists(tempFile))
      {
        Log.Warn("Failed to extract image from {0} with {1} {2}", source.GetDebugName(), info.FileName, info.Arguments);
        return false;
      }

      return true;
    }

    public static Stream GetImage(ImageMediaSource src, string format)
    {
      return StreamPostprocessedImage(src, null, null, null, format);
    }

    public static Stream GetResizedImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
    {
      return StreamPostprocessedImage(src, maxWidth, maxHeight, borders, format);
    }

    public static bool CacheImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
    {
      if (!src.Exists)
      {
        Log.Info("Tried to request image from non-existing source {0}", src.GetDebugName());
        return false;
      }

      if (borders != null && (!maxWidth.HasValue || !maxHeight.HasValue))
      {
        Log.Error("RequestImage() called with a borders value but width or height is null");
        return false;
      }

      if (format == null)
        format = src.Extension.Substring(1);

      return ProcessAndCacheImage(src, maxWidth, maxHeight, borders, format) != null;
    }

    private static Stream StreamPostprocessedImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
    {
      if (!src.Exists)
      {
        Log.Info("Tried to stream image from non-existing source {0}", src.GetDebugName());
        WCFUtil.SetResponseCode(HttpStatusCode.NotFound);
        return Stream.Null;
      }

      if (borders != null && (!maxWidth.HasValue || !maxHeight.HasValue))
      {
        Log.Error("ResizeImage() called with a borders value but width or height is null");
        WCFUtil.SetResponseCode(HttpStatusCode.BadRequest);
        return Stream.Null;
      }

      if (format == null)
        format = src.Extension.Substring(1);

      Func<Stream> streamFactory = ProcessAndCacheImage(src, maxWidth, maxHeight, borders, format);
      if (streamFactory == null)
        return Stream.Null;

      try
      {
        // return image to client
        WCFUtil.AddHeader(HttpResponseHeader.CacheControl, "public, max-age=5184000, s-maxage=5184000");
        WCFUtil.SetContentType(GetMime(format));
        return streamFactory();
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to post-process and stream image {0}", src.GetDebugName()), ex);
        return Stream.Null;
      }
    }

    private static Func<Stream> ProcessAndCacheImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
    {
      // if it's already in the cache, use that file
      string filename = String.Format("stream_{0}_{1}_{2}_{3}.{4}", src.GetUniqueIdentifier(), maxWidth, maxHeight, borders, format);
      if (cache.Contains(filename))
      {
        if (src.GetFileInfo().LastModifiedTime > cache.GetLastModifiedTime(cache.GetPath(filename)))
        {
          cache.Invalidate(filename);
        }
        else
        {
          return () => new FileStream(cache.GetPath(filename), FileMode.Open, FileAccess.Read, FileShare.Read);
        }
      }

      // otherwise, resize if needed and save to cache
      try
      {
        bool hasToResize = maxWidth.HasValue || maxHeight.HasValue;
        bool hasToRecode = format != src.Extension.Substring(1);

        if (!hasToResize && !hasToRecode)
          return () => src.Retrieve();

        // process and save to cache
        using (var stream = src.Retrieve())
        {
          var image = hasToResize ? ResizeImage(stream, maxWidth, maxHeight, borders) : Image.FromStream(stream);
          SaveImageToFile(image, cache.GetPath(filename), format);
          image.Dispose();
        }

        return () => new FileStream(cache.GetPath(filename), FileMode.Open, FileAccess.Read, FileShare.Read);
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to process and cache image {0}", src.GetDebugName()), ex);
        return null;
      }
    }

    private static string GetMime(string extension)
    {
      string lowerExtension = (extension.StartsWith(".") ? extension.Substring(1) : extension).ToLower();
      Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>()
            {
                { "jpeg", "image/jpeg" },
                { "jpg", "image/jpeg" },
                { "png", "image/png" },
                { "gif", "image/gif" },
                { "bmp", "image/x-ms-bmp" },
            };
      return commonMimeTypes.ContainsKey(lowerExtension) ? commonMimeTypes[lowerExtension] : "application/octet-stream";
    }

    private static Image ResizeImage(Stream stream, int? maxWidth, int? maxHeight, string borders)
    {
      using (var origImage = Image.FromStream(stream))
      {
        // newImageSize is the size of the actual graphic, which might not be the size of the canvas (bitmap). Unless
        // we're instructed to stretch the image, it's a proportional resize of the source graphic. Borders will be
        // added when we're instructed to and the aspect ratio of the image isn't equal to the aspect ratio of the 
        // bitmap.
        Resolution newImageSize = borders != "stretch" ?
            Resolution.Calculate(origImage.Width, origImage.Height, maxWidth, maxHeight, 1) :
            Resolution.Create(maxWidth.Value, maxHeight.Value);
        bool addBorders = !String.IsNullOrEmpty(borders) && borders != "stretch" && newImageSize.AspectRatio != (double)maxWidth / maxHeight;

        Resolution bitmapSize = addBorders ? Resolution.Create(maxWidth.Value, maxHeight.Value) : newImageSize;
        Bitmap newImage = new Bitmap(bitmapSize.Width, bitmapSize.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphic = Graphics.FromImage(newImage))
        {
          graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
          graphic.SmoothingMode = SmoothingMode.HighQuality;
          graphic.CompositingQuality = CompositingQuality.HighQuality;
          graphic.CompositingMode = CompositingMode.SourceCopy;
          graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

          if (addBorders && borders != "transparent")
            graphic.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#" + borders)), 0, 0, bitmapSize.Width, bitmapSize.Height);

          // We center the graphic in the canvas. If we should stretch the image, newImageSize is equal to the canvas, so 
          // the graphic is pasted at the top-left corner, which is fine.
          int leftOffset = (bitmapSize.Width - newImageSize.Width) / 2;
          int heightOffset = (bitmapSize.Height - newImageSize.Height) / 2;
          graphic.DrawImage(origImage, leftOffset, heightOffset, newImageSize.Width, newImageSize.Height);
        }

        return newImage;
      }
    }

    private static void SaveImageToFile(Image image, string path, string format)
    {
      switch (format.ToLower())
      {
        case "png":
          image.Save(path, ImageFormat.Png);
          break;

        case "gif":
          image.Save(path, ImageFormat.Gif);
          break;

        case "bmp":
          image.Save(path, ImageFormat.Bmp);
          break;

        case "jpeg":
        case "jpg":
          var jpegInfo = ImageCodecInfo.GetImageEncoders().First(enc => enc.FormatID == ImageFormat.Jpeg.Guid);
          var jpegParameters = new EncoderParameters(1);
          jpegParameters.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
          image.Save(path, jpegInfo, jpegParameters);
          break;

        default:
          Log.Warn("Requested invalid file format '{0}'", format);
          throw new ArgumentException(String.Format("Invalid file format '{0}'", format));
      }
    }

  }
}
