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
using NLog;

namespace MPExtended.Libraries.General
{
    public static class Log
    {
        private static Logger logger;

        static Log()
        {
            try
            {
                logger = LogManager.GetCurrentClassLogger();
            }
            catch (Exception ex)
            {
                // Oops. Logging failed to setup. This really shouldn't happen, but in all cases it's better to continue without logging then to crash. As this code is
                // called during service startup, it even aborts installation - which we really don't want to. So write the exception we encountered during log setup to
                // a file and continue without enabled logging.
                // The only known case where this could happen is on WHS 2011, where it has problems with finding the ${onexception} LayoutRenderer (see issue #137). We
                // should solve that properly, but that isn't done yet.
                string path = Path.Combine(Installation.GetLogDirectory(), "LoggingFailure.log");
                using (FileStream stream = File.Open(path, FileMode.Append))
                {
                    using (TextWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("[{0:yyyy-MM-dd HH:mm:ss}] Exception occured while setting up logging", DateTime.Now);
                        do
                        {
                            writer.WriteLine("{0}: {1}", ex.GetType().FullName, ex.Message);
                            writer.WriteLine(ex.StackTrace.ToString());
                            writer.WriteLine();
                            ex = ex.InnerException;
                        } while (ex != null);
                    }
                }

                logger = LogManager.CreateNullLogger();
            }
        }

        /// <summary>
        /// Log with level Trace
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Trace(String _msg)
        {
            logger.Trace(_msg);
        }

        /// <summary>
        /// Log with level Trace
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Trace(String _msg, Exception ex)
        {
            logger.TraceException(_msg, ex);
        }

        /// <summary>
        /// Log with level Trace
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Trace(String _msg, params object[] args)
        {
            logger.Trace(_msg, args);
        }

        /// <summary>
        /// Log with level Debug
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Debug(String _msg)
        {
            logger.Debug(_msg);
        }

        /// <summary>
        /// Log with level Debug
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Debug(String _msg, Exception ex)
        {
            logger.DebugException(_msg, ex);
        }

        /// <summary>
        /// Log with level Debug
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Debug(String _msg, params object[] args)
        {
            logger.Debug(_msg, args);
        }

        /// <summary>
        /// Log with level Info
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Info(String _msg)
        {
            logger.Info(_msg);
        }

        /// <summary>
        /// Log with level Info
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Info(String _msg, Exception ex)
        {
            logger.InfoException(_msg, ex);
        }

        /// <summary>
        /// Log with level Info
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Info(String _msg, params object[] args)
        {
            logger.Info(_msg, args);
        }

        /// <summary>
        /// Log with level Warn
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Warn(String _msg)
        {
            logger.Warn(_msg);
        }

        /// <summary>
        /// Log with level Warn
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Warn(String _msg, Exception ex)
        {
            logger.WarnException(_msg, ex);
        }

        /// <summary>
        /// Log with level Warn
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Warn(String _msg, params object[] args)
        {
            logger.Warn(_msg, args);
        }

        /// <summary>
        /// Log with level Error
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Error(String _msg)
        {
            logger.Error(_msg);
        }

        /// <summary>
        /// Log with level Error
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Error(String _msg, Exception ex)
        {
            logger.ErrorException(_msg, ex);
        }

        /// <summary>
        /// Log with level Error
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Error(String _msg, params object[] arg)
        {
            logger.Error(_msg, arg);
        }

        /// <summary>
        /// Log with level Fatal
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Fatal(String _msg)
        {
            logger.Fatal(_msg);
        }

        /// <summary>
        /// Log with level Fatal
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Fatal(String _msg, Exception ex)
        {
            logger.FatalException(_msg, ex);
        }

        /// <summary>
        /// Log with level Fatal
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Fatal(String _msg, params object[] args)
        {
            logger.Trace(_msg, args);
        }

        /// <summary>
        /// Flush all logs to disk
        /// </summary>
        public static void Flush()
        {
            NLog.LogManager.Flush();
        }

        /// <summary>
        /// Direct access to the NLog Logger instance (for advanced logging)
        /// </summary>
        public static Logger AdvancedLogger
        {
            get { return logger; }
        }
    }
}