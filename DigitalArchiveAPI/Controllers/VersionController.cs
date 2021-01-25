using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using DigitalArchiveAPI.Models;
using Microsoft.Extensions.Configuration;

namespace DigitalArchiveAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly VersionInfo versionInfo = new VersionInfo();

        public VersionController(IConfiguration configuration)
        {
            versionInfo.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            versionInfo.Search  = configuration?["AzureSearch:ServiceName"];
            versionInfo.MailTo  = configuration?.GetSection("Feedback:MailRecipients").Get<List<string>>();
            versionInfo.Storage = 
                configuration?["AzureStorage:ConnectionString"].Split(";")?.Select(p => 
                {
                    string[] s = p.Split('=');
                    return (key: s[0], value: s[1]);
                })
                .Where(t => t.key == "AccountName")
                .Select(t => t.value)
                .SingleOrDefault();
        }

        /// <summary>
        /// Get current build number and configuration information.
        /// </summary>
        /// <returns>JSON containing the version info.</returns>
        [HttpGet]
        public VersionInfo Get()
        {
            return versionInfo;
        }
    }
}
