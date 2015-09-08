using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.ElasticSearch
{
    [Biz]
    public class BizObject
    {
        [Required]
        public DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        [Required]
        public String UserEmail { get; set; }
        public string FromUrl { get; set; }
        public string NowUrl { get; set; }
        public string UserIP { get; set; }
        /// <summary>
        /// 功能模块的名称
        /// </summary>
        [Required]
        public string ModelName { get; set; }
        [Required]
        public string SessionId { get; set; }
    }
}
