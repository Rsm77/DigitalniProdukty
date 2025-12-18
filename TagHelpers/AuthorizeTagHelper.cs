using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DigitalniProdukty.TagHelpers;

[HtmlTargetElement("authorize")]
public sealed class AuthorizeTagHelper(IAuthorizationService authorizationService) : TagHelper
{
    /// <summary>
    /// Authorization policy name.
    /// </summary>
    [HtmlAttributeName("policy")]
    public string? Policy { get; set; }

    /// <summary>
    /// Comma-separated list of roles. If specified, user must be in at least one role.
    /// </summary>
    [HtmlAttributeName("roles")]
    public string? Roles { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = ViewContext?.HttpContext?.User;

        // Default behavior: require authenticated user.
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

        // Render only children (no wrapping <authorize> element).
        output.TagName = null;
        output.Attributes.Clear();
    }
}
