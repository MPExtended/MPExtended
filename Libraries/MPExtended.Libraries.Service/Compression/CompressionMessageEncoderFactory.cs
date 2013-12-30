#region Copyright (C) 2013 MPExtended, Microsoft Corporation
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) Microsoft Corporation, http://www.microsoft.com/
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
//
// Based upon original assumed Public Domain (since a license declaration
// is missing) code from Microsoft Corporation, obtained from
// http://go.microsoft.com/fwlink/?LinkId=150780.
// Additional fixes based on code from Francesco De Vittori, 
// http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Xml;
using System.ServiceModel.Security;
using System.ServiceModel.Description;
using MPExtended.Libraries.Service;
using System.Net;

namespace MPExtended.Libraries.Service.Compression
{
    public enum CompressionType
    {
        None,
        GZip,
        Deflate,
    }

    /*
     * Carries the compression context around a request / response
     * 
     * This extension is added to the current OperationContext by a message interceptor
     * as part of compression behaviour. It needs to be on a per request basis
     * because each client making the request will be different
     * (some will accept gzip and / or deflate encoding, others nothing)
     */
    public class CompressionContext : IExtension<OperationContext>
    {
        public CompressionType Type { get; set; }

        public CompressionContext(CompressionType type)
        {
            Type = type;
        }

        public void Attach(OperationContext owner) { }

        public void Detach(OperationContext owner) { }
    }

    //This class is used to create the custom encoder
    internal class CompressionMessageEncoderFactory : MessageEncoderFactory
    {
        private MessageEncoder encoder;

        //The compression encoder wraps an inner encoder
        //We require a factory to be passed in that will create this inner encoder
        public CompressionMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory)
        {
            if (messageEncoderFactory == null)
                throw new ArgumentNullException("messageEncoderFactory", "A valid message encoder factory must be passed to the CompressionEncoder");

            encoder = new CompressionMessageEncoder(messageEncoderFactory.Encoder);
        }

        //The service framework uses this property to obtain an encoder from this encoder factory
        public override MessageEncoder Encoder
        {
            get { return encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return encoder.MessageVersion; }
        }

        //This is the actual encoder
        private class CompressionMessageEncoder : MessageEncoder
        {
            //This implementation wraps an inner encoder that actually converts a WCF Message
            //into textual XML, binary XML or some other format. This implementation then compresses the results.
            //The opposite happens when reading messages.
            //This member stores this inner encoder.
            private MessageEncoder innerEncoder;

            //We require an inner encoder to be supplied (see comment above)
            internal CompressionMessageEncoder(MessageEncoder messageEncoder)
                : base()
            {
                if (messageEncoder == null)
                    throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to the CompressionEncoder");
                innerEncoder = messageEncoder;
            }

            public override string ContentType
            {
                get
                {
                    return innerEncoder.ContentType;
                }
            }

            public override string MediaType
            {
                get { return innerEncoder.MediaType; }
            }

            //SOAP version to use - we delegate to the inner encoder for this
            public override MessageVersion MessageVersion
            {
                get { return innerEncoder.MessageVersion; }
            }

            public override T GetProperty<T>()
            {
                return innerEncoder.GetProperty<T>();
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                return innerEncoder.IsContentTypeSupported(contentType);
            }

            //Helper method to compress an array of bytes
            private static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset, CompressionType type)
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(buffer.Array, 0, messageOffset);

                Stream outerStream =
                    type == CompressionType.GZip ? (Stream)new GZipStream(memoryStream, CompressionMode.Compress, true) :
                    type == CompressionType.Deflate ? (Stream)new DeflateStream(memoryStream, CompressionMode.Compress, true) :
                    null;
                if (outerStream != null)
                {
                    outerStream.Write(buffer.Array, messageOffset, buffer.Count);
                    outerStream.Dispose();
                }

                byte[] compressedBytes = memoryStream.ToArray();
                byte[] bufferedBytes = bufferManager.TakeBuffer(compressedBytes.Length);

                Array.Copy(compressedBytes, 0, bufferedBytes, 0, compressedBytes.Length);

                bufferManager.ReturnBuffer(buffer.Array);
                ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, compressedBytes.Length);

                return byteArray;
            }

            //One of the two main entry points into the encoder. Called by WCF to decode a buffered byte array into a Message.
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                return innerEncoder.ReadMessage(buffer, bufferManager, contentType);
            }

            //One of the two main entry points into the encoder. Called by WCF to encode a Message into a buffered byte array.
            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                CompressionContext context = OperationContext.Current.Extensions.Find<CompressionContext>();
                if (context != null)
                {
                    //Use the inner encoder to encode a Message into a buffered byte array
                    ArraySegment<byte> buffer = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                    //Compress the resulting byte array
                    ArraySegment<byte> compressedBuffer = CompressBuffer(buffer, bufferManager, messageOffset, context.Type);
                    Log.Trace("CompressionMessageEncoder::WriteMessage {0} compressed {1} bytes to {2}", context.Type, buffer.Array.Length, compressedBuffer.Array.Length);

                    return compressedBuffer;
                }
                else
                {
                    return innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                }
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                return innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                CompressionContext context = OperationContext.Current.Extensions.Find<CompressionContext>();
                if (context != null)
                {
                    Stream outerStream =
                        context.Type == CompressionType.GZip ? (Stream)new GZipStream(stream, CompressionMode.Compress, true) :
                        context.Type == CompressionType.Deflate ? (Stream)new DeflateStream(stream, CompressionMode.Compress, true) :
                        null;
                    if (outerStream != null)
                    {
                        innerEncoder.WriteMessage(message, outerStream);
                        outerStream.Dispose();
                    }

                    // innerEncoder.WriteMessage(message, gzStream) depends on that it can flush data by flushing 
                    // the stream passed in, but the implementation of the compression stream's Flush will not
                    // flush underlying stream, so we need to flush here.
                    stream.Flush();
                }
                else
                    innerEncoder.WriteMessage(message, stream);
            }
        }
    }
}
