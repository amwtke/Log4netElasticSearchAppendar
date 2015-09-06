using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace LogHelper
{
    public delegate void LogInfoHandler(Type t, object message);
    public delegate void LogErrorHandler(Type t, Exception ex);
    public static class LogHelper
    {
        static LogHelper()
        {
            System.Reflection.Assembly.LoadFrom("log4stash.dll");
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly a in loadedAssemblies)
            {
                Console.WriteLine(a.ToString());
            }
        }

        #region 

        public static void WriteError(Type t, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(ex.Message, ex);
        }

        public static void WriteLogInfo(Type t, object message)
        {
            Console.WriteLine("log info rprocess:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(message);
        }

        public static void LogInfoAsync(Type t, object message)
        {
            Console.WriteLine("into log infor:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            LogInfoHandler handler = new LogInfoHandler(WriteLogInfo);
            handler.BeginInvoke(t, message, LogInfoCallBack, handler);
        }
        public static void LogErrorAsync(Type t, Exception ex)
        {
            Console.WriteLine("into error" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            LogErrorHandler handler = new LogErrorHandler(WriteError);
            handler.BeginInvoke(t, ex, LogInfoCallBack, handler);
        }

        public static void LogInfoCallBack(IAsyncResult ar)
        {
            Console.WriteLine("log info call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("WebRequestCallBacks3 fails,because ar is null!");

            LogInfoHandler handler = ar.AsyncState as LogInfoHandler;
            handler.EndInvoke(ar);
        }
        public static void LogErrCallBack(IAsyncResult ar)
        {
            Console.WriteLine("log err call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("WebRequestCallBacks3 fails,because ar is null!");

            LogErrorHandler handler = ar.AsyncState as LogErrorHandler;
            handler.EndInvoke(ar);
        }
        #endregion
    }
}
