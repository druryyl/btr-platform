using System;
using System.Reflection;
using System.Web.Http;
using btr.portal.api.Models;
using NLog;

namespace btr.portal.api.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            Logger.Info("Health check requested");
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

            var data = new HealthData
            {
                Status = "ok",
                Service = "btr.portal.api",
                Version = version,
                TimestampUtc = DateTime.UtcNow
            };

            return Ok(ApiResponse<HealthData>.Success(data));
        }
    }

    public class HealthData
    {
        public string Status { get; set; }
        public string Service { get; set; }
        public string Version { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
