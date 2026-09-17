(() => {
    const storageKey = "fieldnotes.theme";
    const root = document.documentElement;
    const systemPreference = window.matchMedia("(prefers-color-scheme: dark)");
    let explicitPreference = readPreference();

    function readPreference() {
        try {
            const value = localStorage.getItem(storageKey);
            return value === "dark" || value === "light" ? value : null;
        } catch {
            return null;
        }
    }

    function syncControls() {
        const dark = root.dataset.theme === "dark";
        for (const button of document.querySelectorAll("[data-theme-toggle]")) {
            const label = dark ? "Switch to light mode" : "Switch to dark mode";
            button.setAttribute("aria-pressed", String(dark));
            button.setAttribute("aria-label", label);
            button.title = label;
        }
    }

    function applyTheme(theme) {
        root.dataset.theme = theme;
        root.style.colorScheme = theme;
        const meta = document.querySelector('meta[name="theme-color"]');
        if (meta) meta.content = theme === "dark" ? "#181d1b" : "#fafbfa";
        syncControls();
    }

    applyTheme(explicitPreference ?? (systemPreference.matches ? "dark" : "light"));

    document.addEventListener("click", event => {
        if (!(event.target instanceof Element) || !event.target.closest("[data-theme-toggle]")) return;
        explicitPreference = root.dataset.theme === "dark" ? "light" : "dark";
        applyTheme(explicitPreference);
        try {
            localStorage.setItem(storageKey, explicitPreference);
        } catch {
            root.dataset.theme = explicitPreference;
        }
    });

    systemPreference.addEventListener("change", event => {
        if (explicitPreference === null) applyTheme(event.matches ? "dark" : "light");
    });

    window.addEventListener("storage", event => {
        if (event.key !== storageKey && event.key !== null) return;
        explicitPreference = readPreference();
        applyTheme(explicitPreference ?? (systemPreference.matches ? "dark" : "light"));
    });

    new MutationObserver(syncControls).observe(root, { childList: true, subtree: true });
    document.addEventListener("DOMContentLoaded", syncControls);
})();