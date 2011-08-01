using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal enum TransportMethod
    {
        Filename,       // always a full path to a file
        NamedPipe,      // always a named pipe (all data passes through MPWebStream)
        Path,           // dynamically dispatch between Filename and NamedPipe (recommended)
        StandardIn,     // only input: is written to standard input of transcoder
        StandardOut,    // only output: is read from standard output of transcoder
        External        // don't touch input or output (i.e. hardcoded in transcoder config)
    }

    internal class TranscoderProfile
    {
        public string Name { get; set; }
        public bool UseTranscoding { get; set; }
        public bool HasVideoStream { get; set; }
        public string Transcoder { get; set; }
        public TransportMethod InputMethod { get; set; }
        public TransportMethod OutputMethod { get; set; }
        public string MIME { get; set; }
        public int MaxOutputWidth { get; set; }
        public int MaxOutputHeight { get; set; }
        public string Description { get; set; }
        public string CodecParameters { get; set; }
        public string Target { get; set; }
        public string Bandwidth { get; set; }

        public WebTranscoderProfile ToWebTranscoderProfile()
        {
            return new WebTranscoderProfile()
            {
                Name = this.Name,
                Description = this.Description,
                MIME = this.MIME,
                MaxOutputWidth = this.MaxOutputWidth,
                MaxOutputHeight = this.MaxOutputHeight,
                UseTranscoding = this.UseTranscoding,
                Target = this.Target,
                Bandwidth = this.Bandwidth,
                HasVideoStream = this.HasVideoStream
            };
        }

        public static TranscoderProfile CreateFromXmlNode(XmlNode node)
        {
            TranscoderProfile transcoder = new TranscoderProfile();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "name") transcoder.Name = child.InnerText;
                if (child.Name == "useTranscoding") transcoder.UseTranscoding = child.InnerText == "true";
                if (child.Name == "inputMethod") transcoder.InputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                if (child.Name == "outputMethod") transcoder.OutputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                if (child.Name == "transcoder") transcoder.Transcoder = child.InnerText;
                if (child.Name == "mime") transcoder.MIME = child.InnerText;
                if (child.Name == "maxOutputWidth") transcoder.MaxOutputWidth = Int32.Parse(child.InnerText);
                if (child.Name == "maxOutputHeight") transcoder.MaxOutputHeight = Int32.Parse(child.InnerText);
                if (child.Name == "description") transcoder.Description = child.InnerText;
                if (child.Name == "codecParameters") transcoder.CodecParameters = child.InnerText;
                if (child.Name == "target") transcoder.Target = child.InnerText;
                if (child.Name == "bandwidth") transcoder.Bandwidth = child.InnerText;
                if (child.Name == "videoStream") transcoder.HasVideoStream = child.InnerText == "true";
            }
            return transcoder;
        }
    }

    internal static class Profiles
    {
        public static TranscoderProfile GetTranscoderProfileByName(string name)
        {
            return GetTranscoderProfiles().Where(s => s.Name == name).FirstOrDefault();
        }

        public static List<TranscoderProfile> GetTranscoderProfiles()
        {
            List<TranscoderProfile> list = new List<TranscoderProfile>();
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
            XmlNodeList nodes = doc.SelectNodes("/appconfig/transcoders/transcoder");
            foreach (XmlNode node in nodes)
            {
                TranscoderProfile transcoder = new TranscoderProfile();
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "name") transcoder.Name = child.InnerText;
                    if (child.Name == "useTranscoding") transcoder.UseTranscoding = child.InnerText == "true";
                    if (child.Name == "inputMethod") transcoder.InputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                    if (child.Name == "outputMethod") transcoder.OutputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                    if (child.Name == "transcoder") transcoder.Transcoder = child.InnerText;
                    if (child.Name == "mime") transcoder.MIME = child.InnerText;
                    if (child.Name == "maxOutputWidth") transcoder.MaxOutputWidth = Int32.Parse(child.InnerText);
                    if (child.Name == "maxOutputHeight") transcoder.MaxOutputHeight = Int32.Parse(child.InnerText);
                    if (child.Name == "description") transcoder.Description = child.InnerText;
                    if (child.Name == "codecParameters") transcoder.CodecParameters = child.InnerText;
                    if (child.Name == "target") transcoder.Target = child.InnerText;
                    if (child.Name == "bandwidth") transcoder.Bandwidth = child.InnerText;
                }

                list.Add(transcoder);
            }

            return list;
        }
    }
}
