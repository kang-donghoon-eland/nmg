using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NMG.Core;

namespace NMG.Service
{
    public class SqlServerDBController : DBController
    {
        public SqlServerDBController(string connectionStr) : base(connectionStr)
        {
        }

        public override ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var tableDetailsCommand = conn.CreateCommand())
                {
                    tableDetailsCommand.CommandText = "select column_name, data_type, character_maximum_length from information_schema.columns where table_name = '" + selectedTableName + "'";
                    using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        if (sqlDataReader != null)
                        {
                            while (sqlDataReader.Read())
                            {
                                var columnName = sqlDataReader.GetString(0);
                                var dataType = sqlDataReader.GetString(1);
                                columnDetails.Add(new ColumnDetail(columnName, dataType));
                            }
                        }
                    }
                }

                using (var constraintCommand = conn.CreateCommand())
                {
                    constraintCommand.CommandText = "select constraint_name from information_schema.TABLE_CONSTRAINTS where table_name = '" + selectedTableName + "' and constraint_type = 'PRIMARY KEY'";
                    var value = constraintCommand.ExecuteScalar();
                    if (value != null)
                    {
                        var constraintName = (string)value;
                        using (var pkColumnCommand = conn.CreateCommand())
                        {
                            pkColumnCommand.CommandText = "select column_name from information_schema.CONSTRAINT_COLUMN_USAGE where table_name = '" + selectedTableName + "' and constraint_name = '" + constraintName + "'";
                            var colName = pkColumnCommand.ExecuteScalar();
                            if (colName != null)
                            {
                                var pkColumnName = (string)colName;
                                var columnDetail = columnDetails.Find(detail => detail.ColumnName.Equals(pkColumnName));
                                columnDetail.IsPrimaryKey = true;
                            }
                        }
                    }
                }
            }
            columnDetails.Sort((x, y) => x.ColumnName.CompareTo(y.ColumnName));
            return columnDetails;
        }

        public override List<string> GetTables()
        {
            var tables = new List<string>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                SqlCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "SELECT * FROM sys.Tables";
                SqlDataReader sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                if (sqlDataReader != null)
                    while (sqlDataReader.Read())
                    {
                        string tableName = sqlDataReader.GetString(0);
                        tables.Add(tableName);
                    }
            }
            tables.Sort((x, y) => x.CompareTo(y));
            return tables;
        }

        public override List<string> GetSequences()
        {
            return new List<string>();
        }
    }
}