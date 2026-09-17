(() => {
    const base = new URL(document.baseURI);
    const current = new URL(window.location.href);
    if (current.origin !== base.origin || !current.pathname.startsWith(base.pathname)) return;

    if (current.pathname !== `${base.pathname}404.html`) {
        base.searchParams.set('__fieldnotes_route', current.pathname + current.search + current.hash);
    }
    window.location.replace(base.href);
})();