using Dara.DAL.Dtos;
using Dara.DAL.Utils;
using Dara.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog.Context;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dara.Web.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment _hostiongEnvironment;
        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment hostiongEnvironment)
        {
            this.next = next;
            this._hostiongEnvironment = hostiongEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (TranslateBusinessException err)
            {
                var code = HttpStatusCode.BadRequest;
                var result = JsonConvert.SerializeObject(new { key = err.Message });
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                var errorCode = UniqueIdentifierHelper.NewId();
                LogHelper.Error(ex, errorCode);
                await HandleExceptionAsync(context, ex, errorCode);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, string errorCode)
        {
            var code = HttpStatusCode.InternalServerError;
            LogContext.PushProperty("UserId", context.User.GetUserId());

            if (exception is NotImplementedException)
            {
                code = HttpStatusCode.NotFound;
            }
            else if (exception is UnauthorizedAccessException)
            {
                code = HttpStatusCode.Unauthorized;
            }

            var result = JsonConvert.SerializeObject(new
            {
                errCode = errorCode,
                error = this._hostiongEnvironment.IsDevelopment() ? exception.Message : ""
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
