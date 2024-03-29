﻿using System.Diagnostics;
using System.Reflection;
using form_builder.Controllers.HealthCheck.Models;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class HealthCheckController : Controller
    {
        [HttpGet]
        [Route("_healthcheck")]
        public IActionResult Get()
        {
            var name = Assembly.GetEntryAssembly()?.GetName().Name;
            var assembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "form-builder.dll");
            var version = FileVersionInfo.GetVersionInfo(assembly).FileVersion;

            return Ok(new HealthCheckModel
            {
                AppVersion = version,
                Name = name
            });
        }
    }
}