using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebStoreAPI.Exceptions;

namespace WebStoreAPI.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                HttpResponse response = context.Response;
                response.ContentType = "application/json";
                
                switch (error)
                {
                    case BadRequestException:
                        response.StatusCode = (int) HttpStatusCode.BadRequest;
                        break;

                    case NotFoundException:
                        response.StatusCode = (int) HttpStatusCode.NotFound;
                        break;

                    default:
                        response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        break;
                }

                string result = JsonConvert.SerializeObject(new {message = error.Message});
                await response.WriteAsync(result);
            }
        }
    }
}