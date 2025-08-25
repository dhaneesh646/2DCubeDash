🟦 Mask of secrets
📖 Overview

Cube Escape is a 2D puzzle-platformer prototype built in Unity.
The player controls a cube that can move, jump, charge jump, and dash to overcome obstacles.
The game introduces mechanics step by step through an onboarding level before progressing into challenges like breakable walls and moving platforms.



🎮 Controls

Move → A / D or ← / →
Jump → Space
Charged Jump → Hold Space (uses stamina)
Dash → Left Shift (uses stamina, can break walls)



🧩 Core Mechanics

Movement: Smooth & responsive platforming.
Jump System: Normal jump + charged jump with stamina.
Dash: Snappy dash, consumes stamina, breaks certain walls.
Breakable Walls: Can only be destroyed by dashing.
Checkpoints: Save progress mid-level.
Moving Platforms: Timed jumps required.
Stamina System: Limits charged jumps & dashing.



🗺️ Levels
1. Onboarding (Tutorial)

Learn to move, jump, charge jump, dash, and break walls.
Introduces checkpoints & moving platforms.
No enemies — focused on learning.



2. Challenge Level

Combines mechanics.
Requires stamina management.
Hazards like spikes and moving platforms.



⚡ Visual Design & FX

Trail Renderer → Dash effect.
Particles → Landing dust, breakable wall shards.
Camera Shake → Adds impact when breaking walls.
Lighting → Warm torches (#FFB347) + cool crystals (#00FFF7).



🛠️ Project Setup

Open Unity 2022 LTS or later.
Clone/download this repository.
Open the project in Unity Hub.
Load the Scenes/Onboarding.unity scene to start.



📌 Notes for Developers

Scripts are inside Scripts/
PlayerController.cs → Handles movement, jump, dash.
StaminaController.cs → Manages stamina usage/regeneration.
BreakableWall.cs → Breakable wall logic.
CameraShake.cs → Handles shake effect.
Art assets (icons, warning symbols, etc.) are inside Assets/projectresources/sprites.
Prefabs for walls, checkpoints, and moving platforms are in Assets/projectresources/Prefabs/.



🚀 Next Steps

Add polish: animations, VFX, better UI.
Add enemy interactions.
Expand puzzle-platformer mechanics.