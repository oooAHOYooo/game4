# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project: Skate 4 Clone

**Vision:** A skateboarding game emphasizing trick mechanics, physics-based movement, and open-world exploration inspired by EA's Skate series.

**Core Pillars:**
1. **Physics-based skateboard mechanics** — Dynamic balance, momentum, realistic gravity; not arcade-y floaty physics
2. **Trick system** — Flip tricks, grind tricks, manual tricks, combo chains with risk/reward
3. **Open world exploration** — Freeroam level design, environmental interaction, spots to discover

**Target Player:** Skate series fans aged 13+

## Tech Stack
- **Engine:** Unity (latest stable LTS)
- **Language:** C#
- **Physics:** Built-in Rigidbody/Collider (may migrate to custom for precision)
- **Input:** Unity's new Input System

## Architecture Overview

### Core Systems (to implement)

**1. Skateboard Physics Engine** (`Assets/Scripts/Physics/`)
- `SkateboardController.cs` — Core skateboard movement: velocity, rotation, balance state
- `SkateboardPhysics.cs` — Physics calculations: gravity, drag, friction, lean angle constraints
- `BalanceSystem.cs` — Player's ability to maintain balance; wobble/fall when balance breaks
- Separate logic for **grounded** (on board) vs **airborne** states

**2. Input & Control** (`Assets/Scripts/Input/`)
- `SkaterInput.cs` — Raw input mapping (keys/gamepad → stick flicks, buttons for tricks)
- Input should drive physics, not override them (stick position → lean angle target, not direct rotation)

**3. Trick System** (`Assets/Scripts/Tricks/`)
- `TrickLibrary.cs` — Catalog of all tricks (flip tricks, grind tricks, manuals) with metadata
- `TrickDetector.cs` — Detects player intent (stick flicks, button holds) → which trick is being attempted
- `TrickExecutor.cs` — Plays animations, applies physics modifications (trajectory, spin), validates landing
- **Combo system** — Track consecutive landed tricks, reset on bail, award points/style

**4. Player Controller** (`Assets/Scripts/Player/`)
- `SkaterController.cs` — High-level player state machine: grounded → airborne → landing/bailing
- Coordinates physics, input, animations, VFX

**5. Animation** (`Assets/Scripts/Animation/`)
- Animation state machine synced to SkaterController state
- Idle stance, riding, trick animations, landing, falling
- Consider Mixamo or hand-animated clips

**6. World/Level** (`Assets/Scripts/World/`)
- `LevelManager.cs` — Spot data, collision layers for grinds/manuals
- `SpotMarker.cs` — Challenge locations (gaps, rails, stairs)
- Physics layers: **Skateable** (normal ground), **Grindable** (rails/ledges), **ManualSurface** (curbs)

**7. Camera** (`Assets/Scripts/Camera/`)
- Third-person follow cam tracking skater + momentum
- Smooth look-ahead in direction of travel
- Tighten during tricks, pull back on manual

**8. Feedback** (`Assets/Scripts/Feedback/`)
- `SoundManager.cs` — Trick sounds, impact noises, landing thuds, grind grinds
- `VFXManager.cs` — Spark trails, dust clouds, landing impacts
- `CameraShake.cs` — Bumps on impact/landing

### Key Files (Architecture)
- **Skateboard vs Skater distinction:** The skateboard (`Rigidbody`) is the physics body; skater is the controller that inputs to it
- **Physics runs in FixedUpdate**, input in Update
- **Trick validation happens on landing** — check velocity, rotation, ground contact to confirm trick success
- **Bail system** — Loss of balance → ragdoll/animation → respawn nearby or manual recovery

## Development Workflow

### Setup (First Time)
```bash
# Open project in Unity
unity -projectPath /home/alexg/Dev/game4

# Initial scenes
# - Assets/Scenes/MainMenu.unity
# - Assets/Scenes/Playground.unity (test level)
```

### Building
```bash
# From project root (or use Unity Build Settings)
# Will vary by target platform (Windows/Mac/Linux)
```

### Running Tests (if added)
```bash
# Run in Unity Test Framework or custom test runner
```

## Important Notes for Implementation

**1. Input ≠ Direct Control**
- Player flick = target lean angle, not instant rotation
- Respects momentum and physics constraints
- Feels responsive but not twitchy

**2. Grind/Manual Detection**
- Requires physics raycasts to detect surface type
- Transition from grounded → grind requires specific angle + velocity conditions
- Don't allow grinds mid-air without proper setup

**3. Combo Preservation**
- Land trick → add to combo counter
- First frame of trick lands = must validate landing conditions
- Bail (loss of balance) = combo reset, penalty

**4. Animation Sync**
- Animator state must always reflect actual physics state (don't desync)
- Trick animations should be length-independent (use speed-up/slow-down if needed)

**5. Performance Considerations**
- Skateboard physics updates every frame (FixedUpdate)
- Raycasts for surface detection (grind/manual) are relatively cheap; cache when possible
- VFX should be pooled (sparks, dust clouds)

## Directory Structure (Expected)
```
Assets/
  Scenes/
    MainMenu.unity
    Playground.unity
  Scripts/
    Physics/
    Input/
    Tricks/
    Player/
    Animation/
    World/
    Camera/
    Feedback/
    UI/
    Editor/
  Art/
    Models/
    Animations/
    Materials/
    VFX/
  Audio/
    SFX/
    Music/
```

## Known Unknowns / Decisions TBD
- **Custom physics or Rigidbody?** — Start with built-in, switch if needed for precision
- **Grind/manual surfaces:** Physics layers vs trigger detection?
- **Animation system:** Mecanim or custom state machine?
- **Multiplayer scope:** If planned, what's the MVP? (local only, online, replays?)
