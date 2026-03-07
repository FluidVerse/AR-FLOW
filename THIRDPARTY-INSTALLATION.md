# Third-Party Asset Installation

Due to the Unity Asset Store End User License Agreement (EULA), we cannot redistribute certain third-party assets in
this public repository. Before running the project, you must download and import these assets manually.

Without these assets, the project will have missing scripts, broken prefabs, and compilation errors.

## Required Assets

Please download the following free assets from the Unity Asset Store using the same Unity account you use for the Unity
Editor:

1. **Joystick Pack** by Fenerax Studios
    * **Link**: [https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631](https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631)
    * **Description**: Used for on-screen mobile controls.

2. **Human Character Dummy** by Kevin Iglesias
    * **Link**: [https://assetstore.unity.com/packages/3d/characters/humanoids/humans/human-character-dummy-178395](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/human-character-dummy-178395)
    * **Description**: Used for the character model in the virtual lab.

3. **Human Basic Motions - Free** by Kevin Iglesias
    * **Link**: [https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271)
    * **Description**: Used for character animations.

## Installation Instructions

1. **Open the Project**: Open the project in Unity.
2. **Open Package Manager**: Go to `Window` > `Package Manager`.
3. **Select "My Assets"**: In the top-left dropdown of the Package Manager window, select **My Assets**.
    * *Note: You may need to sign in to your Unity account if you haven't already.*
4. **Find & Download**: Search for each of the assets listed above.
    * If you haven't "purchased" (claimed) them yet, you may need to click "Add to My Assets" on the Asset Store website
      links provided above first.
5. **Import**:
    * Select the asset in the list.
    * Click **Download** (if not already downloaded).
    * Click **Import**.
    * When the Import Unity Package window appears, ensure all files are selected and click **Import**.

## Folder Structure Confirmation & Troubleshooting

After importing, ensure the folders are located in the `Assets` directory as follows to match the existing meta files
and references:

* `Assets/Joystick Pack/`
* `Assets/Kevin Iglesias/` (contains both the Dummy model and Animations)

### Asset Usage & Manual Fixes

If the project still reports missing scripts or broken prefabs after importing, follow these steps to manually resolve
references in the main prefab:

**1. Joystick Pack**

* **Where it's used**: The on-screen joystick is part of the `Assets/Prefabs/Pipemaster_Base.prefab`.
* **How to fix**:
    * Open `Assets/Prefabs/Pipemaster_Base.prefab`.
    * Locate the `Joystick` GameObject in `Pipemaster_Base > Pipemaster_Movement > MovementCanvas > Joystick`.
    * The `Fixed Joystick` prefab in `Assets/Joystick Pack/Prefabs/Fixed Joystick.prefab` is a child of the `Joystick`
      GameObject. If it's missing, drag it from the Joystick Pack folder into the hierarchy as a child of `Joystick`.
    * Locate the `JoystickWrapper` script and assign the joystick component to its `stick` field.
    * Assign the missing sprites in `JoystickWrapper` using the sprites in `Assets/Joystick Pack/Sprites`.

**2. Human Character Dummy & Animations**

* **Where it's used**: The character model acts as the player model within `Assets/Prefabs/Pipemaster_Base.prefab`.
* **How to fix**:
    * Open `Assets/Prefabs/Pipemaster_Base.prefab`.
    * Locate the `DummyCharacter` GameObject in
      `Pipemaster_Base > Pipemaster_Movement > CameraManager > AerialCameraMode > DummyCharacter`.
    * The `HumanDummy_M Green` prefab in `Assets/Kevin Iglesias/Human Character Dummy/Prefabs/HumanDummy_M Green.prefab`
      is a child of the `DummyCharacter` GameObject. If it's missing, drag it from the Human Character Dummy folder into
      the hierarchy as a child of `DummyCharacter`.
    * Ensure the `Animator` component in the prefab is referencing the correct controller in
      `Assets/Kevin Iglesias/Human Animations/Unity Demo Scenes/Human Basic Motions/AnimatorControllers/HumanM@Idles.controller`.
    * Edit the controller to ensure that the model always stays in the idle animation state and never transitions to
      other states.
    * Add the `Visors` prefab from `Assets/Prefabs/Visors.prefab` as a child of `HumanDummy_M Green`.
    * Assign the whole model, the model's head and the `Visors` GameObject to the `DummyCharacterController` script in
      the `DummyCharacter` GameObject.
