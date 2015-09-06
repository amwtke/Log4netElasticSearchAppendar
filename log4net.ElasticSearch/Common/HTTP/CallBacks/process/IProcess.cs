using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.ElasticSearch
{
    public interface IProcess
    {
        void Process(Object o);
    }
}
