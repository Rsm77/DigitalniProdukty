namespace DigitalniProdukty.Services;

public sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            logger.LogInformation("NoOpEmailSender: would send email to {Email} with subject '{Subject}'.", email, subject);
        }
        return Task.CompletedTask;
    }
}

