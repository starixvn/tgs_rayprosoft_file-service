using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using TGS.Core.Infrastructures.Extensions;
using Tgs.Logging;

namespace FileService.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITgsLog log)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var endpoint = context.GetEndpoint();
                var name = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.MethodInfo.Name;

                var message = exception.GetMessageChain();
                log.SetLogger(name);
                log.LogError($"File service error on :{context.Request.Host} with message: {message}", exception);
            }
        }
    }
}