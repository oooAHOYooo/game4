# Skate 4 Clone — Quick Start

## Auto-Setup (Recommended)

Once you open the project in Unity:

1. **In Unity Editor**, go to menu: **Skate > Setup Playground Scene**
   - Creates `Assets/Scenes/Playground.unity`
   - Adds `SceneInitializer` component
   - Auto-generates ground, skateboard rig, camera on play

2. **Set as Default**: **Skate > Set Playground as Default Scene**
   - Makes Playground scene auto-load when you hit Play

3. **Open Scene**: **Skate > Open Playground**
   - Quick shortcut to load the scene anytime

4. **Hit Play!**
   - Scene initializes procedurally
   - Check console for `[Scene]`, `[Rig]`, `[Camera]` logs

## Manual Setup (If Needed)

If the menu items don't appear:

1. Open Unity and let it compile
2. In `File > Build Settings`:
   - Drag `Assets/Scenes/Playground.unity` to the Scenes list
   - Make it index 0 (first)
3. In `Project Settings > Tags and Layers`:
   - Add a layer named `Ground` (any slot 8-31)
4. In the scene, find `_SceneInitializer` GameObject and inspect it—should show nothing (it auto-runs)

## Controls

```
WASD         → Steer (rotate direction)
J            → Push forward (hold skateboard, speed builds up)
Space        → Charge ollie (hold longer = higher jump)
  While airborne:
    J        → Kickflip
    L        → Heelflip
    I        → Pop Shove-it
```

Watch the orange board spin when you land tricks. Combo counter appears in the console.

## What's Generated

- **Ground**: 10×10 plane, gray material, physics collider
- **Skateboard Rig**:
  - Capsule body (rigid body, frozen X/Z rotation)
  - Orange cube child = board (spins during tricks)
  - `SkaterController` + `TrickSystem` components pre-attached
- **Camera**: Positioned 5m back, 2m up, looking at skater

## Troubleshooting

- **"Ground layer not found"** → Manually add "Ground" layer in Project Settings
- **Skater falls through ground** → Make sure ground Collider is not marked as Trigger
- **Board doesn't spin** → Check that `BoardMesh` is assigned in TrickSystem Inspector
- **No input response** → Verify Input System is enabled (usually default in newer Unity)

## What's Next

Edit the `SkaterController` and `TrickSystem` Inspector values to tune:
- `PushForce`, `MaxSpeed`, `TurnSpeed` for feel
- Ollie charge times and heights
- Trick execution speed and combo decay

Have fun! 🛹
