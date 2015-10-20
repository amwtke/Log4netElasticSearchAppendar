using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
namespace LogManager
{
    public static class ConnectionManager
    {
        static ElasticClient _client;
        private static ElasticClient _localClient;
        static Object _syncObject = new object();
        

        public static ElasticClient RemoteClient
        {
            get
            {
                if(_client==null)
                    init();
                return _client;
            }
        }

        public static ElasticClient LocalClient
        {
            get
            {
                if(_localClient==null)
                    init();
                return _localClient;
            }
        }
        static ConnectionManager()
        {
           init();
        }

        private static void

        init()
        {
            if (_client != null) return;
            lock (_syncObject)
            {
                if (_client != null) return;
                try
                {
                    string Address = ConfigurationManager.AppSettings["src_address"];
                    string Port = ConfigurationManager.AppSettings["src_port"];

                    

                    var node = new Uri(@"http://" + Address + ":" + Port);

                    var settings = new ConnectionSettings(
                        node
                        );

                    _client = new ElasticClient(settings);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            if (_localClient != null) return;
            lock (_syncObject)
            {
                if (_localClient != null) return;
                try
                {
                    string l_Address = ConfigurationManager.AppSettings["local_address"];
                    string l_Port = ConfigurationManager.AppSettings["local_port"];



                    var node = new Uri(@"http://" + l_Address + ":" + l_Port);

                    var settings = new ConnectionSettings(
                        node
                        );

                    _localClient = new ElasticClient(settings);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
