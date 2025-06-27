# 💀 Undead Survival Game Prototype

> A third-person survival prototype where you scavenge cursed ruins by night and manage a survivor shop by day.\
> Features modular enemy design, ragdoll-driven combat, and evolving upgrade paths.

This is a third-person zombie survival game prototype developed in Unity, featuring physics-based combat, night-time looting, and a day-time shop phase. Built with custom systems and modular characters designed for reactive gameplay and ragdoll-driven drama.

---

## 🎮 Gameplay Summary

- Loot dangerous zones at night for supplies
- Face off against zombies and other undead foes (skeletons and cursed entities planned)
- Fight off zombies with guns, physics-driven bullets, and ragdoll enemies
- Return home and sell your loot in the shop phase
- Upgrade your weapons, gear, and character
- Future goals: multiple character classes (e.g., gunner, mage), companions, and narrative events

---

## 🧠 Current Systems Implemented

| System           | Description                                      |
| ---------------- | ------------------------------------------------ |
| 🎯 Combat        | Rigidbody bullets, ragdoll impact, hit detection |
| 🦟 Zombie AI     | Basic AI states (idle, aggro, attack, death)     |
| 🧴 Character Rig | Blender > Unity pipeline with facial rig + jaw   |
| 🏟 Level Design  | Gated looting zone inspired by                   |

|   |
| - |

| *Resident Evil 4* **(WIP)** |                                                                  |
| --------------------------- | ---------------------------------------------------------------- |
| 🛒 Shop Phase               | Placeholder logic for day-time selling/upgrades **(WIP)**        |
| 📽 Animation                | PuppetMaster ragdoll integration (head dismemberment functional) |
| 📆 Inventory                | Lightweight inventory system **(WIP)**                           |

---

## 🖼️ Screenshots / Demo

> Physics-driven bullet impact on ragdoll zombies in looting phase

---

## 🗃️ Technical Docs

- [`Zombie Rig Export Pipeline`](docs/Zombie_Rig_Export_Pipeline.md):\
  Blender-to-Unity rigging process using Auto-Rig Pro, with jaw bone, clean FBX export, and dismemberment prep.

More docs coming soon (AI logic, item system, loot tables, etc.)

---

## 🛠 Tools & Tech

### Core Tools

- Unity 2022 (URP)
- Blender + Auto-Rig Pro
- Final IK + PuppetMaster
- Shader Graph
- VS Code, Git, GitHub

### Third-Party Assets Used

- **POLYGON Apocalypse** (Synty Studios)
- **POLYGON Particle FX** (Synty Studios)
- **Apocalypse HUD - Synty INTERFACE**
- **Zombie Animation Set**
- **VertexModeler Character**
- **Final IK** + **PuppetMaster** (RootMotion)
- **A\*** Pathfinding Project Pro
- **MK Toon - Stylized Shader**
- **Weapons Pack - Bullets / FREE**
- **Gun Sounds Pack Vol 1**
- **Simple Sky - Cartoon Assets**
- **ARPG Pack**
- **Crosshairs**

These assets were used for prototyping visuals, sound, and tooling. All gameplay systems, rigging, and logic were developed independently.

---

## 📂 Project Structure

```
/Assets/               # Unity game files
/Blend/                # Blender source files
/Exports/              # Final FBX exports
/Docs/                 # Rig + pipeline documentation
```

---

## 🚧 Next Milestones

- 🎯 Assign limbs to PuppetMaster muscles for runtime dismemberment
- 🌇 Add scene transition logic between looting and shop phases
- 💾 Implement save/load system to persist inventory, money, and upgrades across sessions
- 🧟‍♂️ Expand zombie AI with navigation, chase, and group behaviors
- 🛍 Add shopkeeper NPC with UI for upgrades and weapon purchases
- 🗃 Implement full inventory UI with drag/drop and slot system
- 💥 Add gore FX and hit reactions using Shader Graph + particle systems
- 🔁 Build enemy spawn system with wave escalation logic
- 🧙‍♂️ Prototype second character class (e.g., mage with impact-based spells)
- 📊 Track loot run performance with end-of-run stat summary

---

## 🧰 Key Contributions

- Designed and implemented third-person shooting system with physics-based impact
- Integrated PuppetMaster and Final IK with custom character rigs
- Built Blender-to-Unity rig pipeline with facial animation support
- Developed early gameplay loop: looting → selling → upgrading

---

## 👤 Author

Developed solo by Jason Thai

This project highlights skills in system design, gameplay programming, and technical art workflows.

---

## 🕹 How to Play

- Open in Unity 2022.3+ (URP)
- Press Play to begin in looting phase
- Use WASD + mouse to move/aim
- Press Tab to switch to inventory (WIP)

---

## 📬 Contact

Questions or feedback? Feel free to reach out via email at [thaijaso@gmail.com](mailto\:thaijaso@gmail.com).

