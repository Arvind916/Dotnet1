import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, writeFile, rm, access } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import vm from 'node:vm';
import { normalizeBasePath, preparePages } from '../../scripts/prepare-pages.mjs';

test('base paths support both project Pages and account/custom-domain roots', () => {
    assert.equal(normalizeBasePath(''), '/');
    assert.equal(normalizeBasePath('/'), '/');
    assert.equal(normalizeBasePath('/dotnet-fieldnotes'), '/dotnet-fieldnotes/');
    for (const invalid of ['https://other.test/', '//other.test/', '/app?query', '/app"', '/app\\path']) {
        assert.throws(() => normalizeBasePath(invalid));
    }
});

test('preparation sets both entry bases and retains the no-Jekyll marker', async () => {
    const directory = await mkdtemp(path.join(tmpdir(), 'fieldnotes-pages-'));
    try {
        for (const filename of ['index.html', '404.html']) {
            const source = await readFile(new URL(`../../src/Fieldnotes/wwwroot/${filename}`, import.meta.url));
            await writeFile(path.join(directory, filename), source);
        }
        for (const base of ['/dotnet-fieldnotes/', '/']) {
            await preparePages(directory, base);
            for (const filename of ['index.html', '404.html']) {
                assert.match(await readFile(path.join(directory, filename), 'utf8'),
                    new RegExp(`<base href="${base}" />`));
            }
        }
        await access(path.join(directory, '.nojekyll'));
    } finally {
        await rm(directory, { recursive: true, force: true });
    }
});

test('404 redirect round trips deep links, query strings, and fragments', async () => {
    const redirect = await readFile(new URL('../../src/Fieldnotes/wwwroot/js/redirect.js', import.meta.url), 'utf8');
    const restore = await readFile(new URL('../../src/Fieldnotes/wwwroot/js/pages.js', import.meta.url), 'utf8');
    for (const base of ['/', '/dotnet-fieldnotes/']) {
        const baseURI = `https://example.github.io${base}`;
        const route = `${base}concept/linq?quick=true&q=C%23%20%26%20LINQ#interview`;
        let redirected;
        let restored;
        vm.runInNewContext(redirect, {
            URL, document: { baseURI },
            window: { location: { href: new URL(route, baseURI).href, replace: value => { redirected = value; } } }
        });
        assert.equal(new URL(redirected).pathname, base);
        vm.runInNewContext(restore, {
            URL, document: { baseURI },
            window: { location: { href: redirected }, history: { replaceState: (_, title, value) => { restored = value; } } }
        });
        assert.equal(restored, route);
    }
});

test('route restoration cannot leave the app origin or repository path', async () => {
    const script = await readFile(new URL('../../src/Fieldnotes/wwwroot/js/pages.js', import.meta.url), 'utf8');
    const baseURI = 'https://example.github.io/dotnet-fieldnotes/';
    for (const target of ['https://other.test/', '//other.test/', '/another-repo/']) {
        let restored;
        const url = new URL(baseURI);
        url.searchParams.set('__fieldnotes_route', target);
        vm.runInNewContext(script, {
            URL, document: { baseURI },
            window: { location: { href: url.href }, history: { replaceState: (_, title, value) => { restored = value; } } }
        });
        assert.equal(restored, '/dotnet-fieldnotes/');
    }
});