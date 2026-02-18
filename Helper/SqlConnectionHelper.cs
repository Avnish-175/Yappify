using System.Data.SqlClient;

namespace ChatApp2.Helper
{
    /// <summary>
    /// 
    /// </summary>
    internal class SqlConnectionHelper : IDisposable
    {
        private SqlConnection _connection = null;

        /// <summary>
        /// default ctor
        /// </summary>
        public SqlConnectionHelper(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(connectionString);

                _connection.Open();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Terminate()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~SqlConnectionHelper()
        {
            Dispose(false);
        }

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Terminate();

                }
                _disposed = true;
            }
        }

        #endregion
    }
}