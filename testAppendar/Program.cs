using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogHelper;
namespace testAppendar
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LogHelper.LogHelper.WriteLogInfo(typeof(Program), "你好@！");
                throw new Exception("我是异常！");
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteError(typeof(Program), ex);
            }


            Console.ReadKey();
        }
    }
}
