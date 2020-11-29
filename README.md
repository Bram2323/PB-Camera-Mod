# CameraMod

What it does: It adds more camera controls

Game Version: 1.24+Mod Version: 1.1.0

Dependencies: PolyTech Framework 0.7.

To install: Place this .dll in the ...\Poly Bridge 2\BepInEx\plugins folder


# Settings:
Enable/Disable Mod: Enables/Disables the mod

Change Position At Start : Controls if the camera moves when you start the simulation

Change Position At Stop : Controls if the camera moves when you stop the simulation

Rotate Everywhere : Controls if you can rotate the camera everywhere

Toggle Rotate Everywhere : The key that will toggle the rotate everywhere setting

Follow Position: The camera will follow the target's position

Follow Rotation: The camera will follow the target's rotation
Background: Enables/Disables the gradient sky

First Person: Will give a first person perspective of the target

Auto Offset: Automaticaly add a offset based on the target vehicle type

Offset: A offset thats added to the 0 0 point off the car when using first person view

Change Target: The button that will change the target



# Changelog:
1.0.1
- First person will now also show up in the replay
- Somehow fixed a bug where something would be set to null and break the game when targeting vehicle N and going to a level where there is less then N vehicles (I think its the same bug reported in #bug-reports), I have no idea why it did that but its fixed now ¯\_(ツ)_/¯ (At least I think it is...)

1.1.0
- Added setting "Change Position At Start" : Controls if the camera moves when you start the simulation
- Added setting "Change Position At Stop" : Controls if the camera moves when you stop the simulation
- Added setting "Rotate Everywhere" : Controls if you can rotate the camera everywhere
- Added setting "Toggle Rotate Everywhere" : The key that will toggle the rotate everywhere setting

1.2.0
- Optimisation (Changed most of the static vars to normal vars)