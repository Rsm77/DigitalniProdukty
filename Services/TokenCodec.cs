using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace DigitalniProdukty.Services;

public sealed class TokenCodec
{
    // Zakóduje token do Base64Url, aby byl bezpečný v query stringu (bez problémových znaků).
    public string Encode(string token)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token ?? string.Empty));

    // Pokusí se Base64Url dekódovat; při chybě vrátí původní hodnotu (kvůli robustnosti).
    // Hodí se při migraci nebo ručním copy/paste tokenu uživatelem.
    public string DecodeOrRaw(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded)) return string.Empty;

        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encoded));
        }
        catch
        {
            return encoded;
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: jednoduché (de)kódování tokenů do URL-safe formátu.
- Použité API:
    - Microsoft.AspNetCore.WebUtilities.WebEncoders: `Base64UrlEncode`/`Base64UrlDecode`.
- Vazby na zbytek aplikace:
    - `AuthEmailService` používá `Encode` pro token v potvrzovacím/reset odkazu.
    - `Controllers/AuthController` používá `DecodeOrRaw` při zpracování potvrzení emailu a resetu hesla.
*/

