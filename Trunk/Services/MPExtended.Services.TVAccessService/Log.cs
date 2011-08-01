using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace MPExtended.Services.TVAccessService
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
        public static void Trace(String _msg, Exception ex)
        {
            logger.Trace(_msg, ex);
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
        public static void Debug(String _msg, Exception ex)
        {
            logger.Debug(_msg, ex);
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
        public static void Info(String _msg, Exception ex)
        {
            logger.Info(_msg, ex);
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
        public static void Warn(String _msg, Exception ex)
        {
            logger.Warn(_msg, ex);
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
        public static void Error(String _msg, Exception ex)
        {
            logger.Error(_msg, ex);
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
        public static void Fatal(String _msg, Exception ex)
        {
            logger.Fatal(_msg, ex);
        }

        /// <summary>
        /// Log callback for MPWebStream logs
        /// </summary>
        /// <param name="_msg">Message that will be logged</param>
        public static void MPWebStreamCallback(int _logLevel, String _msg)
        {
            switch (_logLevel)
            {
                case 0:
                    Trace("MPWebStream: " + _msg);
                    break;
                case 1:
                    Debug("MPWebStream: " + _msg);
                    break;
                case 2:
                    Info("MPWebStream: " + _msg);
                    break;
                case 3:
                    Warn("MPWebStream: " + _msg);
                    break;
                case 4:
                    Error("MPWebStream: " + _msg);
                    break;
                case 5:
                    Fatal("MPWebStream: " + _msg);
                    break;
            }
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
