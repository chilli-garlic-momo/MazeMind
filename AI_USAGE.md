# AI Tool Usage Disclosure — MazeMind

Per the *AI in Games* track rules, this document lists every place an AI tool
touched the project.

## Tools used
- **Claude (Anthropic)** — drafting the high-level plan: what to build, room
  structure, AI Director archetypes, and the overall design approach.
- **Arena.ai (Claude "Thinking" mode)** — primary coding assistant for Unity
  C# scripts: scaffolding gameplay systems, debugging runtime issues, and
  iterating on the AI Director rule engine.
- **Lovable** — tracking PR changes, reviewing/understanding code pushed by
  teammates, and drafting pull request descriptions and documentation.

## Where AI helped

### 1. Planning (Claude)
- Drafted the four AI Director personality archetypes (Reckless, Speedrunner,
  Collector, Balanced).
- Proposed the metric → difficulty-variable mapping table that now lives in
  `AIDirector.Evaluate()`.
- Suggested a deterministic rule engine over ML for explainability — which
  directly satisfies the rubric's "explainability of AI decisions" criterion.
- Outlined the room progression and win-condition flow.

### 2. Code generation (Arena.ai — Claude Thinking)
Scaffolded, then hand-tuned and tested in Unity:
- `AIDirector.cs`, `AdaptationState.cs`, `PlayerMetrics.cs`, `DecisionLogger.cs`
- `CheckboxFloorGenerator.cs` (procedural maze in Room 1.3)
- `Section12Director.cs`, `Section14Director.cs`, `Section15Director.cs`
- `BetweeenRoomManager.cs` (room-transition controller)
- `GhostController.cs` (NavMesh chase in Room 3)
- `DacoitRoom2.cs`, `ExitDoorRoom2.cs`

Every generated file was read, edited, and tested in Unity. No file shipped
unread.

### 3. Debugging sessions (Arena.ai — Claude Thinking)
- Room 1 player falling through the floor → traced to Bootstrap single-mode
  scene load cancelling the additive load.
- WinRoom auto-respawning into Room 3 → traced to BetweeenRoomManager firing
  twice on a single trigger collision; added a `_hasLoaded` guard.
- BulletTrap reporting wrong section id → hardcoded `"1.3"` replaced with a
  serialized field.
- `Section15Director` leaking ghost instances on respawn.
- Section 1.3 trap tiles not killing the player → added tall world-space
  `_TrapKillVolume` over each trap tile.
- Section 1.5 dacoit/door gating allowing bypass and incorrect respawns →
  rewrote `DacoitRoom2.TryResolveForPlayer` + `ExitDoorRoom2.TryOpen` flow.

### 4. PR & code review workflow (Lovable)
- Tracking diffs and changes across pull requests.
- Reading and explaining code pushed by teammates.
- Drafting PR descriptions, commit messages, and this AI usage document.
- Producing architecture / HLD diagrams of the gameplay systems.

### 5. Writing
- AI Decision Log phrasing templates (Claude).
- Dacoit dialogue lines (Claude).
- README and PR documentation — drafted by Lovable, edited by the team.

## Where AI was **not** used
- Audio (selected and mixed manually).
- Art (Unity primitives + URP materials).
- Final design decisions on room order, win condition, and difficulty curve —
  all human calls.
- No runtime AI/LLM calls during gameplay. The "AI Director" is a pure rule
  engine; the game runs fully offline.

## Reproducibility
Because the director is deterministic, given the same `PlayerMetrics` it
produces the same `AdaptationState`. Judges can verify any logged decision
by replaying with the same playstyle.
