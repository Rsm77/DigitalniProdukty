namespace DigitalniProdukty.Services;

public interface IEmailSender
{
        // Odešle HTML email; implementace může být reálná (SMTP/API) nebo no-op pro vývoj.
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}

/*
Podrobnosti (vazby a použité části)

- Účel: malá abstrakce pro odesílání emailů bez závislosti na konkrétním provideru.
- Vazby na zbytek aplikace:
    - `AuthEmailService` posílá přes IEmailSender emaily pro potvrzení a reset hesla.
    - V `Program.cs` je typicky zaregistrovaná implementace `NoOpEmailSender` (loguje místo odeslání).
- Poznámka: pokud nasadíte produkční email, přidejte novou implementaci a přepněte registraci v DI.
*/

