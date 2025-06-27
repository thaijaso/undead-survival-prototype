# 🧠 Zombie Rig Export Pipeline

A step-by-step guide to cleanly export your Auto-Rig Pro rigged zombie character from Blender to Unity, with jaw bones, proper weights, and stable materials.

---

## 🔧 Blender Rig + Mesh Prep

**1. Apply All Transforms**  
- Select mesh and armature  
- `Ctrl + A` → Apply Location, Rotation, Scale

**2. Use Auto-Rig Pro Generated Rig**  
- Click **Match to Rig** after placing markers  
- Enable both **Chin** and **Mouth** options for jaw bone support

**3. Place Facial Markers Correctly**  
- **Chin Base (top)** → placed near jaw pivot, inside the head  
- **Chin Tip (bottom)** → placed on the front of the chin  
- **Mouth Corners** → near outer corners of lips

**4. Bind to Rig**  
- Use **Auto-Rig Pro: Bind** to generated rig  
- Delete existing armature modifier if re-binding

**5. Weight Check**  
- Jaw bone should influence only the lower jaw  
- Head should remain stable during jaw movement

**6. Material & Texture Safety**  
- You can safely edit UVs, assign materials, and bake textures — these do **not** affect skinning or weights

---

## 🏗 Exporting to Unity

**7. Use Auto-Rig Pro Export Tool**  
- `File > Export > Auto-Rig Pro FBX`  
- Preset: **Unity / Game Engine**

**Rig Tab Settings**:
- ☑ Apply Transforms  
- ☑ Only Deform Bones  
- ☑ Full Facial  
- ☑ Export Twist  
- ☑ Push Additive  
- ☑ Fix Matrices  
- ☑ Force Rest Pose Export  
- Bone Axes: Y Primary, X Secondary  
- Units x100  
- Root: `root`

**Misc Tab Settings**:
- Smooth: **Normals Only**  
- ☑ Apply Modifiers  
- ☑ Convert Axes: Y up, Z forward  
- ☑ Bake Axis Conversion  
- ☐ Tangent Space  
- ☐ Triangulate (optional)  

**8. Delete Old `.fbx` in Unity Before Reimport**  
- Always delete the existing `.fbx` before replacing — Unity caches avatars and rig data

**9. Unity Import Settings**  
- Set to **Humanoid** or **Generic**  
- Create avatar from this model  
- Use external materials if needed  
- Import blend shapes if applicable

---

## 🎮 In-Game Setup (Unity + PuppetMaster)

**PuppetMaster initialized, limb setup pending**  
**Manual head dismemberment working**

- *(Planned)* Assign limbs to PuppetMaster muscle groups  
- *(Planned)* Enable limb detachment during runtime  
- *(Planned)* Trigger gore FX, physics forces, and particle feedback  

---

## 🦴 Limb Dismemberment Pipeline

**A. Prep Mesh Before Rigging**
- Fill any open geometry (shoulders, necks) using **F**
- Detach limbs using `P > Selection` in Edit Mode
  - e.g., arms, forearms, legs, head

**B. Rig After Splitting**
- Bind each detached mesh part to the same Auto-Rig Pro rig  
- Ensure skin weights still map correctly

**C. In-Game Setup** *(WIP)*  
- PuppetMaster used for ragdoll + runtime logic  
- Head already detaches — other limbs next  
- Add visual effects and colliders for realism
