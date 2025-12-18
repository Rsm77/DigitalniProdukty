using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace DigitalniProdukty.Services;

public sealed class TokenCodec
{
    public string Encode(string token)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token ?? string.Empty));

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

