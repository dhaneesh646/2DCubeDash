Absolutely! Here's an enhanced and streamlined version of your README file, with improved clarity, formatting, and emphasis on your core concept. I’ve also removed any redundant or unclear phrasing while keeping the tone developer-friendly and engaging.

---

# 🟦 **Mask of Secrets**

## 📖 Overview

**Mask of Secrets** is a 2D puzzle-platformer prototype built in Unity.  
You play as a mysterious cube navigating two intertwined realities: the **Present World** and the **Parallel World**.

- The **Present World** is visible and mapped.
- The **Parallel World** is hidden and can only be revealed using the cube’s special ability: the **Mask of Secrets**.

Use movement, jumps, dashes, and your mask ability to uncover hidden dangers, break through obstacles, and escape both worlds.

---

## 🎮 Controls

| Action           | Key(s)                  |
|------------------|-------------------------|
| Move             | A / D or ← / →          |
| Jump             | Space                   |
| Charged Jump     | Hold Space (uses stamina) |
| Dash             | Left Shift (uses stamina, breaks walls and kill enemies) |
| Scan Parallel World | E (activate mask ability) |

---

## 🧩 Core Mechanics

- **Dual World System**: Switch between Present and Parallel Worlds using the mask ability.
- **Movement**: Smooth, responsive platforming.
- **Jump System**: Standard jump + charged jump (consumes stamina).
- **Dash**: Quick burst movement, breaks certain walls, consumes stamina.
- **Breakable Walls**: Only destroyed via dash.
- **Checkpoints**: Save progress mid-level.
- **Moving Platforms**: Require precise timing.
- **Stamina System**: Limits use of charged jump and dash.
- **Mask Ability**: Reveals hidden hazards and paths in the Parallel World.

---

## 🗺️ Levels

### 1. Onboarding (Tutorial)
- Learn core mechanics: movement, jump, charged jump, dash, mask ability.
- Introduces checkpoints and moving platforms.
- No enemies — focused on exploration and learning.

### 2. Challenge Level
- Combines all mechanics.
- Requires stamina management and mask usage.
- Hazards like spikes and moving platforms exist in the Parallel World and can kill the player.

---

## ⚡ Visual Design & FX

- **Trail Renderer**: Dash effect.
- **Particles**: Landing dust, wall shards.
- **Lighting**:  
  - Warm torches `#FFB347`  
  - Cool crystals `#00FFF7`

---

## 🛠️ Project Setup

1. Open Unity **6000.0.50f1 LTS** or later.
2. Clone or download this repository.
3. Open the project via Unity Hub.
4. Load the scene: `Scenes/Onboarding.unity`.

---

## 📌 Developer Notes

**Scripts Directory**: `Assets/projectresources/Scripts/`

| Script Name                  | Description                                      |
|-----------------------------|--------------------------------------------------|
| `PlayerController.cs`       | Handles movement, jump, dash.                   |
| `StaminaController.cs`      | Manages stamina usage and regeneration.         |
| `BreakableWall.cs`          | Logic for breakable walls.                      |
| `ParallelWorldMaskController.cs` | Reveals hidden hazards in the Parallel World. |
| `PlayerParticleController.cs` | Controlls Players particle effect. |
| `GameManager.cs` | Controlls Players Death,Repawn,Respawn point. |
| `AudioManager.cs` | Controlls wholegame Audio. |



**Assets Directory**:  
- Sprites: `Assets/projectresources/sprites/`  
- Prefabs: `Assets/projectresources/Prefabs/`

---

## 🚀 Next Steps

- Add polish: animations, VFX, and improved UI.
- Introduce enemy interactions.
- Expand puzzle-platformer mechanics with new challenges.

---

