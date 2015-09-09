using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using log4net.ElasticSearch;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace LogManager
{
    delegate void LogInfoHandler(Type t, object message);
    delegate void LogErrorHandler(Type t, Exception ex);
    delegate void LogBizHandler(BizObject bo);
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

        #region 同步

        public static void WriteError(Type t, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(ex.Message, ex);
        }

        public static void WriteLogInfo(Type t, object message)
        {
            //Console.WriteLine("log info rprocess:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(message);
        }

        public static void WriteBizLog(BizObject bo)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(BizObject));
            log.Info(bo);
        }
        #endregion

        #region 异步
        public static void LogInfoAsync(Type t, object message)
        {
            //Console.WriteLine("into log infor:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            LogInfoHandler handler = new LogInfoHandler(WriteLogInfo);
            handler.BeginInvoke(t, message, LogInfoCallBack, handler);
        }

        public static void LogBizAsync(BizObject bo)
        {
            LogBizHandler handler = new LogBizHandler(WriteBizLog);
            handler.BeginInvoke(bo, LogBizCallBack, handler);
        }

        public static void LogErrorAsync(Type t, Exception ex)
        {
            //Console.WriteLine("into error" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            LogErrorHandler handler = new LogErrorHandler(WriteError);
            handler.BeginInvoke(t, ex, LogErrCallBack, handler);
        }
        #endregion

        #region callbacks
        private static void LogInfoCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log info call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogInfoCallBack fails,because ar is null!");

            LogInfoHandler handler = ar.AsyncState as LogInfoHandler;
            handler.EndInvoke(ar);
        }

        private static void LogBizCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log info call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogBizCallback fails,because ar is null!");

            LogBizHandler handler = ar.AsyncState as LogBizHandler;
            handler.EndInvoke(ar);
        }

        private static void LogErrCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log err call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogErrCallBack fails,because ar is null!");

            LogErrorHandler handler = ar.AsyncState as LogErrorHandler;
            handler.EndInvoke(ar);
        }
        #endregion

    }
}
