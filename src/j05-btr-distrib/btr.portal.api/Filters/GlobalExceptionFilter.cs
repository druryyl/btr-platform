using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.portal.api.Models;
using NLog;

namespace btr.portal.api.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception == null)
                return;

            var exception = context.Exception;
            Logger.Error(exception, "Unhandled exception processing {Method} {Uri}",
                context.Request?.Method,
                context.Request?.RequestUri);

            var (statusCode, message) = MapException(exception);
            var envelope = ApiResponse<object>.Error((int)statusCode, message);

            context.Response = context.Request.CreateResponse(statusCode, envelope);
        }

        private static (HttpStatusCode statusCode, string message) MapException(Exception exception)
        {
            if (exception is ArgumentException || exception is ValidationException)
                return (HttpStatusCode.BadRequest, exception.Message);

            if (exception is KeyNotFoundException)
                return (HttpStatusCode.NotFound, exception.Message);

            if (exception is UnauthorizedAccessException)
                return (HttpStatusCode.Unauthorized, exception.Message);

            if (exception is DashboardSnapshotUnavailableException)
                return (HttpStatusCode.ServiceUnavailable, exception.Message);

            return (HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
