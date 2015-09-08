using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.ElasticSearch
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BizAttribute : Attribute
    {
        public BizAttribute()
        { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
        bool _isRequired;
        public RequiredAttribute(bool isRequired)
        {
            _isRequired = isRequired;
        }
        public RequiredAttribute()
        {
            _isRequired = true;
        }
        public bool IsRequired
        {
            get
            {
                return _isRequired;
            }
        }
    }
}
