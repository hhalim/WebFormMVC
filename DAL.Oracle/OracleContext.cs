//using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using Domain.Repository;

namespace DAL.Oracle
{
    //Unit Of Work for Oracle
    public class OracleContext : IDbContext
    {
        private bool _isDisposed;
        private string _connectionString;
        private DbTransaction _transaction { get; set; }
        private DbConnection _connection { get; set; }

        public OracleContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection OpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                //Need Oracle.ManagedDataAccess.Client from Oracle
                //_connection = new OracleConnection(_connectionString);
                _connection.Open();
            }

            return _connection;
        }

        public void CloseConnection()
        {
            _connection.Close();
        }

        public void BeginTransaction()
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction();
        }

        public DbConnection GetConnection()
        {
            return _connection;
        }

        public DbCommand GetCommand()
        {
            OpenConnection();
            return _connection.CreateCommand();
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _connection.Close();
        }

        public void Commit()
        {
            _transaction.Commit();
            _connection.Close();
        }

        public void Dispose()
        {
            if (_isDisposed || _connection == null)
                return;

            _connection.Dispose();

            _isDisposed = true;
        }
    }
}
