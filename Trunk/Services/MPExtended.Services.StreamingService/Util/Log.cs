using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace MPExtended.Services.StreamingService.Util
{
    public static class Log
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        /// Log with level Info (compatibility with older MPWebStream code)
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void Write(String _msg)
        {
            logger.Info(_msg);
        }

        /// <summary>
        /// Log with level Info (compatibility with older MPWebStream code)
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Exception that will be logged</param>
        public static void Write(String _msg, Exception ex)
        {
            logger.InfoException(_msg, ex);
        }

        /// <summary>
        /// Log with level Info (compatibility with older MPWebStream code)
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        /// <param name="ex">Values for the message</param>
        public static void Write(String _msg, params object[] args)
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
        /// Direct access to the NLog Logger instance (for advanced logging)
        /// </summary>
        public static Logger AdvancedLogger
        {
            get { return logger; }
        }
    }
}
