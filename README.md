# Welcome to HaptiEditor!

This is a project to showcase terrain painting and exploration at different resolution scales via the 2diy haply haptic board!

# Getting Started:

To get started, you will need either a Gen 2 or Gen 3 2diy Haply Haptic board, and Unity 2022.3.14f1 to open the project.
- Open the `Scenes` folder and launch the `NewEndEffector.unity` scene.
- In the `Resources` folder, duplicate any of the config files and set your appropriate board configuration.
- Save it, and drag it to the `EndEffectorManager`->`Pantograph` Gameobject's `Device.cs` script with the `ConfigData` serialised field.
- Pick your appropriate port from the `ActivePorts` dropdown list in `Board.cs` (If you are unsure, simply iterate through all active ports and check for errors in the console. If no port is showing, check your device connection)
- Play the scene, and click on the stylus button or the space bar to spawn circles in the scene, and feel the force feedback!
