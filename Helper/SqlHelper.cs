using System.Data;
using System.Data.SqlClient;

namespace ChatApp2.Helper
{
    public sealed class SqlHelper
    {
        public SqlHelper()
        {

        }
        /// <summary>
        /// Executes insert/update/delete statements and returns number of rows affected.
        /// </summary>
        /// <param name="commandText">Sql statement or name of procedure to be executed</param>
        /// <param name="commandType">Type of command i.e. Sql query, Stored procedure or table direct</param>
        /// <param name="parameters">Optional parameter accepts array of SqlParameters</param>
        /// <returns>number of rows affected as int</returns>
        public int Execute(string connectionString, string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            var result = 0;
            using (var helper = new SqlConnectionHelper(connectionString))
            {
                var connection = helper.GetConnection();
                using (var command = new SqlCommand(commandText, connection))
                {
                    try
                    {

                        command.CommandType = commandType;
                        command.CommandTimeout = 300;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        result = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Executes select statements and returns value of first most row and first most column.
        /// </summary>
        /// <param name="commandText">Sql statement or name of procedure to be executed</param>
        /// <param name="commandType">Type of command i.e. Sql query, Stored procedure or table direct</param>
        /// <param name="parameters">Optional parameter accepts array of SqlParameters</param>
        /// <returns>value of first most row and first most column</returns>
        public object Scalar(string connectionString, string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            var result = new object();
            try
            {
                using (var helper = new SqlConnectionHelper(connectionString))
                {
                    var connection = helper.GetConnection();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = commandType;
                        command.CommandTimeout = 300;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        result = command.ExecuteScalar();
                        command.Parameters.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {

                throw ex;
            }


            return result;
        }

        /// <summary>
        /// Executes select statements and returns number of rows as datatable.
        /// </summary>
        /// <param name="commandText">Sql statement or name of procedure to be executed</param>
        /// <param name="commandType">Type of command i.e. Sql query, Stored procedure or table direct</param>
        /// <param name="parameters">Optional parameter accepts array of SqlParameters</param>
        /// <returns>number of rows as datatable</returns>
        public DataTable Select(string connectionString, string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            var result = new DataTable();

            try
            {
                using (var helper = new SqlConnectionHelper(connectionString))
                {
                    var connection = helper.GetConnection();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = commandType;
                        command.CommandTimeout = 300;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        using (var reader = command.ExecuteReader())
                        {
                            result.Load(reader);
                        }
                        command.Parameters.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {

                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Executes select statements and returns one or more tables as a dataset.
        /// </summary>
        /// <param name="commandText">Sql statement or name of procedure to be executed</param>
        /// <param name="commandType">Type of command i.e. Sql query, Stored procedure or table direct</param>
        /// <param name="parameters">Optional parameter accepts array of SqlParameters</param>
        /// <returns>returns one or more tables as a dataset</returns>
        public DataSet Multiple(string connectionString, string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            var result = new DataSet();
            using (var helper = new SqlConnectionHelper(connectionString))
            {
                var connection = helper.GetConnection();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = 300;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(result);
                    }
                    command.Parameters.Clear();
                }
            }
            return result;
        }
    }
}