﻿using Domain.Repository;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DAL.MSSQL
{
    //TODO: Refactor using Dapper.ORM
    public class SqlContext : IDbContext
    {
        private bool _isDisposed;
        private string _connectionString;
        private DbTransaction _transaction { get; set; }
        private DbConnection _connection { get; set; }

        public SqlContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection OpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
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
