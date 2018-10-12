using System.Collections.Generic;
using Domain.Models;

namespace Domain.Services
{
    public interface IUserService
    {
        IEnumerable<User> Get();
    }
}
