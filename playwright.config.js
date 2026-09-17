const { defineConfig } = require('@playwright/test');

const basePath = (process.env.FIELDNOTES_BASE_PATH || '/dotnet-fieldnotes/').replace(/\/+$/, '') + '/';
const port = process.env.FIELDNOTES_PREVIEW_PORT || '5270';
const baseURL = `http://127.0.0.1:${port}${basePath}`;

module.exports = defineConfig({
    testDir: './tests/browser',
    fullyParallel: false,
    workers: 1,
    timeout: 60000,
    expect: { timeout: 15000 },
    forbidOnly: Boolean(process.env.CI),
    retries: process.env.CI ? 1 : 0,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL,
        browserName: 'chromium',
        viewport: { width: 1440, height: 1000 },
        colorScheme: 'light',
        reducedMotion: 'reduce',
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure'
    },
    webServer: {
        command: `node scripts/serve-pages.mjs artifacts/pages/wwwroot ${port} ${basePath}`,
        url: baseURL,
        reuseExistingServer: !process.env.CI,
        timeout: 30000
    }
});