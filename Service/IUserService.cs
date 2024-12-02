using ApiPhantom.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPhantom.Service.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<(bool Success, string Message)> AddUserAsync(User user);
        Task<UserInterceptionsDto> GetUserInterceptionsAsync(Guid userId);
    }
}
