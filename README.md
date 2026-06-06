# MazeMind

## Track

AI in Games

## Problem Statement

Build a simple maze game where an **AI Director** adjusts the difficulty based on how the player is performing.

The player moves through rooms, collects gems, avoids monsters or hazards, and tries to reach the exit. The AI Director observes player behavior and modifies later rooms to make the game easier or harder.

The AI does not need to use machine learning. A clear rule-based or heuristic system is acceptable. The important part is that the AI decisions should visibly affect the game.

## Theme

The maze learns how you play.

## Core Requirements

Your project must include:

1. A player-controlled character.
2. A maze, grid, or room-based map.
3. At least **3 rooms or stages**.
4. At least one objective, such as:
   - Collect gems
   - Reach the exit
   - Survive enemies
   - Collect a key
5. At least one enemy or hazard.
6. An AI Director that tracks at least **2 player metrics**, such as:
   - Health remaining
   - Time taken
   - Number of hits taken
   - Number of retries
   - Gems collected
   - Enemy collisions
7. The AI Director must adjust at least **2 difficulty variables**, such as:
   - Enemy speed
   - Number of enemies
   - Number of health pickups
   - Trap count
   - Time limit
   - Number of gems required
8. A visible AI log showing what the AI changed and why.

## Minimum Viable Product

A successful MVP should have:

- A playable maze game.
- Three rooms or stages.
- One enemy or hazard type.
- AI-based difficulty adjustment.
- A visible AI decision log.
- Win or completion screen.

Simple graphics are acceptable.

## Example AI Decisions

- Player took too much damage. Adding one health pickup in the next room.
- Player cleared the room quickly. Increasing monster speed.
- Player missed many gems. Reducing the required gem count.
- Player is doing well. Adding one extra enemy.

## Suggested Tech Stack

You may use any stack, including:

- Pygame
- Godot
- Unity
- Phaser
- HTML Canvas
- JavaScript
- Python

## AI Approach

You may use:

- Rule-based logic
- Decision trees
- Simple scoring systems
- Heuristics
- Procedural generation
- AI-assisted content generation

A rule-based system is fully acceptable.

## Stretch Goals

- Procedural room generation
- Multiple enemy types
- AI-generated room descriptions
- Boss room
- Final player performance summary
- Difficulty personality modes:
  - Friendly
  - Balanced
  - Aggressive
- Adaptive enemy behavior

## AI Tool Usage

You may use AI tools for:

- Designing the AI rules
- Generating level ideas
- Debugging logic
- Writing code
- Creating placeholder text or room descriptions

You must mention in your final README how AI tools were used.

## Demo Expectations

Your final demo should show:

1. Player moving through the maze.
2. Player performance being tracked.
3. AI Director making at least two visible decisions.
4. Game difficulty changing based on performance.
5. Win or completion screen.

## Judging Focus

You will be judged on:

- Meaningful AI behavior
- Gameplay clarity
- Completeness
- Explainability of AI decisions
- Fun factor
- Demo quality