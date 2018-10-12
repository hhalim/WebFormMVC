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
        private List<User> fakeUsers;
        public UserRepository()
        {
            fakeUsers.Add(new User { UserID = 1, FirstName = "user1", LastName = "oracle_lastname"});
            fakeUsers.Add(new User { UserID = 2, FirstName = "user2", LastName = "oracle_lastname"});
            fakeUsers.Add(new User { UserID = 3, FirstName = "user3", LastName = "oracle_lastname"});
            fakeUsers.Add(new User { UserID = 4, FirstName = "user4", LastName = "oracle_lastname"});

        }

        public IEnumerable<User> Get()
        {
            //TODO: Use IDBContext and get data
            return fakeUsers;
        }
    }
}
