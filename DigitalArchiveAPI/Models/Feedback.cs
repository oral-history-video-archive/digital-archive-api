using System;

namespace DigitalArchiveAPI.Models
{
    /// <summary>
    /// Data structure used by client to send end-user feedback.
    /// </summary>
    public class Feedback
    {
        /// <summary>
        /// End-user submitted comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// End-user's browser navigator.platform value.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// End-user's browser navigator.userAgent value.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// End-user's browser navigator.language value.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// End-user's browser window.width x window.height value.
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// End-user's system date at time of feedback submission.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// End-user's browser windows.location value.
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            // Sacrificing a bit of performance for readability, this call has low usage.
            return string.Join("\r\n\r\n", new [] {
                $"Comments: {Comments}",
                $"Platform: {Platform}",
                $"UserAgent: {UserAgent}",
                $"Language: {Language}",
                $"Resolution: {Resolution}",
                $"Date: {Date}",
                $"URL: {URL}"
            });
        }

        public string ToHTML()
        {
            // Sacrificing a bit of performance for readability, this call has low usage.
            return string.Join("<br/><br/>", new[] {
                $"<strong>Comments:</strong> {Comments}",
                $"<strong>Platform:</strong> {Platform}",
                $"<strong>UserAgent:</strong> {UserAgent}",
                $"<strong>Language:</strong> {Language}",
                $"<strong>Resolution:</strong> {Resolution}",
                $"<strong>Date:</strong> {Date}",
                $"<strong>URL:</strong> <a href='{URL}'>{URL}</a>"
            });
        }
    }
}
