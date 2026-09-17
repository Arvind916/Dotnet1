import { spawnSync } from 'node:child_process';
import path from 'node:path';
import { normalizeBasePath, preparePages } from './prepare-pages.mjs';

const basePath = normalizeBasePath(process.argv[2] || '/dotnet-fieldnotes/');
const result = spawnSync('dotnet', [
    'publish', 'src/Fieldnotes/Fieldnotes.csproj', '--configuration', 'Release',
    '--output', 'artifacts/pages', '--nologo'
], { stdio: 'inherit' });
if (result.error) throw result.error;
if (result.status !== 0) process.exit(result.status || 1);

const directory = path.resolve('artifacts/pages/wwwroot');
await preparePages(directory, basePath);
console.log(`GitHub Pages files: ${directory} (base ${basePath})`);