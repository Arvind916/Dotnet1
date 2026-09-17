async function downloadProgress(page) {
    const waiting = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Export progress', exact: true }).click();
    const download = await waiting;
    const stream = await download.createReadStream();
    const chunks = [];
    for await (const chunk of stream) chunks.push(chunk);
    return JSON.parse(Buffer.concat(chunks).toString('utf8'));
}

module.exports = { downloadProgress };