namespace DigitalniProdukty.Models
{
        // ViewModel pro chybovou stránku (typicky Error view v MVC).
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

                // Určuje, zda má UI vůbec zobrazit RequestId (jen když existuje).
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: přenést do view informace o chybě, hlavně `RequestId` pro dohledání v logu.
- Použité „funkce“:
    - `ShowRequestId` je odvozená vlastnost pro jednoduchou podmínku ve view.
- Vazby na zbytek aplikace:
    - Typicky použito v `HomeController` / default error pipeline a view `Views/Shared/Error.cshtml`.
    - `RequestId` odpovídá `Activity.Current?.Id` nebo `HttpContext.TraceIdentifier` (podle implementace error handleru).
*/

