using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformixNetSQL.DbProviderInterface
{
    public interface IEditor
    {
        IDbConnection Connect();
        IExecutionResult Disconnect();
        IDbTransaction BeginWork();
        IExecutionResult CommitWork();
        IExecutionResult RollbackWork();
        IExecutionResult<DataTable> Execution(string sql);
        IExecutionResult SetDatabase(String databaseName);
    }
}
