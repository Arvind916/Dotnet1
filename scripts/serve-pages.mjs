import { createServer } from 'node:http';
import { createReadStream } from 'node:fs';
import { stat } from 'node:fs/promises';
import path from 'node:path';
import { normalizeBasePath } from './prepare-pages.mjs';

const root = path.resolve(process.argv[2] || 'artifacts/pages/wwwroot');
const port = Number(process.argv[3] || 5270);
const basePath = normalizeBasePath(process.argv[4] || '/');
const types = {
    '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8',
    '.json': 'application/json', '.css': 'text/css; charset=utf-8', '.wasm': 'application/wasm',
    '.png': 'image/png', '.svg': 'image/svg+xml', '.woff2': 'font/woff2', '.ico': 'image/x-icon'
};

const server = createServer(async (request, response) => {
    try {
        const url = new URL(request.url, 'http://localhost');
        if (url.pathname === basePath.slice(0, -1)) {
            response.writeHead(302, { Location: basePath + url.search });
            response.end();
            return;
        }
        if (!['GET', 'HEAD'].includes(request.method) || !url.pathname.startsWith(basePath)) {
            response.writeHead(404);
            response.end('Not found');
            return;
        }
        const relative = decodeURIComponent(url.pathname.slice(basePath.length));
        let target = path.resolve(root, relative || 'index.html');
        if (!target.startsWith(root + path.sep) && target !== root) {
            response.writeHead(404);
            response.end('Not found');
            return;
        }
        const entry = await stat(target).catch(() => null);
        if (entry?.isDirectory()) target = path.join(target, 'index.html');
        const exists = await stat(target).then(item => item.isFile()).catch(() => false);
        if (!exists) target = path.join(root, '404.html');
        response.writeHead(exists ? 200 : 404, {
            'Content-Type': types[path.extname(target)] || 'application/octet-stream',
            'Cache-Control': 'no-store',
            'X-Content-Type-Options': 'nosniff'
        });
        if (request.method === 'HEAD') response.end();
        else createReadStream(target).on('error', () => response.destroy()).pipe(response);
    } catch {
        response.writeHead(400);
        response.end('Bad request');
    }
});

server.listen(port, '127.0.0.1', () => console.log(`Static Pages preview: http://127.0.0.1:${port}${basePath}`));