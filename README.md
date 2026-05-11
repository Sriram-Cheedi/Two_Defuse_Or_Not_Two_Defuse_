# COMS30043_2025_Team_Project
The Barbies - Two Defuse or Not Two Defuse

This project aims to create an innovative experience for players utilising the key technology of custom hardware.

The applications runs on a aysymmetric two input system, one within the Unity gameworld created for the project and one operating a custom hardware controller. 

Technical Specifications can be found here.

# Project Setup

To set this project up, a physical 'hardware bomb' is required. 

To Launch the 'hardware':
- Clone the git repository
- Open the Hardware/ControllerV0.1 in Visual Studio Code
- Install PlatformIO extention
- Plug the Teensy 4.1 microcontroller into PC
- press the PlatformIO Upload button and wait for the upload to finish.

To Lauch the project:
- Open Unity editor and import any required packages.
- Build the project using 'Build Profiles', selecting your given system.
- Connect the hardware to any open serial USB port on your device.
- Launch the excecutable.
- Enjoy!

In the case of not having the required hardware, the level will open, assiging a mock API, however the game will not be playable as interactions to navigate and access the game are absent. 

# Project Structure 
```
├── Games Project Pitch Design Documents.pdf
├── Hardware
│   ├── Circuit_Designs
│   ├── ControllerV0.1
│   ├── Example Code
│   └── Manufacturing Drawings
├── Hardware-Software_Interaction.md
├── README.md
├── Team_Project
│   ├── Assembly-CSharp-Editor.csproj
│   ├── Assembly-CSharp.csproj
│   ├── Assets
│   ├── CodeCoverage
│   ├── EditMode.csproj
│   ├── Final_Beta_Build
│   ├── Game.Main.csproj
│   ├── GamesDayTryRun
│   ├── KeypadAssembly.csproj
│   ├── LeaderBoardTesting
│   ├── Library
│   ├── Logs
│   ├── mono_crash.11f9d3f322.0.json
│   ├── mono_crash.23a2ae715.0.json
│   ├── obj
│   ├── Packages
│   ├── PlayMode.csproj
│   ├── PlaytestParty
│   ├── ProjectSettings
│   ├── Team_Project.sln
│   ├── Team_Project.sln.DotSettings.user
│   ├── Team_Project.slnx
│   ├── UpgradeLog.htm
│   └── UserSettings
└── The Barbies - Team Project Pitches.pdf
```
