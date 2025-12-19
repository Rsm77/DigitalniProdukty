namespace DigitalniProdukty;

// Marker typ pro lokalizaci řetězců Identity UI.
public sealed class IdentityUi;

/*
Podrobnosti (vazby a použité části)

- Účel: slouží jako „anchor“ typ pro `IStringLocalizer<IdentityUi>`.
- Použití v aplikaci:
	- `AuthController` a další části Identity flow používají `IStringLocalizer<IdentityUi>`.
	- Překlady jsou v [Resources/IdentityUi.resx](Resources/IdentityUi.resx) a jazykových variantách.
*/

