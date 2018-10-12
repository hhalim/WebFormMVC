using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repository;

namespace DAL.Oracle
{
    public class UserRepository : IUserRepository
    {
        private List<User> fakeUsers = new List<User>();
        private readonly IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;

            fakeUsers.Add(new User { UserID = 1, FirstName = "user1", LastName = "oracle"});
            fakeUsers.Add(new User { UserID = 2, FirstName = "user2", LastName = "oracle"});
            fakeUsers.Add(new User { UserID = 3, FirstName = "user3", LastName = "oracle"});
            fakeUsers.Add(new User { UserID = 4, FirstName = "user4", LastName = "oracle"});
        }

        public IEnumerable<User> Get()
        {
            //TODO: Use IDBContext and get data
            return fakeUsers;
        }
    }
}
