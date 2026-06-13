# Ashes of Outlaws

*Ashes of Outlaws* is a premium 3rd-person Western action-adventure game built in Unity 6 using the Universal Render Pipeline (URP). Take control of the Bandit character as they traverse a atmospheric, stylized environment.

---

## 📸 Gameplay Preview

<!-- 
TODO: Replace this placeholder path with a real screenshot of your game! 
To capture a screenshot:
1. Play the game in the Unity Editor.
2. Take a screenshot (e.g., Win + Shift + S on Windows).
3. Save it to the project folder as `gameplay_screenshot.png` and update the link below.
-->
![Ashes of Outlaws Gameplay](game.png)

---

## 🌟 Key Features

*   **Premium Over-The-Shoulder Camera**: A customized 3rd-person orbit camera (`BanditCameraOrbit`) that tracks the player closely, providing a cinematic over-the-shoulder perspective:
    *   **Terrain Jitter Filter**: Smooths position tracking with `Vector3.SmoothDamp` to eliminate high-frequency micro-collision jitter from the CharacterController.
    *   **Camera-Relative Offset**: Computes the shoulder offset relative to the camera's local right vector to maintain consistent placement when turning.
*   **Procedural Facial Animations**: An advanced facial animation script (`BanditFacialAnimator`) that brings the Bandit character to life:
    *   **Correct Eyeball Materials**: Programmatic configuration of inner (opaque URP with iris/pupil texture) and outer (transparent, high-smoothness URP eye-glass) materials for realistic rendering.
    *   **Parent-Space Rotations**: Corrects mirrored joint axes for blink, gaze, and brows by operating in `faceAttach` parent space.
    *   **Procedural Eyelids & Gaze**: Natural, randomized eye saccades and blinks.
    *   **Jaw Breathing & Reactive Eyebrows**: Subtle breathing movement on the jaw and brows reactive to gaze height.
*   **Stabilized Locomotion**: 
    *   **Damped Pelvis Stabilization**: Damps the pelvis bone's lateral sway by **75%** and slerps its local rotation towards its bind pose by **60%** in `LateUpdate()`. This eliminates side-to-side weaving/drifting while running while preserving necessary flexibility to prevent humanoid leg/IK buckling.
    *   **Speed Smoothing & Momentum**: Implements linear speed interpolation (momentum) to synchronize physical movement with the animator, preventing foot sliding.

*   **Cold Sunset Environment**: Set under Unity's default procedural skybox with warm, warm-toned sunset directional lighting for a moody, high-fidelity aesthetic.

---

## 🕹️ Controls

| Input | Action |
|---|---|
| **W / A / S / D** or **Arrow Keys** | Move Character (relative to Camera) |
| **Left Shift** | Sprint / Run |
| **Mouse Move** | Rotate Camera |
| **Escape** | Unlock Cursor |
| **Left Click** | Relock Cursor |

---

## 📂 Project Structure

```
Assets/
├── Bandit/
│   ├── fbx/                        # Bandit model and locomotion animations
│   ├── BanditAnimatorController    # Locomotion BlendTree controller (Idle -> Walk -> Run)
│   ├── BanditCameraOrbit.cs        # Camera orbit and tracking logic
│   └── BanditMovement.cs           # Standard player movement script
├── Scripts/
│   ├── AdvancedPlayerController.cs # Dynamic locomotion controller & hips stabilizer
│   └── BanditFacialAnimator.cs     # Procedural eye/jaw/brow face animator
├── Scenes/
│   └── Main.unity                  # Main playable scene
└── Editor/                         # Automation & scene configuration scripts
```

---

## ⚙️ Technical Requirements

*   **Game Engine**: Unity 6.4 (6000.4.8f1) or newer
*   **Render Pipeline**: Universal Render Pipeline (URP)
*   **Input System**: Supports both Unity's legacy Input Manager and the modern Input System package

---

## 🚀 Setup & Installation (Cloning Guide)

To run this project locally on your machine:

1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/dixonsimon/Ashes-of-Outlaws.git
    ```
    *(Replace the URL above with your actual repository link)*

2.  **Open in Unity Hub**:
    *   Open **Unity Hub**.
    *   Click **Add** -> **Add project from disk**.
    *   Select the cloned `Ashes of Outlaws` directory.
    *   Ensure the Unity Editor version is set to **6.4** (or `6000.4.8f1`+).

3.  **Load the Main Scene**:
    *   Once the project opens, navigate to `Assets/Scenes/` in the Project panel.
    *   Double-click to open `Main.unity`.
    *   Press the **Play** button at the top to enter gameplay mode.

---

## 🎖️ Credits & Asset Attribution

This project uses assets and resources from the following creators:

*   **Bandit Character Model & Rig**: Mixamo / Unity Asset Store character package (includes base humanoid rig and Locomotion animations).
*   **All Sky Free**: Developed by *Boris Vlasov* (Unity Asset Store) — provides the skybox and atmospheric profile for the "Cold Sunset" scene environment.
*   **Unity Demo Terrain**: Unity Technologies — provides the base terrain design, materials, and nature elements for the level environment.
