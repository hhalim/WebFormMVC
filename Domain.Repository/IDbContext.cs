using System;
using System.Data.Common;

namespace Domain.Repository
{
    //UnitOfWork
    public interface IDbContext : IDisposable
    {
        DbCommand GetCommand();
        DbConnection GetConnection();
        DbConnection OpenConnection();
        void CloseConnection();
        void BeginTransaction();
        void Rollback();
        void Commit();
    }
}
