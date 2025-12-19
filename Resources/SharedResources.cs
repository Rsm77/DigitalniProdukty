namespace DigitalniProdukty;

// Marker typ pro sdílené lokalizační řetězce napříč aplikací.
public sealed class SharedResources;

/*
Podrobnosti (vazby a použité části)

- Účel: slouží jako „anchor“ typ pro `IStringLocalizer<SharedResources>`.
- Použití v aplikaci:
	- Controllery a služby používají `IStringLocalizer<SharedResources>` pro společné UI texty.
	- Překlady jsou v [Resources/SharedResources.resx](Resources/SharedResources.resx) a jazykových variantách.
*/

