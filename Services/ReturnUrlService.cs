using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Services;

public sealed class ReturnUrlService
{
    public string GetSafeReturnUrl(IUrlHelper urlHelper, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && urlHelper.IsLocalUrl(returnUrl)
            && !IsFragmentReturnUrl(returnUrl))
        {
            return returnUrl;
        }

        var home = urlHelper.Content("~/");
        return string.IsNullOrWhiteSpace(home) ? "/" : home;
    }

    public bool IsFragmentReturnUrl(string returnUrl)
    {
        var trimmed = returnUrl.Trim();
        if (trimmed.Length == 0) return false;

        return trimmed.StartsWith("/_fragments", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("_fragments", StringComparison.OrdinalIgnoreCase);
    }
}

