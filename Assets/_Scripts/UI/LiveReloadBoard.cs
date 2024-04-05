using System.Collections.Generic;
using System.IO.Ports;
using TMPro;
using UnityEngine;

public class LiveReloadBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TMP_Dropdown portsDropdown;
    [SerializeField] private EndEffectorManager endEffectorManager;
    
    private DeviceConfig customConfig;
    private string[] ports;
    private string targetPort;
    private bool isInvalid = false;
    
    private void Start()
    {
        customConfig = ScriptableObject.CreateInstance<DeviceConfig>();
        customConfig.ActuatorRotations = new ActuatorRotations();
        customConfig.EncoderRotations = new EncoderRotations();
        customConfig.Offset = new Offset();
        ports = GetAvailablePorts();
        portsDropdown.ClearOptions();
        portsDropdown.AddOptions(new List<string>(ports));
    }

    public void SetBoardType(int value)
    {
        customConfig.BoardType = value == 0 ? BoardTypes.Gen3 : BoardTypes.Gen2;
        Debug.Log(customConfig.BoardType.ToString());
    }
    
    public void SetCustomPort(int value)
    {
        targetPort = ports[value];
        Debug.Log(targetPort);
    }
    
    public void SetLeftEncoder(int value)
    {
        customConfig.EncoderRotations.Rotation1 = value == 0 ? Rotation.CW : Rotation.CCW;
        Debug.Log(customConfig.EncoderRotations.Rotation1.ToString());
    }
    
    public void SetRightEncoder(int value)
    {
        customConfig.EncoderRotations.Rotation2 = value == 0 ? Rotation.CW : Rotation.CCW;
        Debug.Log(customConfig.EncoderRotations.Rotation2.ToString());
    }

    public void SetLeftActuator(int value)
    {
        customConfig.ActuatorRotations.Rotation1 = value == 0 ? Rotation.CW : Rotation.CCW;
        Debug.Log(customConfig.ActuatorRotations.Rotation1.ToString());
    }
    
    public void SetRightActuator(int value)
    {
        customConfig.ActuatorRotations.Rotation2 = value == 0 ? Rotation.CW : Rotation.CCW;
        Debug.Log(customConfig.ActuatorRotations.Rotation2.ToString());
    }
    
    public void SetLeftOffset(string value)
    {
        customConfig.Offset.Left = int.Parse(value);
        Debug.Log(customConfig.Offset.Left);
        isInvalid = customConfig.Offset.Left is < 0 or > 360;
    }
    
    public void SetRightOffset(string value)
    {
        customConfig.Offset.Right = int.Parse(value);
        Debug.Log(customConfig.Offset.Right);
        isInvalid = customConfig.Offset.Left is < 0 or > 360;
    }

    public void SetBoardResolution(string value)
    {
        customConfig.Resolution = int.Parse(value);
        Debug.Log(customConfig.Resolution);
    }

    public void SetStylusButton(bool isFlipped)
    {
        customConfig.FlippedStylusButton = isFlipped;
        Debug.Log(customConfig.FlippedStylusButton);
    }

    public void LiveReload()
    {
        if(isInvalid)
        {
            title.SetText("Invalid!");
        }
        else
        {
            title.SetText("Updating Board!");
            endEffectorManager.ReloadBoard(customConfig, targetPort);
        }
    }
    
    private string [] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }
}