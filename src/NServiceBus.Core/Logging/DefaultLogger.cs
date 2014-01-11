namespace NServiceBus.Logging
{
    using System;
    using System.IO;
    using System.Threading;

    class DefaultLogger : ILog
    {
        string typeName;
        string targetDirectory;
        object locker;

        public DefaultLogger(string typeName, string targetDirectory, object locker)
        {
            this.typeName = typeName;
            this.targetDirectory = targetDirectory;
            this.locker = locker;
        }

        void LogMessage(string info, string message, Exception exception = null)
        {
            var exceptionMessage = "";
            if (exception != null)
            {
                exceptionMessage = exception.ToString();
            }
            var now = DateTime.Now;
            var lineToWrite = string.Format("{0} {1} {2} {3} {4} {5}", now, Thread.CurrentThread.Name, info, typeName, message, exceptionMessage);
            Console.WriteLine(lineToWrite);
            var currentLogFilePath = Path.Combine(targetDirectory, string.Format("nservicebusLog_{0:yyyy-MM-dd}.txt", now));
            lock (locker)
            {
                File.WriteAllText(currentLogFilePath, lineToWrite + Environment.NewLine);
            }
            foreach (var logFilePath in Directory.EnumerateFiles("nservicebusLog_*"))
            {
                if (string.Equals(logFilePath, currentLogFilePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                var logFileName = Path.GetFileNameWithoutExtension(logFilePath);
                var datePart = logFileName.Replace("nservicebusLog_", string.Empty);
                var logFileDate = DateTime.Parse(datePart);
                if ((logFileDate - now).TotalDays > 10)
                {
                    File.Delete(logFilePath);
                }
            }
        }

        public bool IsDebugEnabled { get{return false;} }
        public bool IsInfoEnabled { get { return true; } }
        public bool IsWarnEnabled { get { return true; } }
        public bool IsErrorEnabled { get { return true; } }
        public bool IsFatalEnabled { get { return true; } }
        
        public void Debug(string message)
        {
        }

        public void Debug(string message, Exception exception)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void Info(string message)
        {
            LogMessage("Info", message);
        }
        
        public void Info(string message, Exception exception)
        {
            LogMessage("Info", message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogMessage("Info", string.Format(format, args));
        }

        public void Warn(string message)
        {
            LogMessage("Warn", message);
        }

        public void Warn(string message, Exception exception)
        {
            LogMessage("Warn", message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogMessage("Warn", string.Format(format, args));
        }

        public void Error(string message)
        {
            LogMessage("Error", message);
        }

        public void Error(string message, Exception exception)
        {
            LogMessage("Error", message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogMessage("Error", string.Format(format, args));
        }

        public void Fatal(string message)
        {
            LogMessage("Fatal", message);
        }

        public void Fatal(string message, Exception exception)
        {
            LogMessage("Fatal", message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogMessage("Fatal", string.Format(format, args));
        }
    }
}