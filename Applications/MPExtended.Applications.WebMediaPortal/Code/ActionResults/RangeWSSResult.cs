﻿using System;
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
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code.ActionResults
{
    public class RangeWSSResult : RangeFileResult
    {
        #region Fields
        private const int _bufferSize = 100000;
        private String _itemId;
        private String _clientDesc;
        private WebMediaType _type;
        private int? _provider;
        #endregion

        public RangeWSSResult(WebRecordingFileInfo fileInfo, string clientDescription, WebMediaType type, int? provider, string itemId)
            : base(MIME.GetFromFilename(fileInfo.Path, "application/octet-stream"), fileInfo.Path, fileInfo.LastModifiedTime, fileInfo.Size, fileInfo.Name)
        {
            _itemId = itemId;
            _clientDesc = clientDescription;
            _provider = provider;
            _type = type;
        }

        public RangeWSSResult(WebFileInfo fileInfo, string clientDescription, WebMediaType type, int? provider, string itemId)
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
            WriteEntityRange(response, 0, FileLength);
        }

        /// <summary>
        /// Writes the file range to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        /// <param name="rangeStartIndex">Range start index</param>
        /// <param name="rangeEndIndex">Range end index</param>
        protected override void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex)
        {
            try
            {
                Log.Debug("WriteEntityRange: {0} - {1}", rangeStartIndex, rangeEndIndex);
                response.BufferOutput = false;

                string address = _type == WebMediaType.TV || _type == WebMediaType.Recording ? Connections.Current.Addresses.TAS : Connections.Current.Addresses.MAS;
                String fullUrl = String.Format("http://{0}/MPExtended/StreamingService/stream/GetMediaItem?" +
                    "clientDescription={1}&type={2}&provider={3}&itemId={4}&startPosition={5}",
                    address, _clientDesc, (int)_type, _provider, _itemId, rangeStartIndex);
                WebRequest req = WebRequest.Create(fullUrl);
                WebResponse source = req.GetResponse();
                Stream stream = source.GetResponseStream();

                int bytesRemaining = Convert.ToInt32(rangeEndIndex - rangeStartIndex) + 1;
                byte[] buffer = new byte[_bufferSize];

                while (bytesRemaining > 0 && response.IsClientConnected)
                {
                    int bytesRead = stream.Read(buffer, 0, _bufferSize < bytesRemaining ? _bufferSize : bytesRemaining);
                    response.OutputStream.Write(buffer, 0, bytesRead);
                    bytesRemaining -= bytesRead;
                }

                stream.Close();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warn("Error in WriteEntityRange: " + ex.Message);
            }
        }
        #endregion

    }
}
