using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
namespace LogManager
{
    public static class MappingHelper
    {
        /// <summary>
        /// 检查服务器是否有指定index下的mapping，如果没有则创建，如果有则什么都不做。
        /// </summary>
        /// <typeparam name="T">对象必须有[ElasticType(Name = "trace2")]；[ElasticProperty(Index = FieldIndexOption.NotAnalyzed,Name = "UserEmail")]</typeparam>
        /// <param name="indexName"></param>
        /// <returns>true表示保证服务器有这个mapping，false表示服务器错误。</returns>
        public static bool CheckMapping<T>(string indexName) where T : class
        {
            IGetMappingResponse mapping = ConnectionManager.RemoteClient.GetMapping<T>();
            if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;
            //var response = ConnectionManager.RemoteClient.CreateIndex(
            //    c => c.Index(BIZ_INDEX_NAME).InitializeUsing(_bizSettings).AddMapping<BizObject>(m => m.MapFromAttributes()));

            var response = ConnectionManager.RemoteClient.Map<T>(x => x.Index(indexName));
            return response.Acknowledged;
        }
    }
}
