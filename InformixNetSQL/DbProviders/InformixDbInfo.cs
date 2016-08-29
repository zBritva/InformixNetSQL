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
        public enum InformixColumnTypesEnum
        {
            CHAR = 0,
            SMALLINT = 1,
            INTEGER = 2,
            FLOAT = 3,
            SMALLFLOAT = 4,
            DECIMAL = 5,
            SERIAL = 6,
            DATE = 7,
            MONEY = 8,
            NULL = 9,
            DATETIME = 10,
            BYTE = 11,
            TEXT = 12,
            VARCHAR = 13,
            INTERVAL = 14,
            NCHAR = 15,
            NVARCHAR = 16,
            INT8 = 17,
            SERIAL8 = 18,
            SET = 19,
            MULTISET = 20,
            LIST = 21,
            ROW_unnamed = 22,
            COLLECTION = 23,
            VariableLengthOpaqueType2 = 40,
            FixedLengthOpaqueType2 = 41,
            LVARCHAR = 43,
            BOOLEAN = 45,
            BIGINT = 52,
            BIGSERIAL = 53,
            IDSSECURITYLABEL2 = 2061,
            ROW_named = 4118
        }

        public InformixDbInfo(String connectionString) : base(connectionString)
        {

        }

        /// <summary>
        /// Processing info about column 
        /// See: https://www.ibm.com/support/knowledgecenter/SSGU8G_11.50.0/com.ibm.sqlr.doc/ids_sqr_030.htm
        /// </summary>
        /// <param name="coltype"></param>
        /// <param name="collength"></param>
        /// <param name="coldefault"></param>
        /// <param name="default_type"></param>
        /// <returns></returns>
        private string DecodeIfxType(string coltype, string collength, string coldefault, string default_type)
        {
            var ifxtype = "";
            switch(coltype)
            {
                case "DATETIME":
                    ifxtype = coltype + " " + collength.Replace(":","to");
                    break;
                case "VARCHAR":
                    ifxtype = coltype + "(" + collength + ")";
                    break;
                case "CHAR":
                    ifxtype = coltype + "(" + collength + ")";
                    break;
                case "LVARCHAR":
                    ifxtype = coltype + "(" + collength + ")";
                    break;
                case "DECIMAL":
                    ifxtype = coltype + "(" + collength + ")";
                    break;
                case "MONEY":
                    ifxtype = coltype + "(" + collength + ")";
                    break;
                default:
                    ifxtype = coltype;
                    break;
            }

            //process literal value
            if(default_type == "L")
            {
                if (coltype == "SMALLINT")
                    coldefault = coldefault.Replace("AAE", "").Replace("AAA","").Trim();

                ifxtype += " default " + coldefault.Trim();
            }

            if (default_type == "N")
            {
                ifxtype += " default NULL ";
            }

            if (default_type == "T")
            {
                ifxtype += " default today ";
            }

            return ifxtype;
        }

        public string GetTableDDL(string tablename)
        {
            var sqlSelectTable = "select * from systables where tabname= '" + tablename +"'";
            IExecutionResult<DataTable> table = this.Execution(sqlSelectTable);

            //From http://www.sql.ru/faq/faq_topic.aspx?fid=550
            var sqlSelectColumns =
                " select colname," +
                "         CASE mod(coltype,256)" +
                "                 WHEN  0 THEN 'CHAR'" +
                "                 WHEN  1 THEN 'SMALLINT'" +
                "                 WHEN  2 THEN 'INTEGER'" +
                "                 WHEN  3 THEN 'FLOAT'" +
                "                 WHEN  4 THEN 'SMALLFLOAT'" +
                "                 WHEN  5 THEN 'DECIMAL'" +
                "                 WHEN  6 THEN 'SERIAL'" +
                "                 WHEN  7 THEN 'DATE'" +
                "                 WHEN  8 THEN 'MONEY'" +
                "                 WHEN  9 THEN 'NULL'" +
                "                 WHEN 10 THEN 'DATETIME'" +
                "                 WHEN 11 THEN 'BYTE'" +
                "                 WHEN 12 THEN 'TEXT'" +
                "                 WHEN 13 THEN 'VARCHAR'" +
                "                 WHEN 14 THEN 'INTERVAL'" +
                "                 WHEN 15 THEN 'NCHAR'" +
                "                 WHEN 16 THEN 'NVCHAR'" +
                "                 WHEN 17 THEN 'INT8'" +
                "                 WHEN 18 THEN 'SERIAL8'" +
                "                 WHEN 19 THEN 'SET'" +
                "                 WHEN 20 THEN 'MULTISET'" +
                "                 WHEN 21 THEN 'LIST'" +
                "                 WHEN 22 THEN 'rOW (unnamed)'" +
                "                 WHEN 23 THEN 'COLLECTION'" +
                "                 WHEN 24 THEN 'ROWREF'" +
                "                 WHEN 25 THEN 'rOW (unnamed)'" +
                "                 WHEN 40 THEN 'Variable-length opaque type'" +
                "                 WHEN 41 THEN 'Fixed-length opaque type'" +
                "                 WHEN 4118 THEN 'Named row type'" +
                "                 ELSE 'UNKNOWN'" +
                "         END coltype," +
                "         CASE" +
                "                 WHEN mod(coltype,256) in (5,8) THEN trunc(collength/256)||','||mod(collength,256)" +
                "                 WHEN mod(coltype,256) in (10,14) THEN" +
                "                         CASE trunc(mod(collength,256)/16)" +
                "                                 WHEN  0 THEN 'YEAR'" +
                "                                 WHEN  2 THEN 'MONTH'" +
                "                                 WHEN  4 THEN 'DAY'" +
                "                                 WHEN  6 THEN 'HOUR'" +
                "                                 WHEN  8 THEN 'MINUTE'" +
                "                                 WHEN 10 THEN 'SECOND'" +
                "                                 WHEN 11 THEN 'FRACTION(1)'" +
                "                                 WHEN 12 THEN 'FRACTION(2)'" +
                "                                 WHEN 13 THEN 'FRACTION(3)'" +
                "                                 WHEN 14 THEN 'FRACTION(4)'" +
                "                                 WHEN 15 THEN 'FRACTION(5)'" +
                "                         END ||'('||trunc(collength/256)+trunc(mod(collength,256)/16)-mod(collength,16)||') : '||" +
                "                         CASE mod(collength,16)" +
                "                                 WHEN  0 THEN 'YEAR'" +
                "                                 WHEN  2 THEN 'MONTH'" +
                "                                 WHEN  4 THEN 'DAY'" +
                "                                 WHEN  6 THEN 'HOUR'" +
                "                                 WHEN  8 THEN 'MINUTE'" +
                "                                 WHEN 10 THEN 'SECOND'" +
                "                                 WHEN 11 THEN 'FRACTION(1)'" +
                "                                 WHEN 12 THEN 'FRACTION(2)'" +
                "                                 WHEN 13 THEN 'FRACTION(3)'" +
                "                                 WHEN 14 THEN 'FRACTION(4)'" +
                "                                 WHEN 15 THEN 'FRACTION(5)'" +
                "                         END" +
                "                 ELSE ''||collength" +
                "         END collength," +
                "         CASE" +
                "                 WHEN coltype>255 THEN 'NO'" +
                "                 ELSE 'YES' " +
                "         END nnull," +
                "      c.colno, " +
                "      nvl(def.default,'') as default, " +
                "      nvl(def.type,'') as default_type " +
                " from syscolumns c " +
                " join systables t on c.tabid = t.tabid " +
                " left join sysdefaults def on def.tabid = t.tabid and def.colno = c.colno" +
                " where tabname= '" + tablename + "'" +
                " order by colno";

            IExecutionResult<DataTable> columns = this.Execution(sqlSelectColumns);
            var ddl =
                "CREATE TABLE " + tablename + " ( " + System.Environment.NewLine;

            foreach(DataRow tableColumn in columns.resultData.Rows)
            {
                var colname = tableColumn["colname"].ToString().Trim();
                var coltype = tableColumn["coltype"].ToString().Trim();
                var collength = tableColumn["collength"].ToString().Trim();
                var allownull = tableColumn["nnull"].ToString().Trim();
                var colno = tableColumn["colno"].ToString().Trim();
                var coldefault = tableColumn["default"].ToString().Trim();
                var default_type = tableColumn["default_type"].ToString();

                ddl += string.Format("\t{0} {1} {2},{3}",
                    colname,
                    this.DecodeIfxType(coltype, collength, coldefault, default_type),
                    allownull == "NO" ? " NOT NULL ": "",
                    System.Environment.NewLine
                    );
            }

            ddl = ddl.Substring(0, ddl.Length - 1);

            ddl += ")" + System.Environment.NewLine;

            var EXTENT_SIZE = table.resultData.Rows[0]["fextsize"].ToString();
            var NEXT_SIZE = table.resultData.Rows[0]["nextsize"].ToString();
            var LOCK_LEVEL = table.resultData.Rows[0]["locklevel"].ToString();

            ddl += "EXTENT SIZE " + EXTENT_SIZE + " NEXT SIZE " + NEXT_SIZE + " LOCK MODE " + (LOCK_LEVEL == "R" ? "ROW" : "PAGE") + ";";

            return ddl;
        }

        public List<string> GetProcedures()
        {
            throw new NotImplementedException();
        }
        
        public string GetProcedureBody(string procname)
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
