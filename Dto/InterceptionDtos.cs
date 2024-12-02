using System;
using System.Collections.Generic;

namespace ApiPhantom.Models
{
    public class UserInterceptionsDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public List<ServiceInterceptionDto> Services { get; set; }
    }

    public class ServiceInterceptionDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public List<ApiInterceptionDto> Apis { get; set; }
    }

    public class ApiInterceptionDto
    {
        public Guid ApiId { get; set; }
        public string ApiPath { get; set; }
        public string ApiMethod { get; set; }
    }
}
