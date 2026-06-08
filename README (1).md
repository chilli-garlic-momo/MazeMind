# MazeMind

**MazeMind Hackathon Submission — AI in Games Track**
**Theme:** *The maze learns how you play.*
**Engine:** Unity 6 (6000.x), URP, WebGL + Standalone builds

A 3D first-person adaptive maze game where a rule-based **AI Director**
personalises each room based on how the player is playing. Rush through and
the maze stops handing you health. Hoard gems and the gatekeeper demands
more. Die a lot and the trap floors quietly thin out. Every adaptation is
logged on-screen so the AI is never a black box.

---

## Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [Core Loop](#core-loop)
4. [AI Director System](#ai-director-system)
5. [AI Log System](#ai-log-system)
6. [Gameplay Walkthrough](#gameplay-walkthrough)
   - [Room 1 — Tutorial Labyrinth](#room-1--tutorial-labyrinth)
   - [Room 2 — Adaptive Challenge Room](#room-2--adaptive-challenge-room)
   - [Room 3 — Adaptive Memory and Hazard Room](#room-3--adaptive-memory-and-hazard-room)
   - [Win Screen](#win-screen)
7. [Win Condition](#win-condition)
8. [Metrics Tracked](#metrics-tracked)
9. [Difficulty Variables Adjusted](#difficulty-variables-adjusted)
10. [Project Requirements Mapping](#project-requirements-mapping)
11. [Controls](#controls)
12. [Installation Guide](#installation-guide)
13. [Self-Hosting Instructions](#self-hosting-instructions)
14. [Demo Guide for Judges](#demo-guide-for-judges)
15. [Project Structure](#project-structure)
16. [How AI Tools Were Used](#how-ai-tools-were-used)
17. [Stretch Goals Hit](#stretch-goals-hit)
18. [Credits](#credits)

---

## Overview

MazeMind is a first-person, room-based maze game where the challenge evolves
in real time based on how the player performs. A rule-based **AI Director**
monitors behavioural metrics across every room and uses heuristic rules to
make transparent difficulty adjustments — all visible to the player through
a live **AI Log**.

There are no static difficulty presets. Every run is personalised.

## Features
- First-person player controller
- Three-room adaptive maze structure (Room 1 → Room 2 → Room 3 → WinRoom)
- Gem collection with variable requirements
- Real key / dummy key decision system
- Laser hazards with adaptive density
- Dacoit enemy with speed-adjusted patrol behaviour
- Hazardous corridor floor tiles
- Fake gems that penalise careless collection
- Visible AI Director log updated in real time
- Win screen with full performance summary and AI adaptation report

## Core Loop
1. Spawn in a room.
2. Collect gems (optional but the AI is watching).
3. Solve the room's challenge — maze, jump, key, dacoit, lasers.
4. Reach the exit door.
5. The AI Director re-tunes the next room based on what you just did.
6. Repeat through three rooms, then the **WinRoom**.

---

## AI Director System

`Assets/Scripts/Core/AIDirector.cs` is a **rule-based heuristic director** —
no ML, no training data, fully explainable. It exposes a single
`AdaptationState` that every room reads on `Start()`.

Pipeline each room load:
```
PlayerMetrics  ──►  AIDirector.Evaluate()  ──►  AdaptationState  ──►  Room scripts
                            │
                            └──►  DecisionLogger.Log(reason)  ──►  on-screen AI Log
```

Personality classification (heuristic): the director tags the run as one of
**Reckless / Speedrunner / Collector / Balanced** from the metric mix, and
each tag biases the rules.

### Metrics Tracked

| Metric | Purpose |
|---|---|
| Health Remaining | Determines overall player survivability |
| Damage Taken | Measures player struggle level |
| Gems Collected | Measures objective completion efficiency |
| Hazard Collisions | Tracks mistakes made by the player |
| Room Completion Time | Measures player skill and speed |

### Difficulty Variables Controlled

| Variable | Easier Mode | Harder Mode |
|---|---|---|
| Dacoit Speed | Reduced | Increased |
| Laser Difficulty | Reduced | Increased |
| Fake Gem Count | Reduced | Increased |
| Hazard Density | Reduced | Increased |
| Resource Availability | Increased | Reduced |

### Example AI Decisions
- Player completed Room 1 quickly → Increase Dacoit speed in Room 2.
- Player took excessive damage → Reduce Room 3 hazard difficulty.
- Player avoided most hazards → Increase fake gem count.
- Player struggled with navigation → Reduce challenge intensity.

Every decision is recorded in the visible AI Log.

---

## AI Log System

A visible **AI Log** is displayed during gameplay. Its purpose is to explain:
- What the AI changed.
- Why the AI changed it.
- How player performance influenced future rooms.

### Example Log Entries
```
Player performed well. Increasing Dacoit speed.
Health is low. Reducing hazard density.
Fast completion detected. Increasing challenge level.
Multiple collisions detected. Providing easier room configuration.
Player missed 70% of gems — dacoit demand bumped to 14.
```

This transparency lets players and judges directly observe AI behaviour.

---

## Gameplay Walkthrough

### Room 1 — Tutorial Labyrinth
A multi-section opener that teaches every mechanic.
- **1.1 Walk & look** — grounded movement, gem pickup.
- **1.4 Color-floor corridor** — spike rhythm + moving hazard.
- **1.3 Laser / Checkbox maze** — a generated safe path through a trap-tile
  grid; key sits on the path. Key position is AI-driven (early for
  strugglers, late for collectors).
- **1.2 Valley jump** — three platforms; the middle gap *widens mid-jump*
  if the AI thinks you can take it.
- **1.5 Dacoit chamber** — free key on the floor plus a gatekeeper who
  demands gems. The number is **set by the AI Director** based on how
  greedy you were earlier.

### Room 2 — Adaptive Challenge Room

**Spawn Room Information Board**
- Collect all required gems.
- Find the correct key.
- Avoid hazards and enemies.
- Reach the exit door.

**Gem Collection**
Gems are distributed throughout the room and must be collected to satisfy
the room completion requirement. The AI Director can modify:
- Number of gems required.
- Placement difficulty of gems.

Collecting gems contributes to the player's performance evaluation.

**Laser Hazard**
Laser barriers are placed along key pathways.
- Touching a laser causes health damage.
- Laser hits are recorded by the AI Director.
- Excessive laser collisions indicate the player is struggling.

AI Adaptation:
- Strong performance may increase laser density or placement difficulty.
- Weak performance may reduce laser challenge.

**Real Key and Dummy Key System**
- **Real Key** — unlocks the exit door, required to complete the room.
- **Dummy Key** — looks identical to the real key but does not unlock the
  exit; forces exploration and decision-making.

**Exit Door**
The exit door remains locked until (1) required gems are collected and (2)
the real key has been acquired.

**Dacoit Enemy**
The Dacoit is the primary enemy in Room 2.
- Patrols the maze.
- Damages the player on collision.
- Creates pressure during exploration.

The AI monitors collisions, remaining health, and completion efficiency,
then adjusts dacoit speed and future challenge intensity.

**Room 2 Difficulty Manager**
Receives performance data from Room 1 and adapts Room 2 accordingly using
health remaining, damage taken, completion speed, and gems collected.
Possible adaptations: dacoit speed, laser difficulty, gem requirement, key
placement complexity. All decisions are recorded in the AI Log.

### Room 3 — Adaptive Memory and Hazard Room

**Spawn Room Information Board**
- This is the final challenge room.
- Hazards are influenced by previous performance.
- Some collectibles may be deceptive.
- Find the correct key and reach the final exit.

**Spawn Room Map**
A room map is displayed near the spawn location to give an overview of the
layout, help plan a route, and assist navigation through the larger maze.

**Gem Collection**
The AI Director can adjust gem count, distribution difficulty, and risk
associated with obtaining them.

**Long Corridor Tile Hazard**
A long corridor contains hazardous floor tiles.
- Certain tiles trigger damage when stepped on.
- Incorrect movement increases health loss.
- Tile interactions are tracked by the AI Director.

Tests observation, movement precision, and raises tension before the final
objective.

**Laser Hazard**
Additional laser hazards appear in Room 3, with more aggressive placement
than Room 2. Strong players get more difficult positioning; struggling
players get reduced intensity.

**Real Key and Dummy Key System**
Returns in Room 3 — pick the real key to unlock the final exit.

**Fake Gems in Corridor Section**
- Look like real gems.
- Collecting one causes a health penalty.
- Fake gem interactions are logged.

AI Influence: strong players → more fake gems; struggling players → fewer
fake gems. Creates adaptive risk-reward gameplay.

**Dacoit Enemy**
Returns in Room 3 as a persistent threat with potentially increased speed,
more aggressive patrol behaviour, and greater area coverage.

**Room 3 Difficulty Manager**
The final adaptation layer. Evaluates cumulative performance across all
previous rooms (health, total damage, gem efficiency, hazard collisions,
completion speed) and adjusts fake gem count, hazard density, laser
challenge, dacoit difficulty, and resource availability. All decisions are
displayed in the AI Log.

### Win Screen

After completing Room 3 and reaching the final exit, the **Win Screen** is
displayed.

**Congratulations Message** — completion banner confirming the player
escaped MazeMind.

**Player Profile Description** — the AI Director generates a profile based
on observed behaviour across all rooms. Example profiles:
- Careful Explorer
- Risk Taker
- Efficient Runner
- Gem Hunter
- Adaptive Survivor

**Performance Statistics**

| Stat | Description |
|---|---|
| Total Gems Collected | All real gems collected across rooms |
| Health Remaining | Final health at game end |
| Damage Taken | Cumulative damage across all rooms |
| Rooms Completed | Number of rooms successfully cleared |
| Completion Time | Total time from start to finish |

**AI Long Summary** — comprehensive explanation of player performance:
hazard avoidance, gem efficiency, survival, difficulty trends, and why
certain adaptations occurred.

**AI Adaptation Log** — full record of AI decisions made during the run.
Example entries:
```
Increased Dacoit speed due to strong performance.
Reduced hazard density due to low health.
Added fake gems after efficient completion.
Reduced challenge after repeated damage events.
```

This log demonstrates the adaptive nature of the AI Director and gives full
transparency into every major gameplay modification.

---

## Win Condition

The player wins by:
1. Completing Room 1.
2. Completing Room 2.
3. Completing Room 3.
4. Obtaining the correct keys in each room.
5. Reaching the final exit while surviving all hazards.

On completion, the Win Screen shows the Congratulations Message, Player
Profile, Performance Statistics, AI Long Summary, and AI Adaptation Log.

---

## Metrics Tracked
Implemented in `PlayerMetrics.cs`:
1. Health remaining
2. Hits taken / damage taken
3. Deaths / retries
4. Gems collected (absolute + percentage of available)
5. Time per room
6. Trap tiles stepped on / hazard collisions
7. Average jump success rate (Room 1.2)
8. Dacoit payment compliance

## Difficulty Variables Adjusted
Implemented across the room directors:
1. **Dacoit gem demand** (Room 1.5 and Room 2) — base 6–10, ramps up if you ignored gems.
2. **Dacoit movement speed** (Room 2, Room 3) — `hazardSpeedMultiplier`.
3. **Ghost / hazard speed multiplier** (Room 3).
4. **Trap density / spike rhythm rate** (Room 1.4).
5. **Key position on the safe path** (Room 1.3).
6. **Gap 2 widening distance** (Room 1.2).
7. **Laser density and placement** (Room 2, Room 3).
8. **Fake gem count** (Room 3).

At least **two** are always visibly different across runs, satisfying the
rubric.

---

## Project Requirements Mapping

| Challenge Requirement | MazeMind Implementation |
|---|---|
| Player-Controlled Character | First-person player controller |
| Maze or Room-Based Map | Three-room maze structure |
| At Least 3 Rooms | Room 1, Room 2, Room 3 |
| Objective System | Collect gems, obtain key, reach exit |
| Enemy or Hazard | Dacoit, Lasers, Hazard Tiles, Fake Gems |
| AI Director | Rule-based adaptive difficulty system |
| Track Multiple Metrics | Health, Damage, Gems, Completion Time |
| Adjust Difficulty Variables | Enemy speed, hazard density, fake gem count |
| Visible AI Log | Real-time AI adaptation log |
| Win Screen | Final performance summary and AI report |

---

## Controls

| Action | Key |
|---|---|
| Move | WASD |
| Look | Mouse |
| Jump / Double-jump | Space (×2) |
| Interact / Pay dacoit | E |
| Pause | Esc |

---

## Installation Guide

### Requirements
- Unity Hub
- Unity 6 (6000.x) or the version specified in `ProjectSettings`
- Windows 10 / Windows 11 (macOS and Linux also supported)

### Method 1 — Open in Unity
1. Clone the repository:
   ```
   git clone https://github.com/codenamed22/MazeMind.git
   ```
2. Open **Unity Hub** → **Add Project** → select the MazeMind folder.
3. Open the project and wait for package import + script compilation.
4. Open `Assets/Scenes/Boot.unity` (or the Main Menu scene).
5. Press **Play**. Boot auto-loads Room 1.

### Method 2 — Play Build
1. Navigate to `Builds/`.
2. Run `MazeMind.exe`.
3. Start a new game.

---

## Self-Hosting Instructions

Designed so another developer or AI coding assistant can set up the project
without additional guidance.

1. Install **Unity Hub**.
2. Install the Unity version used by the project.
3. Clone the repository.
4. Open the project through Unity Hub.
5. Allow Unity to import all assets.
6. Resolve any package imports automatically through **Package Manager**.
7. Open the starting scene.
8. Press **Play**.

No external servers, databases, APIs, or cloud services are required. The
entire project runs locally inside Unity.

---

## Demo Guide for Judges

A complete demonstration should show:
1. Player movement through the maze.
2. Gem collection.
3. Laser hazard interaction.
4. Dacoit enemy interaction.
5. Key collection system (real vs. dummy key decision).
6. Exit door unlocking.
7. AI Log updates in real time.
8. AI Director difficulty adjustments between rooms.
9. Room 2 adaptive changes.
10. Room 3 adaptive changes.
11. Final Win Screen.
12. AI Performance Summary.
13. AI Adaptation Log.

**Expected demo duration:** approximately 3–5 minutes. See `DEMO_SCRIPT.md`
for a 90-second highlight reel.

---

## Project Structure
```
Assets/
├── Scenes/         Boot, MainMenu, Room1, Room2, Room3, WinRoom
├── Scripts/
│   ├── Boot/       Bootstrap, BootInit
│   ├── Core/       AIDirector, AdaptationState, PlayerMetrics, DecisionLogger, GameManager
│   ├── Gameplay/   PlayerController, PlayerHealth, KeyPickup, GemPickup
│   ├── World/      CheckboxFloorGenerator, Section1{1..5}Director, MovingHazard,
│   │               SpikeTrap, BulletTrap, ForcedFallSequence
│   ├── Rooms/      Room2/BetweeenRoomManager, DacoitRoom2, ExitDoorRoom2,
│   │               Room2DifficultyManager; Room3/GhostController, Room3DifficultyManager,
│   │               FakeGem, HazardTile, LaserHazard
│   └── UI/         AIDecisionLogUI, RunSummaryUI, WinScreenUI
├── Prefabs/        Player, Ghost, Dacoit, Key, Gem, FakeGem, SpikeTrap, MovingHazard, ExitDoor
└── Audio/          ambient, sfx, dacoit, ghost
```

---

## How AI Tools Were Used

Per the submission rules, this is the full disclosure:
- **Design assistance (ChatGPT / Lovable AI):** drafted the AI Director rule
  table, four personality archetypes, and the room-by-room metric → variable
  mapping.
- **Code generation (Lovable AI):** scaffolded `AIDirector.cs`,
  `AdaptationState.cs`, `DecisionLogger.cs`, `CheckboxFloorGenerator.cs`,
  `Section12Director.cs`, `Section14Director.cs`, `Section15Director.cs`,
  and `BetweeenRoomManager.cs`. Hand-tuned afterwards.
- **Debugging:** Lovable AI walked through Room 1's bootstrap fall-through
  bug, the `winRoom → room3` double-load bug, and the bullet-trap section-id
  hardcoding.
- **Placeholder text:** dacoit dialogue lines and AI-log phrasing were AI-drafted.
- **Documentation:** README, AI_USAGE, and demo script outlines were AI-drafted then edited.
- **Audio:** ambient + sfx selected manually; no AI-generated audio.
- **Art:** Unity primitives + URP materials. No AI art.

All gameplay logic, AI behaviour, room design, balancing decisions, and
final implementation were reviewed and integrated by the development team.
Nothing in the gameplay loop relies on a runtime AI call — the "AI Director"
is a deterministic rule engine, by design, for explainability.

---

## Stretch Goals Hit
- Procedural room generation (Room 1.3 maze).
- Multiple hazard types (spikes, moving hazards, bullet trap, lasers, hazard tiles, fake gems, ghost/dacoit).
- Real key / dummy key decision system across Room 2 and Room 3.
- Final player performance summary on Win Screen.
- Difficulty personality modes (Reckless / Speedrunner / Collector / Balanced + Careful Explorer / Risk Taker / Efficient Runner / Gem Hunter / Adaptive Survivor on Win Screen).
- Adaptive enemy behaviour (dacoit and ghost speed scale with player metrics).

---

## Credits
- **Team:** chilli-garlic-momo & codenamed22
- **Engine:** Unity 6 (6000.x), URP
- **Built for:** the *AI in Games* track.

*MazeMind — Hackathon Submission · AI in Games Track*
