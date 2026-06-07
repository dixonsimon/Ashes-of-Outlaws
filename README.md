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

*   **Premium Over-The-Shoulder Camera**: A customized 3rd-person orbit camera (`BanditCameraOrbit`) that tracks the player closely, providing a cinematic over-the-shoulder perspective perfect for shooting and exploration.
*   **Procedural Facial Animations**: An advanced facial animation script (`BanditFacialAnimator`) that brings the Bandit character to life using bone-driven procedural techniques:
    *   **Natural Blinking**: Dynamic open/close eyelid movement driven by a smooth sine wave.
    *   **Eye Saccades**: Natural, randomized look-around eye tracking.
    *   **Jaw Breathing**: Subtle mouth/jaw opening and closing to simulate breathing when idle.
    *   **Reactive Eyebrows**: Eyebrows that dynamically raise or lower in response to eye gaze height.
*   **Stabilized Locomotion**: 
    *   Bakes character root motion position relative to the **Center of Mass** to ensure consistent trajectory.
    *   Dynamic local X-axis hips stabilization in `AdvancedPlayerController` to eliminate side-to-side weaving, keeping movements perfectly straight.
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
├── AllSkyFree/
│   └── Cold Sunset/                # Main scene folder
│       └── Cold Sunset.unity       # Active project scene
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
    *   Once the project opens, navigate to `Assets/AllSkyFree/Cold Sunset/` in the Project panel.
    *   Double-click to open `Cold Sunset.unity`.
    *   Press the **Play** button at the top to enter gameplay mode.

---

## 🎖️ Credits & Asset Attribution

This project uses assets and resources from the following creators:

*   **Bandit Character Model & Rig**: Mixamo / Unity Asset Store character package (includes base humanoid rig and Locomotion animations).
*   **All Sky Free**: Developed by *Boris Vlasov* (Unity Asset Store) — provides the skybox and atmospheric profile for the "Cold Sunset" scene environment.
*   **Unity Demo Terrain**: Unity Technologies — provides the base terrain design, materials, and nature elements for the level environment.
