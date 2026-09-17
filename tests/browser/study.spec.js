const { test, expect } = require('@playwright/test');
const { downloadProgress } = require('./helpers');

test('lesson, quiz, flashcard, and progress survive reload', async ({ page }) => {
    await page.goto('./library');
    await page.getByRole('searchbox', { name: 'Search the library' }).fill('Value & reference');
    await expect(page.locator('.concept-card')).toHaveCount(1);
    await page.getByRole('button', { name: 'Bookmark Value & reference types', exact: true }).click();
    await expect(page.getByRole('button', { name: 'Remove bookmark for Value & reference types' })).toBeVisible();

    await page.getByRole('link', { name: 'Value & reference types', exact: true }).click();
    await expect(page.locator('pre code').first()).toContainText('int original = 10');
    await expect(page.locator('pre code .token').first()).toBeVisible();
    await page.getByRole('button', { name: 'Mark complete', exact: true }).first().click();
    await expect(page.getByRole('button', { name: 'Completed', exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Quick notes', exact: true }).click();
    await expect(page.locator('.code-window')).toHaveCount(0);
    await page.getByRole('button', { name: 'Full lesson', exact: true }).click();
    await expect(page.locator('.code-window')).toHaveCount(2);
    await page.getByText('Can a method mutate an object passed without ref?', { exact: true }).click();
    await expect(page.locator('details[open]')).toContainText('copy of the reference');

    await page.goto('./practice?concept=csharp-types');
    await page.getByRole('button', { name: 'Start 2 questions' }).click();
    await expect(page.getByRole('button', { name: 'Check answer', exact: true })).toBeDisabled();
    await page.getByRole('radio').first().check();
    await page.getByRole('button', { name: 'Check answer', exact: true }).click();
    await expect(page.locator('.answer-feedback.incorrect')).toBeVisible();
    await expect(page.getByRole('radio').first()).toBeDisabled();
    await page.getByRole('button', { name: 'Next question' }).click();
    await page.getByRole('radio', { name: /A reference to the same list|Inline within the class instance/ }).check();
    await page.getByRole('button', { name: 'Check answer', exact: true }).click();
    await expect(page.locator('.answer-feedback.correct')).toBeVisible();
    await page.getByRole('button', { name: 'See results' }).click();
    await expect(page.locator('.result-score')).toHaveText('50%');

    await page.goto('./revision?mode=weak');
    await page.getByRole('button', { name: 'Reveal answer' }).click();
    await expect(page.locator('.flashcard-answer')).toBeVisible();
    await page.getByRole('button', { name: 'Again', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'A good session, all wrapped up.' })).toBeVisible();

    await page.goto('./progress');
    const exported = await downloadProgress(page);
    expect(exported.progress[0]).toMatchObject({
        conceptId: 'csharp-types', isCompleted: true, isBookmarked: true,
        attempts: 2, correctAnswers: 1, reviewStage: 0
    });
    expect(exported.progress[0]).not.toHaveProperty('learnerId');
    expect(exported.quizAttempts).toHaveLength(2);
    expect(exported.history.map(activity => activity.kind)).toContain('Revisit');

    await page.reload();
    await expect(page.getByRole('heading', { name: 'My progress.' })).toBeVisible();
    const persisted = await downloadProgress(page);
    expect(persisted.quizAttempts).toHaveLength(2);
    await expect(page.locator('.weak-concepts')).toContainText('Value & reference types');
});

test('filters, empty states, and mobile navigation remain usable', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('./library');
    await page.getByRole('combobox', { name: 'Learning path', exact: true }).selectOption('Runtime & concurrency');
    await page.getByRole('combobox', { name: 'Difficulty', exact: true }).selectOption('Advanced');
    await expect(page.locator('.concept-card')).toHaveCount(1);
    await expect(page.locator('.concept-card')).toContainText('Concurrency & thread safety');
    await page.getByRole('searchbox', { name: 'Search the library' }).fill('unknown-12345');
    await expect(page.getByRole('heading', { name: 'No matching concepts' })).toBeVisible();
    await page.getByRole('button', { name: 'Clear filters' }).click();
    await expect(page.locator('.concept-card')).toHaveCount(30);
    await page.getByRole('button', { name: 'Bookmarked 0', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'No bookmarks yet' })).toBeVisible();
    await page.getByRole('button', { name: 'Toggle navigation' }).click();
    await expect(page.locator('.sidebar')).toHaveClass(/is-open/);
    await page.getByRole('link', { name: 'Quick revision', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Nothing due right now.' })).toBeVisible();
    await expect(page.locator('.sidebar')).not.toHaveClass(/is-open/);
    await page.getByRole('button', { name: 'All concepts', exact: true }).click();
    await expect(page.getByRole('button', { name: 'Reveal answer' })).toBeVisible();
});

test('browser profiles stay isolated and invalid routes show the not-found page', async ({ browser, page, baseURL }) => {
    await page.goto('./concept/csharp-types');
    await page.getByRole('button', { name: 'Mark complete', exact: true }).first().click();
    await expect(page.getByRole('button', { name: 'Completed', exact: true })).toBeVisible();
    const otherContext = await browser.newContext({ baseURL });
    try {
        const otherPage = await otherContext.newPage();
        await otherPage.goto('./progress');
        const exported = await downloadProgress(otherPage);
        expect(exported.progress).toEqual([]);
        expect(exported.quizAttempts).toEqual([]);
        await otherPage.goto('./concept/not-a-concept');
        await expect(otherPage.getByRole('heading', { name: 'This page is off the path.' })).toBeVisible();
    } finally {
        await otherContext.close();
    }
});