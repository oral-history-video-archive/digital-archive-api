using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using SendGrid;
using SendGrid.Helpers.Mail;

using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides in-application feedback.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly List<EmailAddress> feedbackRecipients;
        private readonly EmailAddress feedbackMailFrom;
        private readonly string feedbackSubject;
        private readonly string sendGridApiKey;

        public FeedbackController(IConfiguration configuration)
        {
            var recipients = configuration?.GetSection("Feedback:MailRecipients").Get<List<string>>();
            feedbackRecipients = new List<EmailAddress>();
            foreach (var recipient in recipients)
            {
                feedbackRecipients.Add(new EmailAddress(recipient));
            }

            var from = configuration?["Feedback:MailFrom"];

#if SCIENCEMAKERS_ONLY
            feedbackMailFrom = new EmailAddress(from, "ScienceMakers Feedback");
#else
            feedbackMailFrom = new EmailAddress(from, "Digital Archive Feedback");
#endif

            feedbackSubject = configuration?["Feedback:MailSubject"];
            sendGridApiKey = configuration?["Feedback:SendGridApiKey"];
        }

        /// <summary>
        /// Receives end-user feedback from the application and mails it to the development team.
        /// </summary>
        /// <param name="feedback">A JSON document containing the end-user's comment and runtime environment information.</param>
        /// <returns>Status code from the SendGrid service.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Feedback feedback)
        {
            if (feedback == null)
            {
                return BadRequest();
            }

            // Using SendGrid's C# Library
            // https://github.com/sendgrid/sendgrid-csharp

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                feedbackMailFrom,
                feedbackRecipients, 
                feedbackSubject, 
                feedback.ToString(), 
                feedback.ToHTML()
            );

            var client = new SendGridClient(sendGridApiKey);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            return StatusCode((int)response.StatusCode);
        }
    }
}