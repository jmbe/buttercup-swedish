using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Buttercup.Control.Common.IO;

namespace Buttercup.Control.Common
{
    /// <summary>
    /// This class is used to log Exceptions and messages. It is initialised by passing
    /// in a FileSystem object that is most likely going to be the IsolatedStorageFileSystem.
    /// </summary>
    public class Logger
    {
        #region Member Variables

        private IFileSystem _fileSystem;

        // Hardcoded at the moment, should be passed into a constructor maybe?
        private const string _LOG_FILE_PATH = "buttercup.log";

        private Stream _logFileStream;
        private StreamWriter _logFileStreamWriter;

        #endregion

        public static bool LoggingEnabled { get; set; }

        public static Logger Instance { get; set; }

        /// <summary>
        /// Log a text message (i.e. for trace)
        /// </summary>
        /// <param name="text"></param>
        public static void Log(string text)
        {
            if (LoggingEnabled)
            {
                if (Instance != null)
                {
                    Instance.Write(text);
                }
            }
        }

        //Log a test message given some string formatting arguments
        public static void Log(string text, params object[] args)
        {
            if (LoggingEnabled)
            {
                Log(string.Format(text, args));
            }
        }

        //Log an exception
        public static void Log(Exception ex)
        {
            if (LoggingEnabled)
            {
                Exception current = ex;
                string errorMsg = string.Empty;

                while (ex != null)
                {
                    errorMsg += ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
                    ex = ex.InnerException;
                }

                Log("Exception: " + errorMsg);
            }
        }

        public Logger(IFileSystem fileSystem)
        {
            //Create the StreamWriter which the logger will use for outputting to file
            _fileSystem = fileSystem;

            if (!fileSystem.FileExists(_LOG_FILE_PATH))
            {
                _logFileStream = fileSystem.CreateFile(_LOG_FILE_PATH);
            }
            else
            {
                _logFileStream = fileSystem.OpenFile(_LOG_FILE_PATH, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }

            _logFileStreamWriter = new StreamWriter(_logFileStream);
            //Since we are logging, set autoflush to true for immediate writes
            _logFileStreamWriter.AutoFlush = true;
            _logFileStreamWriter.WriteLine("------------ New Buttercup Session (" + DateTime.Now + ") ------------" + Environment.NewLine);

        }

        public void Write(string text)
        {
            if (_logFileStream != null && LoggingEnabled)
            {
                //Write the logging message to the log file, along with a timestamp
                _logFileStreamWriter.WriteLine("(" + DateTime.Now + ") - " + text);
            }
        }
    }
}