const { test, expect } = require('@playwright/test');
const path = require('node:path');

test('theme preference is accessible, persistent, and synchronized across tabs', async ({ page, context }) => {
    await page.goto('./');
    await page.getByRole('button', { name: 'Switch to dark mode' }).click();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await expect(page.getByRole('button', { name: 'Switch to light mode' })).toHaveAttribute('aria-pressed', 'true');
    expect(await page.evaluate(() => localStorage.getItem('fieldnotes.theme'))).toBe('dark');
    await page.reload();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await page.getByRole('link', { name: 'Concept library 30', exact: true }).click();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    const secondPage = await context.newPage();
    await secondPage.goto('./');
    await expect(secondPage.locator('html')).toHaveAttribute('data-theme', 'dark');
    await page.getByRole('button', { name: 'Switch to light mode' }).click();
    await expect(secondPage.locator('html')).toHaveAttribute('data-theme', 'light');
    await secondPage.close();
});

test('system dark preference is honored until the user chooses a theme', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.goto('./');
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await page.getByRole('button', { name: 'Switch to light mode' }).click();
    await page.emulateMedia({ colorScheme: 'light' });
    await page.emulateMedia({ colorScheme: 'dark' });
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
    await page.reload();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
});

for (const theme of ['light', 'dark']) {
    for (const viewport of [{ name: 'desktop', width: 1440, height: 1000 }, { name: 'mobile', width: 390, height: 844 }]) {
        test(`${theme} ${viewport.name}: all pages fit and load their assets`, async ({ page }) => {
            const errors = [];
            page.on('pageerror', error => errors.push(error.message));
            await page.setViewportSize({ width: viewport.width, height: viewport.height });
            await page.emulateMedia({ colorScheme: theme });

            for (const route of ['./', './library', './concept/linq', './practice', './revision?mode=all', './progress']) {
                await page.goto(route);
                await expect(page.locator('.workspace')).toBeVisible();
                await expect(page.locator('.main-content h1')).toBeVisible();
                if (route === './') await expect(page.locator('.stats-band')).toBeVisible();
                if (route === './concept/linq') await expect(page.locator('pre code .token').first()).toBeVisible();
                await page.evaluate(async () => {
                    await document.fonts.ready;
                    await Promise.all([...document.images].map(image => image.decode()));
                });
                await expect(page.locator('html')).toHaveAttribute('data-theme', theme);
                const overflow = await page.evaluate(() => document.documentElement.scrollWidth > innerWidth + 1);
                expect(overflow, `Horizontal overflow at ${route}`).toBe(false);
                await expect(page.locator('.error-note')).toHaveCount(0);
                await expect(page.locator('#blazor-error-ui')).not.toBeVisible();
                const maskUrls = await page.locator('.icon').evaluateAll(icons => [...new Set(icons.map(icon => {
                    const mask = getComputedStyle(icon).maskImage;
                    return mask.startsWith('url(') ? mask.slice(5, -2) : null;
                }).filter(Boolean))]);
                for (const url of maskUrls) {
                    expect((await page.request.get(url)).ok(), `Missing icon ${url}`).toBe(true);
                }
                if (route === './' || (theme === 'dark' && route === './concept/linq')) {
                    await page.evaluate(() => {
                        if (document.activeElement instanceof HTMLElement) document.activeElement.blur();
                        window.scrollTo(0, 0);
                    });
                    const screenshotName = `${route === './' ? 'overview' : 'lesson'}-${theme}-${viewport.name}.png`;
                    await page.screenshot({ path: path.join('docs', 'screenshots', screenshotName) });
                }
            }

            expect(errors).toEqual([]);
        });
    }
}