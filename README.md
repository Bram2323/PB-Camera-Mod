# CameraMod

What it does: It adds more camera controls

Game Version: 1.28+

Dependencies: PolyTech Framework 0.9.5

To install: Place this .dll in the "...\Poly Bridge 2\BepInEx\plugins" folder


# Settings:
Enable/Disable Mod: Enables/Disables the mod

Change Position At Start: Change the position of the camera at the start of a simulation

Change Rotation At Start: Change the rotation of the camera at the start of a simulation

Change Size At Start: Change the size of the camera at the start of a simulation

Change Position At Stop: Change the position of the camera at the end of a simulation

Change Rotation At Stop: Change the rotation of the camera at the end of a simulation

Change Size At Stop: Change the size of the camera at the end of a simulation

Rotate Everywhere: Controls if you can rotate the camera everywhere

Toggle Rotate Everywhere: The key that will toggle the rotate everywhere setting

Position Boundaries: Enable/Disable position boundaries for the camera

Rotation Boundaries: Enable/Disable rotation boundaries for the camera

Recenter Button: Enable/Disable the recenter button

Always Grid: Controls if the grid should always be displayed

Visualize Pivot: Controls if there will be a sphere displayed at the pivot point of the camera

Camera Position: Set the position of the camera (Reset to get the position of the camera)

Camera Rotation: Set the rotation of the camera (Reset to get the rotation of the camera)

Camera Size: Set the size of the camera (Reset to get the size of the camera)

Follow Position: The camera will follow the target's position

Follow Rotation: The camera will follow the target's rotation

Background: Enables/Disables the gradient background

Background Color: The color that gets used as the background when the gradient background is disabled

Theme: The theme everything gets displayed in

Main Menu World: The world that gets loaded as the background of the main menu

Custom Main Menu: Controls if it should load custom levels as the background of the main menu

First Person: Will give a first person perspective of the target

Auto Offset: Automaticaly add a offset based on the target vehicle type

Third Person: The key that will switch between first person and third person view when auto offset is enabled

Position Offset: A offset thats added to the position off the car in first person mode

Rotation Offset: A offset thats added to the rotaion of the car in first person mode

Change Target: The button that will change the target

Change Projection: The button that changes the camara projection type

Field of View: The vield of view when using perspective projection type

#How to add custom levels for the main menu:
- Run the game once to let the mod generate some folders
- Make a level in sandbox and save it
- Go to '...\Documents\Dry Cactus\Poly Bridge 2\Sandbox' and copy the .layout file with the name of your level
- Go to '...\Poly Bridge 2\BepInEx\plugins\CameraMod\Custom Menu Layouts\<World Name>' and paste the .layout in the folder
- Run the game and enable "Custom Main Menu" in the settings to make it load your levels (if you have "Main Menu World" set to default it will pick randomly between the worlds with custom layouts)
- The game will run the levels in alphabetical order!

#How to add custom camera angles for custom main menu levels
- Go to '...\Poly Bridge 2\BepInEx\plugins\CameraMod\Custom Menu Layouts\<World Name>' and make a .txt file with the same name as the .layout file
- Use this format to add the custom camera angles in the .txt file: 
" 
	<Position.x>, <Position.y>, <Position.z>,
	<Rotation.x>, <Rotation.y>, <Rotation.z>,
	<Size>
"
- You can get these values by loading the level in sandbox and when the camera angles are right reset the settings "Camera Position", "Camera Rotation" and "Camera Size", those are the values you have to copy
