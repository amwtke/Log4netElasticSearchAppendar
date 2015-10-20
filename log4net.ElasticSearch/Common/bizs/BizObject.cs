using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch;
using Nest;

namespace log4net.ElasticSearch
{
    public enum BizEnum
    {
        NOVALUE
    }
    [Biz]
    [ElasticType(Name = "trace")]
    public class BizObject
    {
        public BizObject(DateTime timestamp, string useremail, string modelname, string sessionId, string fromUrl, string nowUrl, string user_uuid)
        {
            TimeStamp = timestamp;
            UserEmail = useremail;
            ModelName = modelname;
            SessionId = sessionId;
            FromUrl = fromUrl;
            NowUrl = nowUrl;
            UUID = user_uuid;
        }
        [Required]
        [ElasticProperty(Name = "TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "UserName")]
        public string UserName { get; set; }

        [ElasticProperty(Name = "UserId")]
        public int UserId { get; set; }

        [Required]
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "UserEmail")]
        public String UserEmail { get; set; }

        [ElasticProperty(Name = "FromUrl")]
        public string FromUrl { get; set; }

        [ElasticProperty(Name = "NowUrl")]
        public string NowUrl { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "UserIP")]
        public string UserIP { get; set; }

        /// <summary>
        /// 功能模块的名称
        /// </summary>
        [Required]
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "ModelName")]
        public string ModelName { get; set; }

        [Required]
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "SessionId")]
        public string SessionId { get; set; }

        [ElasticProperty(Name = "Message")]
        public string Message { get; set; }


        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "UUID")]
        public string UUID { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "Platform")]
        public string Platform { get; set; }

        [ElasticProperty(Name = "UnUsed1")]
        public string UnUsed1 { get; set; }
        [ElasticProperty(Name = "UnUsed2")]
        public string UnUsed2 { get; set; }
        [ElasticProperty(Name = "UnUsed3")]
        public string UnUsed3 { get; set; }
        
    }
}
