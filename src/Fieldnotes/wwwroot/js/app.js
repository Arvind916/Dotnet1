window.fieldnotes = {
    highlight(element) {
        if (window.Prism) {
            window.Prism.highlightAllUnder(element);
        }
    },
    async copy(text) {
        if (!navigator.clipboard) {
            throw new Error("Clipboard access requires a secure browser context.");
        }
        await navigator.clipboard.writeText(text);
    }
};