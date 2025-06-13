using Microsoft.AspNetCore.Identity.UI.Services;

namespace Web.Components.Account
{
    // Email sender that does nothing - just for development
    internal sealed class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage) => Task.CompletedTask;
    }
}