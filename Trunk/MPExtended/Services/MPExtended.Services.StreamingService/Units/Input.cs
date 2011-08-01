#region Copyright
/* 
 *  Copyright (C) 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.IO;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Units {
    internal class InputUnit : IProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private string source;

        public InputUnit(string source) {
            this.source = source;
        }

        public bool Setup() {
            try {
                if (source.IndexOf(".ts.tsbuffer") != -1) {
                    Log.Write("Using TsBuffer to read input");
                    DataOutputStream = new TsBuffer(this.source);
                } else {
                    Log.Write("Using FileStream to read input");
                    DataOutputStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
            } catch (Exception e) {
                Log.Error("Failed to setup InputProcessingUnit", e);
                return false;
            }
            return true;
        }

        public bool Start() {
            return true;
        }

        public bool Stop() {
            DataOutputStream.Close();
            return true;
        }
    }
}
