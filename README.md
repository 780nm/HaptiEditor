# Welcome to HaptiEditor!

This is a project to showcase terrain painting and exploration at different resolution scales via the 2diy haply haptic board!

# Getting Started:

To get started, you will need either a Gen 2 or Gen 3 2diy Haply Haptic board, and Unity 2022.3.14f1 to open the project.
- Open the `Scenes` folder and launch the `NewEndEffector.unity` scene.
- In the `Resources` folder, duplicate any of the config files and set your appropriate board configuration.
- Save it, and drag it to the `EndEffectorManager`->`Pantograph` Gameobject's `Device.cs` script with the `ConfigData` serialised field.
- Pick your appropriate port from the `ActivePorts` dropdown list in `Board.cs` (If you are unsure, simply iterate through all active ports and check for errors in the console. If no port is showing, check your device connection)
- Play the scene, and click on the stylus button or the space bar to spawn circles in the scene, and feel the force feedback!

# Controls 

The project is meant for the Haply 2DIY Haptic board, the control of the end effector goes through that interface.
Although we have a few additionals keybind needed :
- `Arrow Up`: in any scene will **scale up** the End Effector representation
- `Arrow down`:  in any scene will **scale down** the End Effector representation
- `Haply 2DIY Gen3 stylus button`, `Spacebar`: will **interact** with the scene to press the buttons or **paint**. 

*(The UI is accessible with your mouse)*

# Contributors

[Sean Bocirnea](https://github.com/780nm) [(email)](mailto:sean@passingti.me)
[Rishav Banerkee](https://github.com/GAmuzak) [(email)](mailto:rishavbanerjee1710@gmail.com)
[Céleste Maruejol](https://github.com/CelesteMaru) [(email)](mailto:celeste.maruejol@gmail.com)