# Content authoring

Lessons are stored in six arrays under `src/Fieldnotes/Content` and embedded into the WebAssembly client at build time. Rebuild the app after changing content; commits to the default branch rebuild and redeploy the GitHub Pages site once Pages is configured.

## Lesson contract

| Field | Requirement |
| --- | --- |
| `id` | Stable, unique, lowercase route-friendly identifier |
| `title`, `track` | Clear concept name and one of the established learning paths |
| `level` | `Beginner`, `Intermediate`, or `Advanced` |
| `minutes` | Positive estimated reading time, not measured time spent |
| `summary` | One plain-language sentence for the concept card |
| `explanation` | A concise, accurate mental model; define unfamiliar terms |
| `why` | The concrete problem this idea solves |
| `useCase` | A realistic .NET application scenario |
| `whenToUse` | Suitable contexts and meaningful limits or alternatives |
| `pitfalls`, `bestPractices` | Specific mistakes and actionable habits |
| `simpleCode` | A focused minimal example |
| `practicalCode` | A realistic excerpt with the surrounding assumptions made clear |
| `takeaways` | At least three short revision points |
| `interview` | At least two objects with `question` and `answer` |
| `quiz` | At least two objects with unique `id`, `question`, `options`, zero-based `correctIndex`, and `explanation` |
| `related` | At least two valid concept IDs |
| `docsUrl` | An HTTPS link to an authoritative reference |

The model is defined in `src/Fieldnotes/Models/Concept.cs`. Use the existing entries as examples; do not copy lengthy third-party explanations into the catalog. Write original explanations and link to official documentation. The entire curriculum, including correct answers, is public client-side content.

## Review checklist

1. Can someone explain the concept in their own words after the summary and takeaways?
2. Does the lesson explain both the useful case and a situation where the approach is unsuitable?
3. Are runtime, framework, and database-provider assumptions accurate?
4. Is each quiz question unambiguous, with exactly one best answer?
5. Does the feedback explain the distinction instead of merely repeating the answer?
6. Are code excerpts internally consistent, safe, and explicit about missing surrounding types/packages?
7. Do related IDs resolve, and do the official links support the topic?
8. Have both the full lesson and quick-note view been read in the browser?

## Validate a change

```powershell
dotnet test tests/Fieldnotes.Tests --filter FullyQualifiedName~ConceptCatalogTests
```

Startup also rejects an empty catalog, duplicate concept/question IDs, invalid difficulty or answer indices, and broken related links. These checks validate structure, not every educational claim or compilation of every displayed excerpt; content still needs technical review.

Existing browser progress refers to concept and question IDs. Prefer editing titles and prose without changing IDs. If you must remove or rename an ID, plan a corresponding browser-storage migration. The model supports more lessons and questions without a storage-format change.