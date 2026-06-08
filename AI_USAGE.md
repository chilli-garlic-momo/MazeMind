# AI Tool Usage Disclosure — MazeMind

Per the *AI in Games* track rules, this document lists every place an AI tool
touched the project.

## Tools used
- **Lovable AI** (primary) — code scaffolding, debugging, design review.
- **ChatGPT** — early brainstorming on the AI Director rule table.
- **GitHub Copilot** — inline autocomplete during Unity script editing.

## Where AI helped

### 1. AI Director design
- Drafted the four personality archetypes (Reckless, Speedrunner, Collector, Balanced).
- Proposed the metric → difficulty-variable mapping table that now lives in
  `AIDirector.Evaluate()`.
- Suggested using a deterministic rule engine over ML for explainability —
  which directly satisfies the rubric's "explainability of AI decisions"
  criterion.

### 2. Code generation (scaffolded, then hand-tuned)
- `AIDirector.cs`, `AdaptationState.cs`, `PlayerMetrics.cs`, `DecisionLogger.cs`
- `CheckboxFloorGenerator.cs` (procedural maze in Room 1.3)
- `Section12Director.cs`, `Section14Director.cs`, `Section15Director.cs`
- `BetweeenRoomManager.cs` (room-transition controller)
- `GhostController.cs` (NavMesh chase in Room 3)
- `DacoitRoom2.cs`, `ExitDoorRoom2.cs`

Every generated file was read, edited, and tested in Unity. No file shipped
unread.

### 3. Debugging sessions
- Room 1 player falling through the floor → traced to Bootstrap single-mode
  scene load cancelling the additive load.
- WinRoom auto-respawning into Room 3 → traced to BetweeenRoomManager firing
  twice on a single trigger collision; added a `_hasLoaded` guard.
- BulletTrap reporting wrong section id → hardcoded `"1.3"` replaced with a
  serialized field.
- `Section15Director` leaking ghost instances on respawn.

### 4. Writing
- AI Decision Log phrasing templates.
- Dacoit dialogue lines.
- This README and AI_USAGE document — drafted by AI, edited by the team.

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
