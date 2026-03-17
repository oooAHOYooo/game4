# Skateboarding Game - Controls & Level Building

## Player Controls

| Key | Action |
|-----|--------|
| **W/A/S/D** | Steer / Move direction |
| **SPACE** | Ollie (Jump) - Hold to charge for more height |
| **J** | Push forward (grounded) / **Kickflip** (airborne) |
| **I** | Pop ShoveIt (airborne only) |
| **L** | Heelflip (airborne only) |

### Tips
- **Steering:** Use WASD to turn left/right and move forward/back while on the ground
- **Ollies:** Hold SPACE longer for higher ollies (charges up to 0.7 seconds)
- **Tricks in air:** You can only do ONE trick per air session (J/I/L)
- **Landing:** Tricks must land flat to count (within 25° tolerance)

---

## Building Your Level

The game includes a level piece system for easily creating skate parks. You can:

### Option 1: Use the Auto-Generated Park
The default scene (`Playground.unity`) includes example pieces already set up. Just edit them in the inspector.

### Option 2: Add Pieces via Code
In a script, use the `LevelBuilder` class:

```csharp
var levelBuilder = gameObject.AddComponent<LevelBuilder>();

// Quarter-pipe ramp
levelBuilder.AddQuarterPipe(new Vector3(0, 0, 0), height: 2.5f, length: 4f);

// Grind rail
levelBuilder.AddGrindRail(new Vector3(3, 2, 0), length: 8f, height: 1.5f);

// Box/Ledge
levelBuilder.AddSkateBox(new Vector3(0, 0, 5), width: 2f, height: 0.8f, depth: 3f);

// Half-pipe
levelBuilder.AddHalfPipe(new Vector3(-5, 0, 0), radius: 3f, width: 6f);

// Stairs
levelBuilder.AddStairs(new Vector3(5, 0, 0), steps: 5, stepHeight: 0.3f);

// Flat surface / platform
levelBuilder.AddFlatSurface(new Vector3(0, 0, -5), width: 5f, length: 10f);
```

### Option 3: Manual Scene Editing (Advanced)
1. Create an empty GameObject
2. Add a script component (QuarterPipe, GrindRail, SkateBox, HalfPipe, or Stairs)
3. Position and configure in inspector

---

## Level Piece Types

### **QuarterPipe**
- A curved ramp that slopes up
- Great for launching tricks and building speed
- **Parameters:** height, length, width

### **GrindRail**
- A cylindrical rail for grinding
- Perfect for long grinds and manuals
- **Parameters:** length, railRadius, height

### **SkateBox**
- A simple rectangular box/ledge
- Good for manuals, landing tricks
- **Parameters:** width, height, depth

### **HalfPipe**
- A curved U-shaped surface
- For vert skating and high-air tricks
- **Parameters:** radius, width, curveSegments

### **Stairs**
- Discrete steps going up
- For launching gaps and practicing ollies
- **Parameters:** stepCount, stepHeight, stepDepth, stairWidth

### **FlatSurface**
- A simple flat platform
- Use for landing areas and flat ground
- **Parameters:** width, length

---

## Creating Custom Layouts

Edit `Assets/Scripts/Player/SceneInitializer.cs` in the `AddExamplePieces()` method to customize the default park:

```csharp
private void AddExamplePieces()
{
    var levelBuilder = new GameObject("_LevelBuilder").AddComponent<LevelBuilder>();

    // Add your pieces here
    levelBuilder.AddQuarterPipe(new(0, 0, 0));
    levelBuilder.AddGrindRail(new(3, 2, 0));
    // ... etc
}
```

---

## Troubleshooting

**Can't control the skater?**
- Check that the "Ground" layer exists (Project Settings > Tags & Layers)
- Verify the ground plane has a collider and is on the Ground layer
- Look at the Console for warnings about missing layers

**Pieces aren't rendering?**
- Make sure they have a MeshRenderer component
- Check that a material is assigned
- Verify colliders are present for physics

**Skater falling through ground?**
- Ground collider may not be set up properly
- Rigidbody constraints might be wrong
- Try adjusting ground raycast distance in SkaterController inspector

---

## Next Steps

- Improve trick detection (currently simple single-button triggers)
- Add manual tricks and grinds
- Implement combo system properly
- Add animations and sound effects
- Create more complex level geometry
