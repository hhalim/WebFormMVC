using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Services;
using Domain.Repository;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            //use DI to inject the correct repository (SQL or Oracle)
            _userRepository = userRepository;
        }

        public IEnumerable<User> Get()
        {
            return _userRepository.Get();
        }
    }
}
