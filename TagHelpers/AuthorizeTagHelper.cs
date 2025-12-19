using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DigitalniProdukty.TagHelpers;

[HtmlTargetElement("authorize")]
public sealed class AuthorizeTagHelper(IAuthorizationService authorizationService) : TagHelper
{
    /// <summary>
    /// Název autorizační policy.
    /// </summary>
    [HtmlAttributeName("policy")]
    public string? Policy { get; set; }

    /// <summary>
    /// Seznam rolí oddělený čárkami. Pokud je zadaný, uživatel musí být alespoň v jedné z nich.
    /// </summary>
    [HtmlAttributeName("roles")]
    public string? Roles { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    // Rozhodne, zda se obsah vyrenderuje (dle loginu/rolí/policy), nebo se element potlačí.
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = ViewContext?.HttpContext?.User;

        // Výchozí chování: vyžaduje přihlášeného uživatele.
        var authorized = user?.Identity?.IsAuthenticated == true;

        if (authorized && !string.IsNullOrWhiteSpace(Roles))
        {
            var roles = Roles
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            authorized = roles.Any(role => user!.IsInRole(role));
        }

        if (authorized && !string.IsNullOrWhiteSpace(Policy))
        {
            var result = await authorizationService.AuthorizeAsync(user!, Policy);
            authorized = result.Succeeded;
        }

        if (!authorized)
        {
            output.SuppressOutput();
            return;
        }

        // Vyrenderuje pouze potomky (bez obalového <authorize> elementu).
        output.TagName = null;
        output.Attributes.Clear();
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: jednoduchá deklarativní autorizace v Razor views přes `<authorize ...>`.
- Vstupy:
    - `roles`: alespoň jedna role musí sedět.
    - `policy`: spustí `IAuthorizationService.AuthorizeAsync` nad danou policy.
- Chování:
    - Když autorizace neprojde, použije `SuppressOutput()` a nic se nevyrenderuje.
    - Když autorizace projde, odstraní obalový tag a nechá vyrenderovat jen children.
- Vazby:
    - Políčka `roles`/`policy` typicky odkazují na konstanty v `Security/Authz`.
*/
