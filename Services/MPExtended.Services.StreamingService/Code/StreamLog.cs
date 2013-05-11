#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Logging;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class StreamLog
    {
        public class StreamLogDetails
        {
            public StringBuilder FullLog { get; set; }
            public string LastError { get; set; }

            public StreamLogDetails()
            {
                FullLog = new StringBuilder();
            }
        }

        private static Dictionary<string, StreamLogDetails> streamLogs = new Dictionary<string, StreamLogDetails>();

        public static StreamLogDetails GetStreamLogDetails(string streamIdentifier)
        {
            if (!streamLogs.ContainsKey(streamIdentifier))
                streamLogs[streamIdentifier] = new StreamLogDetails();

            return streamLogs[streamIdentifier];
        }

        private static void WriteLogHeader(string streamIdentifier, LogLevel level, string message, params object[] args)
        {
            if (!streamLogs.ContainsKey(streamIdentifier))
                streamLogs[streamIdentifier] = new StreamLogDetails();
            else
                streamLogs[streamIdentifier].FullLog.AppendLine();
            streamLogs[streamIdentifier].FullLog.AppendFormat("{0:HH:mm:ss.fffff} {1,5}: ", DateTime.Now, Enum.GetName(typeof(LogLevel), level).ToUpperInvariant());

            if (level >= LogLevel.Error)
                streamLogs[streamIdentifier].LastError = String.Format(message, args);
        }

        private static void WriteLog(string streamIdentifier, LogLevel level, string message, params object[] args)
        {
            WriteLogHeader(streamIdentifier, level, message, args);
            streamLogs[streamIdentifier].FullLog.AppendFormat(message, args);
            Log.Write(level, String.Format("[{0,30}] {1}", streamIdentifier, message), args);
        }

        private static void WriteLog(string streamIdentifier, LogLevel level, string message, Exception ex)
        {
            WriteLogHeader(streamIdentifier, level, message);
            streamLogs[streamIdentifier].FullLog.Append(message);
            Log.Write(level, String.Format("[{0,30}] {1}", streamIdentifier, message), ex);
        }

        public static void Debug(string identifier, string message, params object[] args)
        {
            WriteLog(identifier, LogLevel.Debug, message, args);
        }

        public static void Debug(string identifier, string message, Exception ex)
        {
            WriteLog(identifier, LogLevel.Debug, message, ex);
        }

        public static void Info(string identifier, string message, params object[] args)
        {
            WriteLog(identifier, LogLevel.Info, message, args);
        }

        public static void Info(string identifier, string message, Exception ex)
        {
            WriteLog(identifier, LogLevel.Info, message, ex);
        }

        public static void Warn(string identifier, string message, params object[] args)
        {
            WriteLog(identifier, LogLevel.Warn, message, args);
        }

        public static void Warn(string identifier, string message, Exception ex)
        {
            WriteLog(identifier, LogLevel.Warn, message, ex);
        }

        public static void Error(string identifier, string message, params object[] args)
        {
            WriteLog(identifier, LogLevel.Error, message, args);
        }

        public static void Error(string identifier, string message, Exception ex)
        {
            WriteLog(identifier, LogLevel.Error, message, ex);
        }

        public static void Fatal(string identifier, string message, params object[] args)
        {
            WriteLog(identifier, LogLevel.Fatal, message, args);
        }

        public static void Fatal(string identifier, string message, Exception ex)
        {
            WriteLog(identifier, LogLevel.Fatal, message, ex);
        }
    }
}
