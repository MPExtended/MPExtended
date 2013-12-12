//  Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Additional fixes from http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/

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

namespace Microsoft.ServiceModel.Samples
{
    /*
     * Carries the gzip context around a request / response
     */
    public class GzipContext : IExtension<OperationContext>
    {
        public void Attach(OperationContext owner)
        {
            //Log.Debug(GetType().FullName + "::Attach " + owner.GetType().FullName);
        }

        public void Detach(OperationContext owner)
        {
            //Log.Debug(GetType().FullName + "::Detach " + owner.GetType().FullName);
        }
    }

    //This class is used to create the custom encoder (GZipMessageEncoder)
    internal class GZipMessageEncoderFactory : MessageEncoderFactory
    {
        MessageEncoder encoder;

        //The GZip encoder wraps an inner encoder
        //We require a factory to be passed in that will create this inner encoder
        public GZipMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory)
        {
            if (messageEncoderFactory == null)
                throw new ArgumentNullException("messageEncoderFactory", "A valid message encoder factory must be passed to the GZipEncoder");
            //Log.Debug(GetType().FullName + "::ctor with " + messageEncoderFactory.GetType().FullName);

            encoder = new GZipMessageEncoder(messageEncoderFactory.Encoder);
        }

        //The service framework uses this property to obtain an encoder from this encoder factory
        public override MessageEncoder Encoder
        {
            get {
				//Log.Debug(GetType().FullName + "::Encoder, " + encoder.GetType().FullName);
				return encoder;
			}
        }

        public override MessageVersion MessageVersion
        {
            get {
				//Log.Debug(GetType().FullName + "::MessageVersion, " + encoder.MessageVersion);
				return encoder.MessageVersion;
			}
        }

        //This is the actual GZip encoder
        class GZipMessageEncoder : MessageEncoder
        {
            //This implementation wraps an inner encoder that actually converts a WCF Message
            //into textual XML, binary XML or some other format. This implementation then compresses the results.
            //The opposite happens when reading messages.
            //This member stores this inner encoder.
            MessageEncoder innerEncoder;

            //We require an inner encoder to be supplied (see comment above)
            internal GZipMessageEncoder(MessageEncoder messageEncoder)
                : base()
            {
                if (messageEncoder == null)
                    throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to the GZipEncoder");
                //Log.Debug(GetType().FullName + "::ctor with " + messageEncoder.GetType().FullName);
                innerEncoder = messageEncoder;
            }

            //public override string CharSet
            //{
            //    get { return ""; }
            //}

            public override string ContentType
            {
                get {
					//Log.Debug(GetType().FullName + "::ContentType, " + innerEncoder.ContentType);
					return innerEncoder.ContentType;
				}
            }

            public override string MediaType
            {
                get {
					//Log.Debug(GetType().FullName + "::MediaType, " + innerEncoder.MediaType);
					return innerEncoder.MediaType;
				}
            }

            //SOAP version to use - we delegate to the inner encoder for this
            public override MessageVersion MessageVersion
            {
                get {
					//Log.Debug(GetType().FullName + "::MessageVersion, " + innerEncoder.MessageVersion);
					return innerEncoder.MessageVersion;
				}
            }

            public override T GetProperty<T>()
            {
                //Log.Debug(GetType().FullName + "::GetProperty<" + (typeof(T) != null ? typeof(T).GetType().FullName : null) + ">");
                return innerEncoder.GetProperty<T>();
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                //Log.Debug(GetType().FullName + "::IsContentTypeSupported with " + contentType);
                return innerEncoder.IsContentTypeSupported(contentType);
            }

            //Helper method to compress an array of bytes
            static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset)
            {
                //Log.Debug("CompressBuffer with " + buffer);

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(buffer.Array, 0, messageOffset);

                using (GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzStream.Write(buffer.Array, messageOffset, buffer.Count);
                }


                byte[] compressedBytes = memoryStream.ToArray();
                byte[] bufferedBytes = bufferManager.TakeBuffer(compressedBytes.Length);

                Array.Copy(compressedBytes, 0, bufferedBytes, 0, compressedBytes.Length);

                bufferManager.ReturnBuffer(buffer.Array);
                ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, compressedBytes.Length);

                //Log.Debug("Compressed " + buffer.Array.Length + " bytes to " + byteArray.Array.Length);

                return byteArray;
            }

            //Helper method to decompress an array of bytes
            static ArraySegment<byte> DecompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
            {
                //Log.Debug("DecompressBuffer with " + buffer); 

                MemoryStream memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count - buffer.Offset);
                MemoryStream decompressedStream = new MemoryStream();
                int totalRead = 0;
                int blockSize = 1024;
                byte[] tempBuffer = bufferManager.TakeBuffer(blockSize);
                using (GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    while (true)
                    {
                        int bytesRead = gzStream.Read(tempBuffer, 0, blockSize);
                        if (bytesRead == 0)
                            break;
                        decompressedStream.Write(tempBuffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }
                bufferManager.ReturnBuffer(tempBuffer);

                byte[] decompressedBytes = decompressedStream.ToArray();
                byte[] bufferManagerBuffer = bufferManager.TakeBuffer(decompressedBytes.Length + buffer.Offset);
                Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);
                Array.Copy(decompressedBytes, 0, bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);

                ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);
                bufferManager.ReturnBuffer(buffer.Array);

                //Log.Debug("Decompressed " + buffer.Array.Length + " bytes to " + byteArray.Array.Length);

                return byteArray;
            }

            //One of the two main entry points into the encoder. Called by WCF to decode a buffered byte array into a Message.
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                //Log.Debug(GetType().FullName + "::ReadMessage with " + buffer + " " + contentType);
                return innerEncoder.ReadMessage(buffer, bufferManager, contentType);
            }

            //One of the two main entry points into the encoder. Called by WCF to encode a Message into a buffered byte array.
            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                //Log.Debug(GetType().FullName + "::WriteMessage with " + message.GetType().FullName);
                if (OperationContext.Current.Extensions.Find<GzipContext>() != null)
                {
					//Use the inner encoder to encode a Message into a buffered byte array
                    ArraySegment<byte> buffer = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                	//Compress the resulting byte array
                    ArraySegment<byte> compressedBuffer = CompressBuffer(buffer, bufferManager, messageOffset);
                    Log.Debug(GetType().FullName + "::WriteMessage compressed " + buffer.Array.Length + " bytes to " + compressedBuffer.Array.Length);

                    return compressedBuffer;
                }
                else
				{
                    return innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
				}
            }

            public override Message ReadMessage(System.IO.Stream stream, int maxSizeOfHeaders, string contentType)
            {
                //Log.Debug(GetType().FullName + "::ReadMessage with " + stream.GetType().FullName + " " + contentType);
                return innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
            }

            public override void WriteMessage(Message message, System.IO.Stream stream)
            {
                //Log.Debug(GetType().FullName + "::WriteMessage with " + message.GetType().FullName + " " + stream.GetType().FullName);
                if (OperationContext.Current.Extensions.Find<GzipContext>() != null)
                {
                    using (GZipStream gzStream = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        innerEncoder.WriteMessage(message, gzStream);
                    }

	                // innerEncoder.WriteMessage(message, gzStream) depends on that it can flush data by flushing 
	                // the stream passed in, but the implementation of GZipStream.Flush will not flush underlying
	                // stream, so we need to flush here.
                    stream.Flush();
                }
                else
                    innerEncoder.WriteMessage(message, stream);
            }
        }
    }
}
