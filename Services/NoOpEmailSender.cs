namespace DigitalniProdukty.Services;

public sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    // Vývojová implementace: místo odeslání jen zaloguje, že by email poslala.
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            logger.LogInformation("NoOpEmailSender: would send email to {Email} with subject '{Subject}'.", email, subject);
        }
        return Task.CompletedTask;
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: bezpečný default pro vývoj/dema (nevynáší emaily ven, ale dává stopu do logu).
- Vazby na zbytek aplikace:
  - Registrováno v DI v `Program.cs` jako implementace `IEmailSender`.
  - Používáno nepřímo přes `AuthEmailService`.
- Poznámka: pro produkci nahraďte reálnou implementací (SMTP/API) a logujte jen metadata.
*/

