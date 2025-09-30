# 🧠 Copilot Instructions for Undead Survival Game Prototype

## Project Overview
- Third-person zombie survival game in Unity 2022.3+ (URP)
- Core gameplay: night-time looting, physics-based combat, ragdoll enemies, day-time shop phase
- Modular systems: Player, Enemy, Inventory, Weapons, UI, and custom StateMachine

## Architecture & Key Patterns
- **State Machines**: Both `Player` and `Enemy` use generic `StateMachine<T>` (see `Common/StateMachine.cs`). States are implemented as classes in `Player/States/` and `Enemy/States/`.
- **Combat**: Weapons use `Weapon` MonoBehaviour (see `Weapons/Weapon.cs`) and reference a `WeaponConfig` ScriptableObject. Bullets (`Bullets/Bullet.cs`) apply physics, damage, and interact with PuppetMaster ragdoll limbs.
- **Ragdoll & Dismemberment**: Enemies use PuppetMaster for ragdoll physics. Limb dismemberment is planned; see `Docs/Zombie_Rig_Export_Pipeline.md` for rigging/export pipeline.
- **Inventory**: Managed by `Inventory.cs` (see `Inventory/`). Items are picked up via `ItemPickupInteractable.cs`.
- **UI**: HUD and weapon UI controllers in `UI/`. Player references these for health, weapon, and overlay updates.

## Developer Workflows
- **Play/Debug**: Open in Unity, press Play. Use WASD + mouse. Tab for inventory (WIP).
- **Rigging Pipeline**: For new enemies, follow `Docs/Zombie_Rig_Export_Pipeline.md` for Blender → Unity → PuppetMaster setup.
- **Weapon Setup**: Add new weapons by creating a prefab and a `WeaponConfig` asset. Reference in `PlayerWeaponManager`.
- **State Debugging**: State transitions are logged. Null state transitions are blocked and logged as errors.
- **Enemy Setup**: Each enemy prefab must have a `PuppetMaster`, `HealthManager`, and reference to `EnemyTemplate`.

## Conventions & Integration
- **Component Initialization**: All major systems (Player, Enemy, Weapon) initialize dependencies in `Awake`/`Start` and log missing references.
- **Layer Usage**: Bullet collision logic relies on custom layers (e.g., `EnemyRagdoll`, `Player`).
- **Third-Party Assets**: Final IK, PuppetMaster, A* Pathfinding, Synty POLYGON packs. All gameplay logic is custom.
- **Code Organization**: Scripts are grouped by feature (`Player/`, `Enemy/`, `Weapons/`, `Inventory/`, `UI/`, `Common/`).

## Examples
- **StateMachine Usage**: `stateMachine.SetState(new AimState(...))`
- **Weapon Firing**: `Weapon.Fire()` instantiates bullet, applies physics, and triggers effects.
- **Enemy Hit**: `Enemy.ProcessHit(damage, limb)` applies damage, triggers state change if health ≤ 0.

## References
- `README.md`: Project summary, tech stack, and workflow overview
- `Docs/Zombie_Rig_Export_Pipeline.md`: Blender-to-Unity rigging and dismemberment pipeline
- `Assets/Scripts/`: All core gameplay logic

---

For unclear or missing conventions, ask the user for clarification or examples from the codebase.