using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace log4net.ElasticSearch
{
    public static class AttributeHelper
    {
        /// <summary>
        /// 判断一个对象是不是一个合格的bizObject
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsBizObject(object o)
        {
            if (o == null) return false;
            object[] bizAtts = o.GetType().GetCustomAttributes(typeof(BizAttribute),false);
            
            if (bizAtts == null)     return false;
            if (bizAtts.Length == 0) return false;
            PropertyInfo[] pis = o.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                 var att = pi.GetCustomAttribute(typeof(RequiredAttribute));
                 if (att == null)
                     continue;

                 RequiredAttribute r = att as RequiredAttribute;
                 if (r.IsRequired)
                 {
                     object pio = pi.GetValue(o);

                     if (pio == null)
                         return false;

                     if (pio is string && string.IsNullOrEmpty(((string)pio)))
                     {
                         return false;
                     }

                     if (pio is DateTime && ((DateTime)pio)==DateTime.MinValue)
                     {
                         return false;
                     }

                     //if (pio is int && ((int)pio) == 0)
                     //{
                     //    return false;
                     //}
                 }
            }
            return true;
        }
    }
}
