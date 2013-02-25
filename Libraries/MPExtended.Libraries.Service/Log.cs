#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
        private static Logger logger;

        static Log()
        {
            logger = new Logger();
        }

        public static void Setup(string fileName, bool consoleLogging)
        {
            List<ILogDestination> destination = new List<ILogDestination>();
            destination.Add(new FileDestination(LogLevel.Debug, Path.Combine(Installation.GetLogDirectory(), fileName)));
            if (consoleLogging)
            {
                destination.Add(new ConsoleDestination(LogLevel.Trace));
            }
            logger = new Logger(destination.ToArray());
        }

        public static bool IsEnabled(LogLevel level)
        {
            return logger.IsEnabled(level);
        }

        public static void Flush()
        {
            logger.Flush();
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