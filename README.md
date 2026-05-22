# Two Defuse or Not Two Defuse
> **Created by The Barbies** | COMS30043 2025 Team Project

This project aims to create an innovative and tense cooperative experience for players by utilising custom hardware. 

The application runs on an asymmetric two-input system: one player operates within the Unity game world created for the project, while the other operates a custom physical hardware controller to communicate and defuse the bomb.

Technical report on the project can be found [here](./docs/Report_Redacted.pdf) (May require downloading due to file size)
[![Watch the Technical Video](path/to/thumbnail.jpg)](https://github.com/felixUOB/Two_Defuse_Or_Not_Two_Defuse/blob/main/Technical_Trailer.mp4)

---

## Project Setup

To fully experience this project, a physical 'hardware bomb' controller is required.

### Prerequisites
*   **Microcontroller:** Teensy 4.1
*   **IDE:** Visual Studio Code with the [PlatformIO extension](https://platformio.org/)
*   **Game Engine:** Unity 6.3 LTS

### 1. Hardware Setup
1. Clone this git repository to your local machine.
2. Open the `Hardware/ControllerV0.1` folder in Visual Studio Code.
3. Ensure the **PlatformIO** extension is installed and active.
4. Plug the Teensy 4.1 microcontroller into your PC via USB.
5. Press the PlatformIO **Upload** button (the right-pointing arrow in the bottom status bar) and wait for the upload to finish.

### 2. Software (Unity) Setup
1. Open the `Team_Project` folder in the Unity Editor and import any required packages.
2. Build the project using **Build Profiles**, selecting your target operating system.
3. Connect the flashed hardware controller to any open serial USB port on your device.
4. Launch the built executable.
5. Enjoy and try not to explode!

> **Note:** If you do not have the required custom hardware, the level will still open and assign a Mock API. However, the game will **not be playable**, as the physical interactions required to navigate and access the game states will be absent.

---

## Project Structure

```text
├── Games Project Pitch Design Documents.pdf
├── Hardware
│   ├── Circuit_Designs
│   ├── ControllerV0.1
│   ├── Example Code
│   └── Manufacturing Drawings
├── Hardware-Software_Interaction.md
├── README.md
├── Team_Project  (Unity Project Root)
│   ├── Assets
│   ├── Packages
│   ├── ProjectSettings
│   └── ... (Builds and project files)
└── The Barbies - Team Project Pitches.pdf
