using System.Collections.Generic;
using log4net.Appender;
using log4net.Util;

namespace SmartStore.Core.Logging
{
    public class SmartFileAppender : RollingFileAppender
    {
        /// <summary>
        /// Dictionary of already known suffixes (based on previous attempts) for a given filename.
        /// </summary>
        private static readonly Dictionary<string, int> _suffixes = new Dictionary<string, int>();

        /// <summary>
        /// The number of suffix attempts that will be made on each OpenFile method call.
        /// </summary>
        private const int Retries = 50;

        /// <summary>
        /// Maximum number of suffixes recorded before a cleanup happens to recycle memory.
        /// </summary>
        private const int MaxSuffixes = 100;

        /// <summary>
        /// Prevents empty log files.
        /// The sequence from log4Net source is as below:
        /// - The first call to OpenFile() is because of ActivateOptions() called from FileAppender's constructor.
        /// - When log message is generated, AppenderSkeleton's DoAppend() calls PreAppendCheck()
        /// - PreAppendCheck() is overridden in TextWriterAppender, the base of FileAppender.
        /// - The overridden PreAppendCheck() calls virtual PrepareWriter if the file is not yet open.
        /// - PrepareWriter() of FileAppender calls SafeOpenFile() which inturn calls OpenFile()
        /// </summary>
        private bool _isFirstCall = true;

        /// <summary>
        /// Opens the log file adding an incremental suffix to the filename if required due to an opening failure (usually, locking).
        /// </summary>
        /// <param name="fileName">The filename as specified in the configuration file.</param>
        /// <param name="append">Boolean flag indicating weather the log file should be appended if it already exists.</param>
        protected override void OpenFile(string fileName, bool append)
        {
            if (_isFirstCall)
            {
                _isFirstCall = false;
                return;
            }

            lock (this)
            {
                bool fileOpened = false;
                string completeFilename = GetNextOutputFileName(fileName);
                string currentFilename = fileName;

                if (_suffixes.Count > MaxSuffixes)
                {
                    _suffixes.Clear();
                }

                if (!_suffixes.ContainsKey(completeFilename))
                {
                    _suffixes[completeFilename] = 0;
                }

                int newSuffix = _suffixes[completeFilename];

                for (int i = 1; !fileOpened && i <= Retries; i++)
                {
                    try
                    {
                        if (newSuffix > 0)
                        {
                            currentFilename = string.Format("{0}-{1}", fileName, newSuffix);
                        }

                        BaseOpenFile(currentFilename, append);

                        fileOpened = true;
                    }
                    catch
                    {
                        newSuffix = _suffixes[completeFilename] + i;

                        LogLog.Error(typeof(SmartFileAppender), string.Format("SmartFileAppender: Failed to open [{0}]. Attempting [{1}-{2}] instead.", fileName, fileName, newSuffix));
                    }
                }

                _suffixes[completeFilename] = newSuffix;
            }
        }

        /// <summary>
        /// Calls the base class OpenFile method. Allows this method to be mocked.
        /// </summary>
        /// <param name="fileName">The filename as specified in the configuration file.</param>
        /// <param name="append">Boolean flag indicating weather the log file should be appended if it already exists.</param>
        protected virtual void BaseOpenFile(string fileName, bool append)
        {
            base.OpenFile(fileName, append);
        }
    }
}
