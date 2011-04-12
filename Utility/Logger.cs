using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verde.Utility
{
    class Logger
    {
        private const string strDateFormatForFilename = "yyyyMMdd";
        private const string strDefaultDateFormat = "yyyy/MM/dd HH:mm:ss";
        private const string strDefaultLogFilename = "logger_{0}.log";

        string strDateFormat = strDefaultDateFormat;

        public Logger()
        : this(String.Format(Logger.strDefaultLogFilename, System.DateTime.Now.ToString(Logger.strDateFormatForFilename)), System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), Logger.strDefaultDateFormat)
        {
        }

        public Logger(string strLogFilename, string strPath, string strDateFormat)
        {
            System.Diagnostics.DefaultTraceListener listener = (System.Diagnostics.DefaultTraceListener)System.Diagnostics.Debug.Listeners["Default"];
            listener.LogFileName = System.IO.Path.Combine(strPath, strLogFilename);
            this.strDateFormat = strDateFormat;
        }

        public void Write(string strMessage)
        {
            System.Diagnostics.StackTrace trcStack = new System.Diagnostics.StackTrace(true);

            string strOutputMessage = String.Format("{0} [{1}.{2}() : Line {3}] : {4}"
                , System.DateTime.Now.ToString(this.strDateFormat)
                , trcStack.GetFrame(1).GetMethod().ReflectedType.FullName
                , trcStack.GetFrame(1).GetMethod().Name
                , trcStack.GetFrame(1).GetFileLineNumber().ToString()
                , strMessage
                );

            System.Diagnostics.Debug.WriteLine(strOutputMessage);
        }
    }
}
