using InformixNetSQL.DbProviderInterface;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace InformixNetSQL
{
    public class ConnectionManager
    {
        public IEditor connection { get; set; }

        public string currentDatabse { get; set; }

        public bool InitConnection(ConnectionStringSettings provider)
        {
            switch (provider.ProviderName)
            {
                case "IBM.Data.Informix":
                    connection = new InformixNetSQL.DbProviders.InformixDbInfo(provider.ConnectionString);
                    currentDatabse = provider.Name;
                    break;
                default:
                    throw new ArgumentException(String.Format("Provider: {0}", provider.ProviderName) );
            }

            return true;
        }

        public List<ConnectionStringSettings> GetConnectionList()
        {
            var list = new List<ConnectionStringSettings>();

            try
            {
                var connStrings = System.Configuration.ConfigurationManager.ConnectionStrings;

                if (connStrings != null)
                {
                    foreach (ConnectionStringSettings cs in connStrings)
                    {
                        list.Add(cs);
                    }
                }

            }
            catch (Exception ex)
            {
                return list;
            }
            return list;
        }
    }
}
