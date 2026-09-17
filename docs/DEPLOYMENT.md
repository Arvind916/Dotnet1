# GitHub Pages deployment

## What is hosted

Fieldnotes is a **standalone Blazor WebAssembly application**. GitHub Pages serves its static HTML, CSS, JavaScript, images, and .NET WebAssembly files. There is no server process or hosted database. Study history stays in the visitor's IndexedDB storage.

## Publish entirely through the GitHub website

1. Create a repository at https://github.com/new. `dotnet-fieldnotes` is a suitable name. Choose a public repository for GitHub Free Pages support, or check your plan's private-repository Pages availability. Leave the initialization options unchecked when uploading this existing project.
2. Select **uploading an existing file**, or **Add file > Upload files**. Upload the source while preserving directories: `src`, `tests`, `scripts`, `docs`, `.github`, and the root project/configuration files. Include `global.json`, `NuGet.Config`, the solution, `package.json`, and `package-lock.json`. The Docker files are optional for Pages.
3. Commit the uploaded files to the repository's default branch, usually `main`. If multiple upload batches are needed, finish all batches before troubleshooting an incomplete-source build.
4. Open **Settings > Pages** and select **GitHub Actions** as the build/deployment source. Do not choose branch-based publishing of the raw C# source.
5. Open **Actions > Build and Deploy Pages > Run workflow**, choose the default branch, then click **Run workflow**. This also retries initial runs that failed before Pages was enabled.
6. Wait for both `verify` and `deploy` to succeed. Follow the URL shown in **Settings > Pages**, or the `github-pages` deployment environment.

Your URL is normally `https://YOUR-USERNAME.github.io/YOUR-REPOSITORY/`. Later commits to the default branch redeploy it automatically. Pull requests and other branches are tested without publishing a site. No separate hosting account or personal access token is required by this workflow.

The website uploader does **not** honor `.gitignore`. Upload a clean copy without `bin`, `obj`, `App_Data`, `node_modules`, `artifacts`, generated reports, local databases, or secrets. GitHub limits each browser batch to 100 files and each file to 25 MB. Upload smaller groups into the matching repository directories rather than flattening them. A ZIP of source is not a runnable Pages deployment.

## What the workflow does

The workflow in `.github/workflows/ci.yml`:

1. Installs the .NET 10 SDK and Node.js, then restores dependencies.
2. Runs C# rules/content/storage tests and the static-routing tests.
3. Reads the Pages configuration to obtain the actual base path.
4. Publishes the app and prepares `index.html`, `404.html`, and `.nojekyll`.
5. Tests the published files using Chromium and a static-only preview server.
6. Uploads only `artifacts/pages/wwwroot` and deploys it using GitHub's official Pages actions.

Deployment permissions are limited to the deploy job (`pages: write` and `id-token: write`). Build verification has read-only repository and Pages access. Browser reports and screenshots are retained as diagnostic artifacts. The workflow does not commit generated files into the source branch.

## Repository paths and custom domains

The same app supports project sites such as `/dotnet-fieldnotes/`, account/organization root sites, and custom-domain roots. `actions/configure-pages` supplies the correct base path, so no source-level hardcoding of your username or repository name is needed.

Configure a custom domain through GitHub Pages settings and DNS according to GitHub's documentation, then rerun the workflow. Changing a domain or base path changes where the browser looks for progress; it does not transfer old data.

Direct lesson links and refreshes work through the supplied static 404 redirect. The redirect retains the route, query string, and fragment, restores them before Blazor starts, and refuses targets outside the current site/base path. Unknown concepts show the app's not-found page rather than requiring a backend.

## Local static preview

From the repository root, with .NET 10 and Node.js 20 or later:

```powershell
npm ci
npm run build:pages
npm run preview:pages
```

Open http://127.0.0.1:5270/dotnet-fieldnotes/. Use another port in the preview command if 5270 is occupied.

For a different repository name:

```powershell
npm run build:pages -- /my-repository/
node scripts/serve-pages.mjs artifacts/pages/wwwroot 5271 /my-repository/
```

For a root/custom-domain site:

```powershell
npm run build:pages -- /
node scripts/serve-pages.mjs artifacts/pages/wwwroot 5271 /
```

Only the contents of `artifacts/pages/wwwroot` are deployed. The preview server maps the chosen base path to that folder and returns real 404 responses for missing paths; it does not implement an API or SPA fallback. The preview script is development tooling, not a production web server. Opening `index.html` with `file://` will not run WebAssembly correctly.

Browser tests default to port 5270 and `/dotnet-fieldnotes/`. To test a different prepared base/port, set `FIELDNOTES_BASE_PATH` and `FIELDNOTES_PREVIEW_PORT` to match before `npm run test:e2e`.

## Progress and privacy

- Progress, quiz attempts, bookmarks, and history are stored in IndexedDB on the visitor's device. The host receives ordinary asset requests, but the app does not upload study progress.
- A database namespace includes the repository base path to avoid accidental collisions. Other JavaScript on the same origin can still access browser storage, so this is not account security.
- Clearing browser/site data, storage eviction, private browsing, switching browsers, or changing the site's origin/base path can make progress unavailable. Accounts, recovery, cloud sync, and import are not implemented.
- **Export progress** downloads a local JSON record for inspection. It is not a restorable backup through the current UI.
- Previous server-version SQLite data and keys in `src/Fieldnotes/App_Data` remain untouched and ignored. The static app does not read, publish, or migrate them.
- Every quiz answer and grading rule is in the client bundle. Do not use these scores for secure assessment.
- Fonts are requested from Google Fonts with local fallbacks. Icons and syntax highlighting are local. There is no offline-installable PWA/service worker in this version.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Workflow cannot find Pages configuration | Enable **Settings > Pages > GitHub Actions**, then run the workflow again |
| Workflow is missing | Preserve `.github/workflows/ci.yml`; ensure it exists on the default branch |
| Build cannot find project files | Preserve the uploaded directory structure and finish all upload batches |
| Deployment permission is denied | Review repository/organization Actions and Pages policies and the `github-pages` environment's branch rules |
| Blank page or missing `_framework` files | Deploy the prepared `wwwroot` output, not raw source; retain `.nojekyll` and the correct base path |
| Refresh on a lesson fails | Ensure the generated `404.html`, redirect scripts, and `index.html` use the same base path |
| Progress seems missing | Check browser, origin, and repository path; allow IndexedDB and inspect site-data settings |

## Optional container hosting

The Docker setup now serves static files with an unprivileged Nginx image; it does not run ASP.NET Core or use a data volume. `docker compose up --build` maps port 5268 to container port 8080. This is an alternative to Pages, not a Pages prerequisite. Docker was not available for local container verification; the static Pages output and browser workflows were verified separately.