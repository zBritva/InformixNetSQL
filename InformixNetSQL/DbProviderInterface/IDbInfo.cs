using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformixNetSQL.DbProviderInterface
{
    public interface IDbInfo
    {
        List<string> GetDataBases();
        List<string> GetTables();
        List<string> GetProcedures();
        string GetTableDDL(string tablename);
        string GetProcedureBody(string procname);
    }
}
