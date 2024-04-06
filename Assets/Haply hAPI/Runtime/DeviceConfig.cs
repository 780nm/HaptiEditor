using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DeviceConfig", menuName = "ScriptableObjects/DeviceConfig", order = 1)]
public class DeviceConfig : ScriptableObject
{
    public BoardTypes BoardType = BoardTypes.Gen3;
    public EncoderRotations EncoderRotations;
    public ActuatorRotations ActuatorRotations;
    public Offset Offset;
    public int Resolution;
    public bool FlippedStylusButton;

    public void Init(DeviceConfig config)
    {
        BoardType = config.BoardType;
        EncoderRotations = new EncoderRotations
        {
            Rotation1 = config.EncoderRotations.Rotation1,
            Rotation2 = config.EncoderRotations.Rotation2
        };
        ActuatorRotations = new ActuatorRotations
        {
            Rotation1 = config.ActuatorRotations.Rotation1,
            Rotation2 = config.ActuatorRotations.Rotation2
        };
        Offset = new Offset
        {
            Left = config.Offset.Left,
            Right = config.Offset.Right
        };
        Resolution = config.Resolution;
        FlippedStylusButton = config.FlippedStylusButton;
    }
}
[Serializable]
public enum BoardTypes
{
    Gen2 = 0,
    Gen3 = 1
}

[Serializable]
public enum Rotation { CW = 0, CCW = 1 }

[Serializable]
public class EncoderRotations
{
    public Rotation Rotation1;
    public Rotation Rotation2;
}

[Serializable]
public class ActuatorRotations
{
    public Rotation Rotation1;
    public Rotation Rotation2;
}

[Serializable]
public class Offset
{
    public int Left;
    public int Right;
}