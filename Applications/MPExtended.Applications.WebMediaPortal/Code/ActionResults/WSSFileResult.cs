using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using MPExtended.Services.Common.Interfaces;
using System.Net;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Code.ActionResults
{
    public class WSSFileResult : RangeFileResult
    {
        #region Fields
        private const int _bufferSize = 0x1000;
        private String _itemId;
        private String _clientDesc;
        private WebMediaType _type;
        private int? _provider;
        #endregion

        public WSSFileResult(WebRecordingFileInfo fileInfo, string clientDescription, WebMediaType type, int? provider, string itemId)
            : base(MIME.GetFromFilename(fileInfo.Path, "application/octet-stream"), fileInfo.Path, fileInfo.LastModifiedTime, fileInfo.Size, fileInfo.Name)
        {
            _itemId = itemId;
            _clientDesc = clientDescription;
            _provider = provider;
            _type = type;
        }

            public WSSFileResult(WebFileInfo fileInfo, string clientDescription, WebMediaType type, int? provider, string itemId)
            : base(MIME.GetFromFilename(fileInfo.Path, "application/octet-stream"), fileInfo.Path, fileInfo.LastModifiedTime, fileInfo.Size, fileInfo.Name)
        {
            _itemId = itemId;
            _clientDesc = clientDescription;
            _provider = provider;
            _type = type;
        }

        #region Methods
        /// <summary>
        /// Writes the entire file to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        protected override void WriteEntireEntity(HttpResponseBase response)
        {
            string address = _type == WebMediaType.TV || _type == WebMediaType.Recording ? Connections.Current.Addresses.TAS : Connections.Current.Addresses.MAS;
            String fullUrl = String.Format("http://{0}/MPExtended/StreamingService/stream/GetMediaItem?" +
                "clientDescription={1}&type={2}&provider={3}&itemId={4}&startPosition={5}",
                address, _clientDesc, (int)_type, _provider, _itemId, 0);
            WebRequest req = WebRequest.Create(fullUrl);
            WebResponse source = req.GetResponse();
            using (Stream stream = source.GetResponseStream())
            {
                byte[] buffer = new byte[_bufferSize];
                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, _bufferSize);
                    response.OutputStream.Write(buffer, 0, bytesRead);
                }

                stream.Close();
            }
        }

        /// <summary>
        /// Writes the file range to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        /// <param name="rangeStartIndex">Range start index</param>
        /// <param name="rangeEndIndex">Range end index</param>
        protected override void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex)
        {
            string address = _type == WebMediaType.TV || _type == WebMediaType.Recording ? Connections.Current.Addresses.TAS : Connections.Current.Addresses.MAS;
            String fullUrl = String.Format("http://{0}/MPExtended/StreamingService/stream/GetMediaItem?" +
                "clientDescription={1}&type={2}&provider={3}&itemId={4}&startPosition={5}",
                address, _clientDesc, (int)_type, _provider, _itemId, rangeStartIndex);
            WebRequest req = WebRequest.Create(fullUrl);
            WebResponse source = req.GetResponse();
            using (Stream stream = source.GetResponseStream())
            {
                int bytesRemaining = Convert.ToInt32(rangeEndIndex - rangeStartIndex) + 1;
                byte[] buffer = new byte[_bufferSize];

                while (bytesRemaining > 0)
                {
                    int bytesRead = stream.Read(buffer, 0, _bufferSize < bytesRemaining ? _bufferSize : bytesRemaining);
                    response.OutputStream.Write(buffer, 0, bytesRead);
                    bytesRemaining -= bytesRead;
                }

                stream.Close();
            }
        }
        #endregion

    }
}
