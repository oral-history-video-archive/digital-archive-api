using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalArchiveAPI.Models
{
    public class VersionInfo
    {
        /// <summary>
        /// Assembly version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Configured Azure Search Service
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Configured Azure Storage Account
        /// </summary>
        public string Storage { get; set; }

        /// <summary>
        /// Configured list of feedback recipients
        /// </summary>
        public List<string> MailTo { get; set; }
    }
}
