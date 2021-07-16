# Changelog:

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