using System;
using System.Collections;
using System.Collections.Generic;
using Domain.Repository;
using Domain.Models;

namespace DAL.MSSQL
{
    public class UserRepository : IUserRepository
    {
        private List<User> fakeUsers = new List<User>();
        private readonly IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;

            fakeUsers.Add(new User { UserID = 1, FirstName = "user1", LastName = "sql-server" });
            fakeUsers.Add(new User { UserID = 2, FirstName = "user2", LastName = "sql-server" });
            fakeUsers.Add(new User { UserID = 3, FirstName = "user3", LastName = "sql-server" });
            fakeUsers.Add(new User { UserID = 4, FirstName = "user4", LastName = "sql-server" });
        }

        public IEnumerable<User> Get()
        {
            //TODO: Use IDBContext and get data
            return fakeUsers;
        }
    }
}
