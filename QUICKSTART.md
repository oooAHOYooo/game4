# Quick Start — 1 Command to Play

## Go!
```bash
./launch.sh
```

That's it! 🛹

- Unity launches automatically
- Scene generates on first run
- Enters Play mode immediately
- You're skateboarding

(Subsequent launches just hit Play — no re-setup needed)

---

## Controls (Once Playing)
| Key | Action |
|---|---|
| W/A/S/D | Steer direction |
| J | Push (hold ground) / Kickflip (airborne) |
| Space | Charge ollie (hold) + pop (release) |
| I | Pop Shove-it (airborne) |
| L | Heelflip (airborne) |

---

## Development Loop

### Tweak Feel
- Select **SkateRig > Body** in Hierarchy
- Inspector → **SkaterController** section
- Adjust `PushForce` (10 → 15), `MaxSpeed` (16 → 20), `TurnSpeed` (90 → 120)
- **Reload scene** (File > Reload Scene or press ⌘/Ctrl+R)

### Add Physics Constraints
- SkateRig > Body → Rigidbody → Constraints
  - Freeze Rotation X/Z prevents unwanted board flips

### Modify Trick Timing
- SkateRig > Body → TrickSystem section
- `TrickExecutionTime` (0.35s = spin duration)
- `LandingRotationTolerance` (25° = how flat must board be to land)

### Watch Combo Counter
- Play → Open Console (Window > General > Console)
- Look for `[COMBO]` and `[SLAM]` messages

---

## File Map
```
Assets/Scripts/
  Player/
    SkaterController.cs    ← Physics, input, state machine
    TrickSystem.cs         ← Trick detection & combo
    SceneInitializer.cs    ← Procedural generation
  Editor/
    SceneSetupMenu.cs      ← Menu items
```

## Reset / Reload
- **Full reset**: Skate > Setup Playground Scene (recreates everything)
- **Reload scene**: File > Reload Scene or Ctrl+R
- **New scene**: File > New Scene, then manually add `SceneInitializer`

---

## Troubleshooting

**Skater won't move?**
- Check Console for errors
- Verify Input System is active (Edit > Project Settings > Input System)
- Make sure Ground layer exists

**Board doesn't spin during tricks?**
- Select SkateRig > Body > TrickSystem
- Drag `SkateRig > Body > BoardMesh` into the `_BoardMesh` field
- (Should be auto-set if using Setup menu)

**Falls forever?**
- Ground plane might be too small or not on "Ground" layer
- Run Setup Playground Scene again

---

## Next Steps

Once you get the vibe dialed in:
1. Add more tricks (Impossible, Varial Kickflip, etc.)—edit `TrickType` enum in TrickSystem.cs
2. Add landing animations
3. Add camera follow/tracking
4. Add sound effects on push/ollie/land
5. Add grind/manual detection
