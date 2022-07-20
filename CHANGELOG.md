# Changelog:

1.3.3
- The mod is now fixed for game version 1.30

1.3.2
- Added setting "Set Camera Position": The key that will set the camera position, rotation and size based on the "Camera Position/Rotation/Size" settings

1.3.1
- Renamed setting "Always Grid" to "Grid Everywhere": It will now let you enable/disable the grid everywhere instead of it always being activated
- The point of view buttons now still work if one of the Change Position/Rotation/Size At Start/Stop is disabled

1.3.0
- Changed setting "Change Position At Start": This will now only control if the pivot will change at the start of the simulation
- Added setting "Change Rotation At Start": Controls if the rotation of the camera will change at the start of the simulation
- Added setting "Change Size At Start": Controls if the size of the camera will change at the start of the simulation
- Changed setting "Change Position At Stop": This will now only control if the pivot will change at the end of the simulation
- Added setting "Change Rotation At Stop": Controls if the rotation of the camera will change at the end of the simulation
- Added setting "Change Size At Stop": Controls if the size of the camera will change at the end of the simulation
- Added setting "Position Boundaries": Controls if the movement of the camera is limited
- Added setting "Rotation Boundaries": Controls if the rotation of the camera is limited
- Added setting "Recenter Button": Controls if the recenter button will be displayed when the camera is to far away from the main island
- Added setting "Always Grid": Controls if the grid will always be displayed even in simulation mode
- Added setting "Visualize Pivot": Controls if a sphere will be displayed at the pivot point of the camera
- Added setting "Camera Position": Sets the position of the camera (This will change to the current position of the camera when you reset this value)
- Added setting "Camera Rotation": Sets the rotation of the camera (This will change to the current rotation of the camera when you reset this value)
- Added setting "Camera Size": Sets the size of the camera (This will change to the current size of the camera when you reset this value)
- Added setting "Theme": Controls in what theme everything will be displayed in (Sim Mode, Build Mode or Sandbox Mode)
- Added setting "Main Menu World": Controls the world that will get loaded as the background of the main menu
- Added setting "Custom Main Menu": Controls if the game should load custom layouts as the background of the main menu (if "Main Menu World" is default it will chose randomly between the worlds that have custom layouts)
- Added setting "Third Person": The key that will switch between a first person and third person offset if auto offset is enabled
- Changed setting "Position Offset": Changed this from a Vector2 to a Vector3 so you can also add a offset in the z direction in first person mode
- Added setting "Rotation Offset": A offset that will be added to the rotation off the car in first person mode

1.2.2
- Added setting "Background Color": The background color that gets used when the gradient background is disabled
- Fixed bug where the firstperson camera would not activate at the begining of the simulation

1.2.1
- The game will now tell you when there is an update
- Added setting "Change Projection": When the keybind is pressed the projection type will switch between perspective and orthographic
- Added setting "Field of View": The vield of view of the camera when using the perspective projection type (first person also uses this)

1.2.0
- Optimisation (Changed most of the static vars to normal vars)

1.1.0
- Added setting "Change Position At Start": Controls if the camera moves when you start the simulation
- Added setting "Change Position At Stop": Controls if the camera moves when you stop the simulation
- Added setting "Rotate Everywhere": Controls if you can rotate the camera everywhere
- Added setting "Toggle Rotate Everywhere": The key that will toggle the rotate everywhere setting

1.0.1
- First person will now also show up in the replay
- Somehow fixed a bug where something would be set to null and break the game when targeting vehicle N and going to a level where there is less then N vehicles (I think its the same bug reported in #bug-reports), I have no idea why it did that but its fixed now ¯\_(ツ)_/¯ (At least I think it is...)