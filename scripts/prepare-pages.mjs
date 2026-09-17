import { readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

export function normalizeBasePath(value = '/') {
    const candidate = value || '/';
    if (!candidate.startsWith('/') || candidate.startsWith('//') || /[\s<>"'?#\\]/.test(candidate)) {
        throw new Error('The Pages base path must be a path such as / or /dotnet-fieldnotes/.');
    }
    const parsed = new URL(candidate, 'https://fieldnotes.invalid');
    return parsed.pathname.endsWith('/') ? parsed.pathname : `${parsed.pathname}/`;
}

export async function preparePages(directory, basePath) {
    const normalized = normalizeBasePath(basePath);
    for (const filename of ['index.html', '404.html']) {
        const target = path.join(directory, filename);
        const html = await readFile(target, 'utf8');
        const baseTag = /<base href="[^"]*"\s*\/?\s*>/g;
        if ([...html.matchAll(baseTag)].length !== 1) {
            throw new Error(`${filename} must contain exactly one base element.`);
        }
        await writeFile(target, html.replace(baseTag, `<base href="${normalized}" />`));
    }
    await writeFile(path.join(directory, '.nojekyll'), '');
    return normalized;
}

if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) {
    const directory = path.resolve(process.argv[2] || 'artifacts/pages/wwwroot');
    const basePath = await preparePages(directory, process.argv[3] || '/');
    console.log(`Prepared static Pages site at ${directory} with base ${basePath}`);
}