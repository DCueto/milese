---
name: worktrees
description: >-
  How to work in a git worktree in this repository, and how to run several Milese API instances at
  once: what a worktree does and does not share (one git history, but uncommitted edits live in ONE
  folder — a worktree starts from a commit), the permanent per-worktree home branch
  `<initials>/wt/<slug>` that the folder is parked on between pieces of work (because a worktree can
  never sit on `main`), branching off `main`, placing the folder as a sibling rather than nested, the
  one shared Postgres container on host port 15432, and the AppHost's automatic instance resolution —
  it detects whether it is the main checkout or a linked worktree, takes its own port slot and
  database. `dotnet run` needs no flags and no per-worktree setup. Read and apply this BEFORE you
  create or remove a worktree, pick the branch a worktree should be on, prune a branch whose PR has
  merged, run the app while another copy may already be running, or debug a port collision — and
  whenever you wonder "why aren't my changes in the worktree?", "why is this branch hitting the other
  branch's data?", "which folder am I actually in?", "which branch do I leave this worktree on?", or
  hit "address already in use" or "branch is already checked out". Trigger on any worktree,
  `git worktree add`, parallel-checkout, two-branches-at-once, or second-instance work, not only when
  "worktree" is named explicitly.
---

# Worktrees in Milese

A worktree is a second folder on disk holding the same repository on a different branch. It is not a
clone, not a container, and not a second database.

## What is shared, and what is not

Most wasted time here comes from misreading this table.

| | Shared between folders? |
|---|---|
| Git history, branches, commits | **shared** — one repository |
| Your **uncommitted** edits | **not shared** — they exist in one folder only |
| `bin/` and `obj/` | separate — each folder builds itself |
| Postgres container `milese-postgres`, host port **15432** | **shared**, and it survives Aspire stopping (`WithPersistentLifetime()`) |
| The database inside that container | **not shared** — each folder gets its own (`milesedb`, `milesedb_<worktree>`) |
| App ports | **not shared** — each folder takes its own slot automatically |
| `postgres-password` user secret | **shared** — `UserSecretsId` is committed, so a new folder needs no setup |
| `CLAUDE.md`, `.claude/skills/` | **shared** — ordinary committed files (no `.agents/`/symlink layer to reason about here — see the repo's `CLAUDE.md`), so any worktree gets them straight from `git checkout` |
| `dotnet test` | needs **no container and no database** — integration tests default to the SQLite provider (see `testing`) |

**The single most common mistake: a worktree starts from a commit, not from your working files.** If
work is uncommitted in the original folder, it does not exist in the new one. That is not a bug, and no
amount of rebuilding will fix it — commit, or copy the files across deliberately.

## Every worktree owns a home branch

Git refuses to check out one branch in two folders, so **a worktree can never sit on `main`** — the
main checkout has it. A worktree therefore always holds *some* branch, and the question is which one it
holds between pieces of work. Left on the `feat/…` branch of a merged PR, that branch cannot be deleted,
its upstream is gone, and the folder is parked commits behind `main` — every build there is answering a
question nobody asked.

So each worktree gets one permanent **home branch**, named:

```
<initials>/wt/<worktree-slug>        e.g. dc/wt/tutor, dc/wt/mobile-nav
```

It is a parking spot, not a line of work:

- **Created once**, with the worktree, and never deleted while the worktree exists.
- **Never pushed, never a PR, never merged.** It is a local bookmark; the remote never hears about it.
- **Nothing is ever committed on it.** Work happens on a `feat/…` · `fix/…` · `chore/…` · `docs/…` ·
  `refactor/…` branch cut off `main`, exactly as anywhere else (see the repo's `CLAUDE.md`).
- The initials keep it obvious whose folder it is in a shared remote's branch list, if one ever leaks.

The full cycle for one piece of work in a worktree:

```bash
cd ../milese-wt/tutor
git fetch origin
git checkout -b feat/<slug> origin/main    # work branches off main, not off the home branch
# …work, commit, PR into main, merge…
git checkout dc/wt/tutor                   # park it back home
git branch -d feat/<slug>                  # now deletable — nothing is holding it
```

Park the folder **before** deleting the work branch, not after: `git branch -d` refuses to delete the
branch you are standing on, and once you have moved off it, `-d`'s merged-check runs against the home
branch — which never contains anything — so it reports "not fully merged" even for merged work. Confirm
the merge with `git branch -r --contains <sha>` (expect `origin/main`) and then `-D` is safe and honest.

## Creating one

The folder is created on its home branch, cut off **`main`**.

Put the folder **beside** the repository, never inside it. A nested copy is a second full checkout of
the tree, so anything that scans recursively from the root finds duplicate projects.

```bash
MAIN=~/Projects/milese
cd $MAIN
git worktree add ../milese-wt/<slug> -b <initials>/wt/<slug> main
cd ../milese-wt/<slug>

cd apps/api
dotnet build Milese.slnx     # confirm the folder is healthy before changing anything
dotnet test --solution Milese.slnx   # no container or database needed
```

The worktree is now sitting on its home branch with nothing in flight. Cut the work branch off `main`
when you actually have work, per the cycle above.

Removing it:

```bash
cd $MAIN
git worktree remove ../milese-wt/<slug>    # --force if build leftovers block it
git worktree prune                         # only needed if the folder was deleted by hand
git branch -D <initials>/wt/<slug>         # the home branch outlives the folder unless you say so
```

`git worktree list` is the source of truth for what exists and on which branch.

## Claude Code's built-in worktrees

`EnterWorktree`, `claude --worktree`, and agent isolation create the folder at
`<repo>/.claude/worktrees/<name>`, nested inside the checkout. Their base ref comes from the
`worktree.baseRef` setting, whose default `fresh` means `origin/<default branch>` — that already is
`origin/main` here, so the default is correct for this repo, unlike a project whose base branch isn't
the default. A nested worktree is still a full second copy of the tree (`Milese.slnx` and every
`.csproj` included), so anything scanning recursively from the repo root sees duplicates.

## Running several instances

There is nothing to configure and no profile to choose. In any folder:

```bash
cd apps/api/src/Aspire/Aspire.AppHost && dotnet run     # or dotnet watch
```

On startup the AppHost resolves its own identity and prints it:

```
Milese instance: checkout tutor, slot 1, database milesedb_tutor, api http://localhost:5180
```

**How the slot is chosen.** `CheckoutLocator` walks up for `.git`. A **directory** means the main
checkout — slot 0, the familiar port (API `5080`) and the `milesedb` database. A **file** means a
linked worktree, and its name comes from the `gitdir:` line inside; its slot is its alphabetical
position among the worktrees registered in `.git/worktrees/`, plus one (`InstanceSlotResolver`). Each
slot shifts the API port by 100 (`InstancePorts`), so slot 1 is API `5180`. If a slot's port is already
held, the next free slot is taken instead (`PortAvailability`); the database name is tied to the
folder, not the slot, so it does not move when that happens.

**The dashboard has no fixed port.** Aspire assigns the dashboard and its internal plumbing free ports
on every run, so they can never collide — follow the link the terminal prints rather than bookmarking
one.

**Overrides**, if you ever need to pin something. All three can also be set as environment variables
using `__` in place of `:`, and any of them wins over the automatic value.

| Key | Automatic value | Effect |
|---|---|---|
| `Milese:PostgresPort` | `15432` | Host port of the shared Postgres container |
| `Milese:DatabaseName` | `milesedb` / `milesedb_<worktree>` | Which database this instance uses |
| `Milese:PortOffset` | slot × 100 | Added to the API's port |

Overriding `Milese:PortOffset` alone is safe — nothing else derives from it that could drift.

### Why 15432 and not 5432

`WithEndpointProxySupport(false)` makes Docker publish `15432` directly instead of Aspire's DCP proxy
forwarding to a random container port. **A GUI client (TablePlus, DBeaver, `psql`) stays connected when
Aspire stops** — the container itself has `WithPersistentLifetime()`, so it survives an AppHost restart
entirely. `5432` is avoided so a locally-installed Postgres (if any) never collides with it.

Consequence: `Server=localhost` alone means the *default* Postgres, not this project's. Always
`Host=localhost;Port=15432`.

## No frontend address-file step (yet)

Milese doesn't have a Blazor-WASM-style UI that needs a runtime-rewritten address file to find its
resolved API port — `apps/web`/`apps/mobile` aren't scaffolded yet. When they exist, each will need its
own mechanism to learn the current instance's API port (an env var for Next.js at dev-server start, a
build-time config value for Expo) — there is nothing to configure for that today.

## Rules that keep agent work correct

- **Subagents may run against the main checkout, not the worktree.** They can miss the worktree's
  uncommitted changes and report success against the wrong tree. Re-run the build or test yourself in
  the worktree, or commit first, before treating a result as verified.
- **Never blanket-stage with `git add -A`.** Several sessions may be editing one tree, so the working
  tree can hold changes that are not yours. Stage explicit paths (already the rule in this repo's
  `CLAUDE.md`, doubly true with more than one folder in play).
- **Leave a worktree on its home branch, not on work you finished.** When a PR merges, park the folder
  on `<initials>/wt/<slug>` before deleting the work branch.
- **Establish where you are before claiming anything is verified**, because almost every confusing
  result is really a wrong-folder result:

  ```bash
  git branch --show-current
  git worktree list
  ```

## When something goes wrong

| Symptom | Cause | Check |
|---|---|---|
| `address already in use` on the API port | the same folder is already running — two folders cannot collide | `pgrep -fl Aspire.AppHost` · `lsof -nP -iTCP:5080 -sTCP:LISTEN` |
| Two folders landed on the same slot | the worktree was created after the AppHost read `.git/worktrees/` | restart the instance; slots are read at startup |
| My changes are missing in the worktree | they were uncommitted when it was created | `git status` in the original folder |
| `fatal: branch is already checked out` | git refuses one branch in two folders — it is protecting you. You cannot put a worktree on `main`; that is what the home branch is for | `git worktree list` |
| `error: the branch … is not fully merged` after a merged PR | `-d` checks against HEAD, and you are standing on the home branch, which contains nothing | `git branch -r --contains <sha>` — if it names `origin/main`, `-D` is safe |
| A worktree is several commits behind and its upstream is gone | it was left on the `feat/…` branch of a merged PR instead of parked on its home branch | `git worktree list` · `git checkout <initials>/wt/<slug>` |
| A worktree starts with no data | it has its own database — this is by design | the `database` field in the `Milese instance:` line |
| `Host=localhost` with no port fails to connect | bare `localhost` means the default Postgres port (5432) | use `Host=localhost;Port=15432` |
| Can't connect to the database at all | container stopped | `docker ps` — expect `milese-postgres` on `15432` |

---

## See also

- **ef-core** — migrations, and why divergent migrations across branches need separate databases
- **testing** — `dotnet test` needs neither the container nor a running instance (SQLite by default)
