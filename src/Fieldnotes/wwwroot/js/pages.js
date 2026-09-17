(() => {
    const current = new URL(window.location.href);
    const route = current.searchParams.get('__fieldnotes_route');
    if (!route) return;

    const base = new URL(document.baseURI);
    const target = new URL(route, base.origin);
    const allowed = target.origin === base.origin && target.pathname.startsWith(base.pathname);
    window.history.replaceState(null, '', allowed ? target.pathname + target.search + target.hash : base.pathname);
})();