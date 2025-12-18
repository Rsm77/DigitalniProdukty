// Poznámka: Tento soubor je záměrně „vanilla JS“ (bez frameworku).
// Slouží jako tenká vrstva pro:
// - ovládání mobilního sidebaru,
// - synchronizaci aktivního stavu navigace při HTMX navigaci,
// - řízení scrollu při HTMX swapu hlavního obsahu,
// - drobné HTMX výjimky (422 swap),
// - reload login partialu po auth akcích,
// - a lokální dialog pro detaily licenčního klíče.

// Mobile menu toggle (sidebar na malých displejích)
document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('-translate-x-full');
        });

        // Close the sidebar after clicking a navigation link (mobile)
        sidebar.addEventListener('click', function (evt) {
            const link = evt.target?.closest?.('a');
            if (!link) return;
            sidebar.classList.add('-translate-x-full');
        });
    }

    // Initial active nav state (důležité pro první vykreslení)
    updateSidebarActiveNav();
});

// Normalizace URL cesty pro porovnávání aktivní položky navigace.
function normalizePath(pathname) {
    if (!pathname) return '/';
    // Normalize common MVC default
    if (pathname === '/Home/Index') return '/';
    // Strip trailing slash (except root)
    if (pathname.length > 1 && pathname.endsWith('/')) return pathname.slice(0, -1);
    return pathname;
}

// Označí aktivní položku navigace v sidebaru podle aktuální URL.
function updateSidebarActiveNav() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    const navLinks = Array.from(sidebar.querySelectorAll('nav a[href]'));
    if (navLinks.length === 0) return;

    const current = normalizePath(window.location?.pathname || '/');

    for (const a of navLinks) {
        a.classList.remove('app-nav-link-active');
    }

    const match = navLinks.find(a => {
        try {
            const url = new URL(a.getAttribute('href'), window.location.origin);
            const linkPath = normalizePath(url.pathname);
            return linkPath === current;
        } catch {
            return false;
        }
    });

    if (match) {
        match.classList.add('app-nav-link-active');
    }
}

// Keep sidebar highlight in sync with HTMX navigation (HTMX mění historii i obsah bez full reloadu)
document.body.addEventListener('htmx:pushedIntoHistory', updateSidebarActiveNav);
document.body.addEventListener('htmx:historyRestore', updateSidebarActiveNav);
document.body.addEventListener('htmx:afterSettle', updateSidebarActiveNav);
window.addEventListener('popstate', updateSidebarActiveNav);

// Prevent the browser from restoring scroll positions when we drive navigation via HTMX.
// Cíl: snížit „náhodné“ posuny scrollu při krátkých viewportech a HTMX swapech.
try {
    if (typeof history !== 'undefined' && 'scrollRestoration' in history) {
        history.scrollRestoration = 'manual';
    }
} catch {
    // ignore
}

// Okamžitě posune stránku na začátek (odolné vůči rozdílům mezi prohlížeči).
function forceScrollTop() {
    try {
        const scrollingElement = document.scrollingElement || document.documentElement;
        if (scrollingElement) scrollingElement.scrollTop = 0;
        document.body.scrollTop = 0;
    } catch {
        // ignore
    }

    try {
        window.scrollTo({ top: 0, left: 0, behavior: 'auto' });
    } catch {
        window.scrollTo(0, 0);
    }
}

// Naplánuje několik „scroll to top“ pokusů po sobě.
// Důvod: layout shift, focus a async render mohou po swapu pohnout stránkou.
function scheduleScrollTop() {
    // Do it a few times to survive layout shifts and focus changes.
    forceScrollTop();
    try {
        requestAnimationFrame(() => {
            forceScrollTop();
            requestAnimationFrame(forceScrollTop);
        });
    } catch {
        // ignore
    }

    setTimeout(forceScrollTop, 0);
}

// Aplikuje scroll-to-top jen po swapu hlavního obsahu (#main).
function scrollToTopAfterMainSwap(evt) {
    const target = evt?.detail?.target;
    if (!target || target.id !== 'main') return;

    // HTMX keeps scroll position by default; reset it so new content starts at the top.
    scheduleScrollTop();
}

// After boosted navigation swaps #main, ensure we start at the top.
// Pozn.: HTMX nav může vyměnit obsah bez full reloadu, proto to řešíme zde.
document.body.addEventListener('htmx:afterSwap', scrollToTopAfterMainSwap);
document.body.addEventListener('htmx:afterSettle', scrollToTopAfterMainSwap);
document.body.addEventListener('htmx:pushedIntoHistory', scrollToTopAfterMainSwap);
document.body.addEventListener('htmx:historyRestore', scrollToTopAfterMainSwap);

// Treat 422 (validation) as a normal swap so server-rendered form errors show up.
// Cíl: při validaci formuláře chceme zobrazit HTML s chybami v cíli swapu.
document.body.addEventListener('htmx:beforeSwap', function (evt) {
    const status = evt.detail?.xhr?.status;
    if (status !== 422) return;

    evt.detail.shouldSwap = true;
    evt.detail.isError = false;
});

// If auth state changes via HTMX POST, refresh the header login partial.
// Cíl: po login/logout/register chceme „header“ přepnout bez full reloadu.
document.body.addEventListener('htmx:afterRequest', function (evt) {
    const verb = evt.detail?.requestConfig?.verb?.toUpperCase?.();
    if (verb !== 'POST') return;

    const path = evt.detail?.pathInfo?.requestPath || '';
    if (
        path.startsWith('/Identity/Account/Login') ||
        path.startsWith('/Identity/Account/Logout') ||
        path.startsWith('/Identity/Account/Register') ||
        path.startsWith('/auth/login') ||
        path.startsWith('/auth/logout') ||
        path.startsWith('/auth/register') ||
        path.startsWith('/auth/login-2fa')
    ) {
        if (window.htmx) {
            htmx.trigger(document.body, 'auth-changed');
        } else {
            document.body.dispatchEvent(new CustomEvent('auth-changed'));
        }
    }
});

// Licensing details dialog (HTMX loaded)
// Malý helper pro otevření/zavření <dialog> a natažení detailu přes HTMX.
window.appLicensing = window.appLicensing || {};

// Otevře dialog a načte HTML detailu do #license-details-body.
window.appLicensing.openDetails = function (url) {
    const dlg = document.getElementById('license-details');
    const body = document.getElementById('license-details-body');
    if (!dlg || !body) return;

    body.innerHTML = '<div class="p-4 flex items-center justify-center">'
        + '<svg class="animate-spin h-4 w-4 text-slate-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">'
        + '<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>'
        + '<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>'
        + '</svg>'
        + '</div>';

    if (typeof dlg.showModal === 'function') {
        dlg.showModal();
    } else {
        dlg.setAttribute('open', 'open');
    }

    if (window.htmx && url) {
        htmx.ajax('GET', url, { target: '#license-details-body', swap: 'innerHTML' });
    }
};

// Zavře dialog.
window.appLicensing.closeDetails = function () {
    const dlg = document.getElementById('license-details');
    if (!dlg) return;
    if (typeof dlg.close === 'function') {
        dlg.close();
    } else {
        dlg.removeAttribute('open');
    }
};

/*
PODROBNÉ SHRNUTÍ (pro údržbu)

1) Sidebar (mobil)
    - Po načtení DOM se naváže handler na #sidebar-toggle.
    - Přepíná se třída "-translate-x-full" na #sidebar (Tailwind utilita) -> sidebar se zasouvá/vysouvá.
    - Klik na libovolný odkaz uvnitř sidebaru sidebar opět zavře (lepší UX na mobilu).

2) Aktivní položka navigace
    - Funkce normalizePath() znormalizuje URL (odstraní trailing slash, převádí /Home/Index na /).
    - updateSidebarActiveNav() porovná aktuální URL a hrefy v sidebaru a přidá třídu "app-nav-link-active".
    - Tohle je důležité i pro HTMX navigaci (bez full reloadu), proto posloucháme HTMX události.

3) Scroll management po HTMX swapech
    - Vypínáme history.scrollRestoration = manual, aby prohlížeč nevracel staré pozice scrollu.
    - forceScrollTop() + scheduleScrollTop() provádí několik pokusů o návrat na vršek.
    - scrollToTopAfterMainSwap() se spustí pouze když HTMX swapne element s id "main".

4) HTMX výjimka pro 422
    - U validace formulářů server někdy vrací 422.
    - HTMX by to defaultně bral jako error a neswapnul by obsah.
    - Handler vynutí swap (shouldSwap=true, isError=false), aby se HTML s chybami vykreslilo.

5) Refresh auth partialu
    - Po vybraných POST requestech (/auth/login, /auth/logout, /auth/register, /auth/login-2fa, …)
      vyvoláme event "auth-changed".
    - Header pak může reloadnout login/logout část (typicky přes HTMX fragment).

6) Licensing details <dialog>
    - window.appLicensing.openDetails(url):
         * vyplní body spinnerem,
         * otevře <dialog> (showModal(), fallback na atribut open),
         * a přes htmx.ajax načte URL do #license-details-body.
    - window.appLicensing.closeDetails(): zavře dialog.

Poznámky k bezpečnosti/robustnosti
    - Kód je defensivní: kontroluje existenci DOM elementů a umí pracovat i bez htmx (no-op).
    - Neuchovává žádný citlivý stav v JS; vše důležité je server-side.
*/
