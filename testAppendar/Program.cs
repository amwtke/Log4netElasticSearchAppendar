using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogManager;
using System.IO;
using System.Threading;
using System.Timers;
using log4net.ElasticSearch;
using Newtonsoft;
using Newtonsoft.Json;
using System.Configuration;

namespace testAppendar
{
    class Program
    {
        static int count = 0;
        private static Boolean flag = true;
        static string interval = ConfigurationManager.AppSettings["Transfer_interval"];
        static int interval_int = Convert.ToInt32(interval);
        static void Main(string[] args)
        {
            #region 
            //try
            //{
            //    //Method2();

            //    //for (int i = 0; i < 5000; i++)
            //    //{
            //    //    Console.WriteLine(i);
            //    //    LogHelper.LogInfoAsync(typeof(Program), "嘻嘻哈哈" + DateTime.Now.ToString());
            //    //}

            //    //throw new Exception("我是异常3！" + DateTime.Now.ToString());
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.LogErrorAsync(typeof(Program), ex);
            //}

            //LogHelper.LogInfoAsync(typeof(Program), "你们怎么这么吊？？");

            //for (int i = 0; i < 5000; i++)
            //{
            //    BizObject o = new BizObject(DateTime.Now, "113966473@qq.com", "test_model" + i.ToString(), Guid.NewGuid().ToString(), BizEnum.NOVALUE.ToString(), "now.aspx", Guid.NewGuid().ToString());
            //    o.Message = "怎么这么吊？！！？";
            //    //LogHelper.LogInfoAsync(typeof(Program), o);
            //    LogHelper.WriteBizLog(o);
            //}


            //ScanScrollHelper.TransferAsync(1000,from,DateTime.Now,ProcessObject);
            //ScanScrollHelper.Transfer(ProcessObject);
            #endregion
            Console.WriteLine("input \'all\' to sync all,and quit. input anything else to fork thread.");
            if (Console.ReadLine().ToLower() == "all")
            {
                int total = TransferToLocal.BeginDependonFile();
                Console.WriteLine(total);
                Console.WriteLine("press any key to quit.");
                Console.ReadKey();
            }
            else
            { 
                System.Threading.Thread thread = new Thread(Excute);
                thread.Start();
                System.Threading.Thread.Sleep(300);
                while (flag)
                {
                    var key = Console.ReadLine().ToLower();
                    if (key == "q")
                    { flag = false; Thread.Sleep(interval_int); }
                }
            }
        }

        static void Excute()
        {
            while(flag)
            {
                Console.WriteLine("begin process");
                int total = TransferToLocal.BeginDependonFile();
                Console.WriteLine(total);
                Thread.Sleep(interval_int);
            }
        }

        public static void Method2()
        {
            FileStream fs = null;
            try
            {
                //假如c:\\file.txt不存在，这里一定会抛出文件未找到异常
                fs = new FileStream(@"c:\\file"+ DateTime.Now.ToString()+@".txt", FileMode.Open);
                fs.ReadByte();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(Program), ex);
            }
            finally
            {
                //LogHelper.WriteLog("Method2 finally");
                if (fs != null)
                    fs.Close();
                fs = null;
            }
        }

    }
}
