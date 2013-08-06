#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.Logging;

namespace MPExtended.Libraries.Service
{
    public static class Log
    {
        public static string Filename { get; set; }
        public static bool ConsoleLogging { get; set; }
        public static bool TraceLogging { get; set; }

        private static Logger logger;

        public static void Setup()
        {
            if (logger != null)
                logger.Dispose();

            List<ILogDestination> destination = new List<ILogDestination>();
            LogLevel minimalFileLevel = TraceLogging ? LogLevel.Trace : LogLevel.Debug;
            destination.Add(new FileDestination(minimalFileLevel, Path.Combine(Installation.GetLogDirectory(), Filename)));
            if (ConsoleLogging)
                destination.Add(new ConsoleDestination(LogLevel.Trace));
            logger = new Logger(destination.ToArray());
        }

        public static void Disable()
        {
            logger = new Logger();
        }

        public static bool IsEnabled(LogLevel level)
        {
            return logger.IsEnabled(level);
        }

        public static void Flush()
        {
            logger.Flush();
        }

        public static void Write(LogLevel level, String _msg)
        {
            logger.LogLine(level, _msg);
        }

        public static void Write(LogLevel level, String _msg, Exception ex)
        {
            logger.LogLine(level, _msg, ex);
        }

        public static void Write(LogLevel level, String _msg, params object[] args)
        {
            logger.LogLine(level, _msg, args);
        }

        public static void Trace(String _msg)
        {
            logger.LogLine(LogLevel.Trace, _msg);
        }

        public static void Trace(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Trace, _msg, ex);
        }

        public static void Trace(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Trace, _msg, args);
        }

        public static void Debug(String _msg)
        {
            logger.LogLine(LogLevel.Debug, _msg);
        }

        public static void Debug(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Debug, _msg, ex);
        }

        public static void Debug(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Debug, _msg, args);
        }

        public static void Info(String _msg)
        {
            logger.LogLine(LogLevel.Info, _msg);
        }

        public static void Info(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Info, _msg, ex);
        }

        public static void Info(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Info, _msg, args);
        }

        public static void Warn(String _msg)
        {
            logger.LogLine(LogLevel.Warn, _msg);
        }

        public static void Warn(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Warn, _msg, ex);
        }

        public static void Warn(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Warn, _msg, args);
        }

        public static void Error(String _msg)
        {
            logger.LogLine(LogLevel.Error, _msg);
        }

        public static void Error(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Error, _msg, ex);
        }

        public static void Error(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Error, _msg, args);
        }

        public static void Fatal(String _msg)
        {
            logger.LogLine(LogLevel.Fatal, _msg);
        }

        public static void Fatal(String _msg, Exception ex)
        {
            logger.LogLine(LogLevel.Fatal, _msg, ex);
        }

        public static void Fatal(String _msg, params object[] args)
        {
            logger.LogLine(LogLevel.Fatal, _msg, args);
        }
    }
}