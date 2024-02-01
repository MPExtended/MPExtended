#region Copyright (C) 2013 TPeczek, 2013 MPExtended (Ms-PL)
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.io/
// Copyright (C) 2013 TPeczek, http://tpeczek.codeplex.com/
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code.ActionResults
{
    /// <summary>
    /// Sends the contents of a file to the range response.
    /// </summary>
    public class RangeFilePathResult : RangeFileResult
    {
        #region Fields
        private const int _bufferSize = 100000;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the RangeFilePathResult class.
        /// </summary>
        /// <param name="contentType">The content type to use for the response.</param>
        /// <param name="fileName">The file name to use for the response.</param>
        /// <param name="modificationDate">The file modification date to use for the response.</param>
        /// <param name="fileLength">The file length to use for the response.</param>
        public RangeFilePathResult(string contentType, FileInfo file)
            : base(contentType, file.FullName, file.LastWriteTime, file.Length, Path.GetFileName(file.FullName))
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes the entire file to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        protected override void WriteEntireEntity(HttpResponseBase response)
        {
            response.TransmitFile(FileName);
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
                FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                stream.Seek(rangeStartIndex, SeekOrigin.Begin);

                int bytesRemaining = Convert.ToInt32(rangeEndIndex - rangeStartIndex) + 1;
                byte[] buffer = new byte[_bufferSize];

                while (bytesRemaining > 0 && response.IsClientConnected)
                {
                    int bytesRead = stream.Read(buffer, 0, _bufferSize < bytesRemaining ? _bufferSize : bytesRemaining);
                    response.OutputStream.Write(buffer, 0, bytesRead);
                    
                    bytesRemaining -= bytesRead;
                    //response.OutputStream.Flush();
                }

                stream.Close();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warn("Error in WriteEntityRange", ex);
            }
        }
        #endregion
    }
}