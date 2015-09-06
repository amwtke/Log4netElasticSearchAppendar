using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace LogHelper
{
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
            log.Error("Error", ex);
        }

        public static void WriteLogInfo(Type t, object message)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(message);
        }
        #endregion
    }
}
