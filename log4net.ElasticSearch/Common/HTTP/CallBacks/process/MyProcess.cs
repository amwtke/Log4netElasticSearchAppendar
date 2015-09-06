using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.ElasticSearch
{
    class MyProcess : IProcess
    {

        public void Process(object o)
        {
            //Program.f.SetText(o as string);
            Console.WriteLine(o as string);
        }
    }
}
