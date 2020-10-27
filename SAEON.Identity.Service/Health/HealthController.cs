using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SAEON.Identity.Service.Data;
using SAEON.Identity.Service.Health;
using SAEON.Logs;
using System;

namespace SAEON.Identity.Service.HealthCheck
{
    [Route("[controller]")]
    public class HealthController : Controller
    {
        private readonly SAEONDbContext _context = null;

        public HealthController(SAEONDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (SAEONLogs.MethodCall(GetType()))
            {
                var model = new HealthModel();
                try
                {
                    if (!(_context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                    {
                        model.Database = "Not exist";
                        model.Healthy = false;
                    }
                }
                catch (Exception ex)
                {
                    SAEONLogs.Exception(ex);
                    model.Database = ex.Message;
                    model.Healthy = false;
                }
                return Ok(model);
            }
        }
    }
}
