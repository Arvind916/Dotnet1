const { test, expect } = require('@playwright/test');
const { downloadProgress } = require('./helpers');

test('direct lesson links preserve the repository base, query, and fragment', async ({ page, baseURL }) => {
    const requests = [];
    page.on('request', request => requests.push(request.url()));
    const target = new URL('concept/linq?quick=true#interview', baseURL).href;
    await page.goto(target);
    await expect(page.getByRole('heading', { name: 'LINQ & deferred execution', exact: true })).toBeVisible();
    await expect(page).toHaveURL(target);
    await expect(page.getByRole('button', { name: 'Quick notes', exact: true })).toHaveAttribute('aria-pressed', 'true');
    await expect(page.locator('.code-window')).toHaveCount(0);
    await page.reload();
    await expect(page.locator('.lesson-content')).toBeVisible();
    await expect(page).toHaveURL(target);

    await page.getByRole('searchbox', { name: 'Search concepts', exact: true }).fill('async');
    await page.getByRole('searchbox', { name: 'Search concepts', exact: true }).press('Enter');
    await expect(page).toHaveURL(new URL('library?q=async', baseURL).href);
    await expect(page.locator('.concept-card')).not.toHaveCount(0);
    const escapedLinks = await page.locator('a[href]').evaluateAll((links, base) =>
        links.map(link => link.href).filter(href => href.startsWith(location.origin) && !href.startsWith(base)), baseURL);
    expect(escapedLinks).toEqual([]);
    expect(requests.filter(url => /\/(api|_blazor)(\/|\?|$)/.test(new URL(url).pathname))).toEqual([]);
});

test('lesson section links stay on the current lesson', async ({ page, baseURL }) => {
    await page.goto('./concept/linq');
    await page.locator('.lesson-outline').getByRole('link', { name: 'Interview questions', exact: true }).click();
    await expect(page).toHaveURL(new URL('concept/linq#interview', baseURL).href);
    await expect(page.getByRole('heading', { name: 'LINQ & deferred execution', exact: true })).toBeVisible();
});

test('concurrent tabs preserve both completion and bookmarks', async ({ page, context }) => {
    await page.goto('./concept/csharp-types');
    const secondPage = await context.newPage();
    await secondPage.goto('./concept/csharp-types');
    await Promise.all([
        page.getByRole('button', { name: 'Mark complete', exact: true }).first().click(),
        secondPage.getByRole('button', { name: 'Bookmark lesson', exact: true }).click()
    ]);
    await expect(page.getByRole('button', { name: 'Completed', exact: true })).toBeVisible();
    await expect(secondPage.getByRole('button', { name: 'Remove bookmark', exact: true })).toBeVisible();
    await page.goto('./progress');
    const saved = await downloadProgress(page);
    expect(saved.progress[0]).toMatchObject({ isCompleted: true, isBookmarked: true });
    expect(saved.history.filter(item => item.kind === 'Read')).toHaveLength(1);
    await secondPage.close();
});

test('a failed browser save reports an error without claiming completion', async ({ page }) => {
    await page.goto('./concept/csharp-types');
    const complete = page.getByRole('button', { name: 'Mark complete', exact: true }).first();
    await expect(complete).toBeEnabled();
    await page.evaluate(() => {
        window.indexedDB.open = () => { throw new DOMException('Simulated storage quota failure', 'QuotaExceededError'); };
    });
    await complete.click();
    await expect(page.locator('.error-note')).toContainText('could not be saved in this browser');
    await expect(page.getByRole('button', { name: 'Completed', exact: true })).toHaveCount(0);
    await page.reload();
    await expect(page.getByRole('button', { name: 'Mark complete', exact: true }).first()).toBeEnabled();
    await expect(page.locator('.error-note')).toHaveCount(0);
});