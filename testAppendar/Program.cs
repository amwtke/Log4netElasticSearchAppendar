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

namespace testAppendar
{
    class Program
    {
        static int count = 0;
        static void Main(string[] args)
        {
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

            int total = TransferToLocal.BeginDependonFile();
            Console.WriteLine(total);

            //System.Timers.Timer t = new System.Timers.Timer(1000000);
            
            //t.Elapsed += new ElapsedEventHandler(Excute);
            Console.ReadKey();
        }

        static void Excute(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Timeer begin");
            int total = TransferToLocal.BeginDependonFile();
            Console.WriteLine(total);
            
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

        private static void ProcessObject<Object>(Object o)
        {
            if (o != null)
            {
                BizObject biz = JsonConvert.DeserializeObject<BizObject>(o.ToString());
                
                //Console.WriteLine(biz.SessionId);
                LogManager.LogHelper.LogBizAsync(biz);
                
                Interlocked.Increment(ref count);
                Console.WriteLine(count);
            }
        }
    }
}
