using System.Collections.Generic;
using Domain.Models;

namespace Domain.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> Get();
    }
}
