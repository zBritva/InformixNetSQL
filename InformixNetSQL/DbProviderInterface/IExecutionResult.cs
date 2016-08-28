using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformixNetSQL.DbProviderInterface
{
    public interface IExecutionResult
    {
        int resultCode { get; set; }
        string resultMessage { get; set; }
    }

    public interface IExecutionResult<T> where T : class
    {
        int resultCode { get; set; }
        string resultMessage { get; set; }
        T resultData { get; set; }
    }
}
