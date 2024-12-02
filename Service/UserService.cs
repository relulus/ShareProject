using ApiPhantom.DAL;
using ApiPhantom.Models;
using ApiPhantom.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPhantom.Service
{
    public class UserService : IUserService
    {
        private readonly ApiPhantomContext _context;

        public UserService(ApiPhantomContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<(bool Success, string Message)> AddUserAsync(User user)
        {
            if (await _context.User.AnyAsync(u => u.Username == user.Username))
                return (false, "Username already exists.");

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User added successfully.");
        }

        public async Task<UserInterceptionsDto> GetUserInterceptionsAsync(Guid userId)
        {
            var user = await _context.User
                .Include(u => u.InterceptedServices)
                .ThenInclude(s => s.InterceptedApis)
                .ThenInclude(a => a.ApiCatalog)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return new UserInterceptionsDto
            {
                UserId = user.Id,
                Username = user.Username,
                Services = user.InterceptedServices
                    .Select(s => new ServiceInterceptionDto
                    {
                        ServiceId = s.Id,
                        ServiceName = s.ServiceCatalog.Name,
                        Apis = s.InterceptedApis
                            .Select(a => new ApiInterceptionDto
                            {
                                ApiId = a.Id,
                                ApiPath = a.ApiCatalog.Path,
                                ApiMethod = a.ApiCatalog.Method
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }
    }
}
