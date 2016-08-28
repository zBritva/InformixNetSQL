using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformixNetSQL.DbProviderInterface;
using IBM.Data.Informix;

namespace InformixNetSQL.DbProviders
{
    public class SqlResult : IExecutionResult
    {
        public int resultCode { get; set; }

        public string resultMessage { get; set; }
    }

    public class SqlResult<T> : IExecutionResult<T> where T: DataTable
    {
        public int resultCode { get; set; }

        public string resultMessage { get; set; }

        public T resultData { get; set; }
    }

    public class InformixProvider : IEditor, IDisposable
    {
        protected IDbConnection connection = null;
        protected IDbTransaction transaction = null;
        protected string connectionString = null;


        public InformixProvider(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbTransaction BeginWork()
        {
            if(this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.transaction = this.connection.BeginTransaction();
                return this.transaction;
            }

            return null;
        }

        public IExecutionResult CommitWork()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.transaction.Rollback();
                this.transaction.Dispose();
                return new SqlResult();
            }

            return null;
        }

        public IDbConnection Connect()
        {
            if(this.connection == null)
            {
                this.connection = new IfxConnection(connectionString);
            }

            if(connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public IExecutionResult Disconnect()
        {
            if (this.connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }

            return new SqlResult();
        }

        public void Dispose()
        {
            if(this.transaction != null)
            {
                this.transaction.Rollback();
                this.transaction.Dispose();
            }
            if (this.connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }
        }

        public IExecutionResult<DataTable> Execution(string sql)
        {
            var result = new SqlResult<DataTable>();
            IDbCommand command = null;
            using (command = this.connection.CreateCommand())
            {
                command.Transaction = this.transaction;
                command.CommandText = sql;
                using (IDataReader reader = command.ExecuteReader())
                {
                    result.resultData = new DataTable();
                    result.resultData.Load(reader);
                }
            }

            return result;
        }

        public IExecutionResult RollbackWork()
        {
            if(this.transaction != null)
            {
                this.transaction.Rollback();
            }

            return new SqlResult();
        }

        public IExecutionResult SetDatabase(string databaseName)
        {
            if(this.connection != null)
            {
                this.Execution(string.Format("DATABASE {0}", databaseName.Trim()));
            }

            return new SqlResult();
        }
    }
}
