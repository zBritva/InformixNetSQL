using InformixNetSQL.DbProviderInterface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformixNetSQL.DbProviders
{
    public class InformixDbInfo : InformixProvider , IDbInfo
    {
        public InformixDbInfo(String connectionString) : base(connectionString)
        {

        }

        public List<string> GetTableDDL(string tablename)
        {
            throw new NotImplementedException();
        }

        public List<string> GetDataBases()
        {
            this.Connect();
            IExecutionResult<DataTable> tables = this.Execution("SELECT * FROM sysdatabases;");
            return tables.resultData.AsEnumerable().Select<DataRow, String>(t => t["name"].ToString()).OrderBy(t=> t).ToList();
        }

        public List<string> GetTables()
        {
            this.Connect();
            IExecutionResult<DataTable> tables = this.Execution("SELECT * FROM systables;");
            return tables.resultData.AsEnumerable().Select<DataRow, String>(t => t["tabname"].ToString()).OrderBy(t => t).ToList();
        }
    }
}
