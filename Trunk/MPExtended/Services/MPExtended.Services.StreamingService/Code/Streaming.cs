using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Streaming
    {
        private static Dictionary<string, ActiveStream> Streams = new Dictionary<string, ActiveStream>();

        private class ActiveStream
        {
            public string Identifier { get; set; }
            public string ClientDescription { get; set; }
            public TranscoderProfile Profile { get; set; }

            public string Source { get; set; }
            public bool IsTsBuffer { get; set; }
            public Resolution OutputSize { get; set; }

            public Pipeline Pipeline { get; set; }
            public EncodingInfo EncodingInfo { get; set; }
        }

        public bool InitStream(string identifier, string clientDescription, string source)
        {
            ActiveStream stream = new ActiveStream();
            stream.Identifier = identifier;
            stream.ClientDescription = clientDescription;
            stream.Source = source;
            stream.IsTsBuffer = source.EndsWith(".ts.tsbuffer");

            Streams[identifier] = stream;
            return true;
        }

        public bool StartStream(string identifier, TranscoderProfile profile, int position = 0, string audioLanguage = null)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null)
            {
                Log.Warn("Stream requested for invalid identifier {0}", identifier);
                return false;
            }

            try
            {
                ActiveStream stream = Streams[identifier];
                stream.Profile = profile;
                stream.OutputSize = CalculateSize(stream.Profile, stream.Source, stream.IsTsBuffer);
                Log.Trace("Using {0} as output size for stream {1}", stream.OutputSize, identifier);

                // calculate stream mappings
                string mappings = "";
                if(!String.IsNullOrEmpty(audioLanguage)) {
                    // get media info
                    WebMediaInfo info;
                    if (stream.IsTsBuffer)
                    {
                        info = MediaInfoWrapper.GetMediaInfo(new TsBuffer(stream.Source));
                    }
                    else
                    {
                        info = MediaInfoWrapper.GetMediaInfo(stream.Source);
                    }
                    mappings = String.Format("-map 0:{0} ", info.VideoStreams.First().Index);

                    // audio streams (select first one if nothing found for selected language)
                    IEnumerable<WebAudioStream> validStreams = info.AudioStreams.Where(a => a.Language == audioLanguage);
                    WebAudioStream astream = validStreams.Count() == 0 ? info.AudioStreams.First() : validStreams.First();
                    mappings += String.Format("-map 0:{0} ", astream.Index);
                }

                // setup input of rendering pipeline
                stream.Pipeline = new Pipeline();

                // the input reader
                bool hasInputUnit = stream.IsTsBuffer || !profile.UseTranscoding;
                if (hasInputUnit)
                {
                    stream.Pipeline.AddDataUnit(new InputUnit(stream.Source), 1);
                }

                if (profile.UseTranscoding)
                {
                    // setup ffmpeg parameters
                    string arguments;
                    if (profile.HasVideoStream)
                    {
                        arguments = String.Format(
                            "-y {0} -i \"#IN#\" -s {1} -aspect {2}:{3} {4} {5} \"#OUT#\"",
                            position != 0 ? "-ss " + position : "",
                            stream.OutputSize, stream.OutputSize.Width, stream.OutputSize.Height,
                            mappings, stream.Profile.CodecParameters
                        );
                    }
                    else
                    {
                        arguments = String.Format(
                            "-y {0} -i \"#IN#\" {1} {2} \"#OUT#\"",
                            position != 0 ? "-ss " + position : "",
                            mappings, stream.Profile.CodecParameters
                        );
                    }

                    // setup input and output
                    TransportMethod prefered = hasInputUnit ? TransportMethod.NamedPipe : TransportMethod.Filename;
                    TransportMethod input = stream.Profile.InputMethod != TransportMethod.Path ? stream.Profile.InputMethod : prefered;
                    TransportMethod output = stream.Profile.OutputMethod != TransportMethod.Path ? stream.Profile.InputMethod : TransportMethod.NamedPipe;
                    EncoderUnit encoder = new EncoderUnit(stream.Profile.Transcoder, arguments, input, output);
                    encoder.Source = stream.Source; // setting it when we don't need it doesn't hurt
                    encoder.DebugOutput = false;
                    stream.Pipeline.AddDataUnit(encoder, 5);

                    // setup output parsing
                    stream.EncodingInfo = new EncodingInfo();
                    Reference<EncodingInfo> eref = new Reference<EncodingInfo>(() => stream.EncodingInfo, x => { stream.EncodingInfo = x; });
                    FFMpegLogParsing unit = new FFMpegLogParsing(eref);
                    unit.LogMessages = true;
                    unit.LogProgress = false;
                    stream.Pipeline.AddLogUnit(unit, 6);
                }

                // start the processes
                stream.Pipeline.Assemble();
                stream.Pipeline.Start();

                Log.Info("Started stream with identifier " + identifier);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start stream " + identifier, ex);
                return false;
            }
        }

        public Stream RetrieveStream(string identifier)
        {
            return Streams[identifier].Pipeline.GetFinalStream();
        }

        public void EndStream(string identifier)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null || Streams[identifier].Pipeline == null || !Streams[identifier].Pipeline.IsStarted)
                return;

            try
            {
                Log.Debug("Stopping stream with identifier " + identifier);
                Streams[identifier].Pipeline.Stop();
                Streams[identifier].Pipeline = null;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to stop stream " + identifier, ex);
            }
        }

        public void KillStream(string identifier)
        {
            EndStream(identifier);
            Streams.Remove(identifier);
            Log.Debug("Killed stream with identifier {0}", identifier);
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return Streams.Select(s => s.Value).Select(s => new WebStreamingSession()
            {
                ClientDescription = s.ClientDescription,
                Identifier = s.Identifier,
                SourceFile = s.Source,
                Profile = s.Profile != null ? s.Profile.Name : null,
                TranscodingInfo = s.EncodingInfo != null ? s.EncodingInfo.ToWebTranscodingInfo() : null
            }).ToList();
        }

        public EncodingInfo GetEncodingInfo(string identifier) 
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
                return Streams[identifier].EncodingInfo;

            return null;
        }

        public Resolution CalculateSize(TranscoderProfile profile, string file, bool isTsBuffer = false)
        {
            if (!profile.HasVideoStream)
                return new Resolution(0, 0);

            decimal aspect;
            if (isTsBuffer)
            {
                // FIXME: we might want to support TV with other aspect ratios
                aspect = (decimal)16 / 9;
            }
            else
            {
                WebMediaInfo info = MediaInfoWrapper.GetMediaInfo(file);
                aspect = info.VideoStreams.First().DisplayAspectRatio;
            }
            return Resolution.Calculate(aspect, new Resolution(profile.MaxOutputWidth, profile.MaxOutputHeight), 2);
        }
    }
}
