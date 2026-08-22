---
name: naming-conventions
description: Naming a new project, class, or file in apps/api and unsure which suffix/casing/location applies. Trigger whenever creating a new C# file, not only when the user asks about naming explicitly.
---

# Naming conventions

| Thing | Pattern | Example |
|---|---|---|
| Value Type | `<Name>` (no suffix) | `LessonId`, `EstimatedMinutes` |
| Domain entity | `<Name>Bo` | `LessonBo`, `LessonCompletionBo` |
| EF Core entity | `<Name>Db` | `LessonDb` |
| Mapper | `<Name>Mappers` (static class) | `LessonMappers` |
| Read data access | `<Name>sReadDataAccess` | `LessonsReadDataAccess` |
| Update data access | `<Name>sUpdateDataAccess` | `LessonsUpdateDataAccess` |
| Service | `<Name>sService` (or `<Name>sReadService`/`<Name>sUpdateService` if the read/write split matters for that entity) | `LessonsService`, `TutorConversationsUpdateService` |
| Test project | `<SourceProject>.Tests`, flat under `apps/api/tests/` (no group folder, unlike `src/`) | `Common.Types.Tests`, `Services.Core.Tests` |
| Project group folder (`src/<Group>/...`) | Plural noun for the layer, singular for nothing | `Common`, `Data`, `Services`, `Api` |

## Rules

- Namespaces mirror the physical folder path: `Milese.Api.<Group>.<Project>.<Path>`.
- File-scoped namespaces always (`namespace X;`, not `namespace X { }`) — enforced via `.editorconfig` as a warning.
- No `I<Name>Service`/`I<Name>DataAccess` interface unless there's a second real implementation (a test double that isn't just "the same class against an in-memory DB," a cached variant, etc.) — see `data-layer`. Don't add one pre-emptively.
- English identifiers everywhere, regardless of Content Language (see `CLAUDE.md` → Always-on baseline).
