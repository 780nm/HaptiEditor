using System;
using System.Collections.Generic;
using System.IO.Ports;
using Haply.hAPI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LiveReloadBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TMP_Dropdown portsDropdown;
    [SerializeField] private EndEffectorManager endEffectorManager;
    [SerializeField] private Board board;
    [SerializeField] private Device device;
    [SerializeField] private DeviceConfig gen2Default;
    [SerializeField] private DeviceConfig gen3Default;
    [Space(10)] 
    [SerializeField] private TMP_Dropdown boardTypeField;
    [SerializeField] private TMP_Dropdown customPortField;
    [SerializeField] private TMP_Dropdown leftEncoderField;
    [SerializeField] private TMP_Dropdown rightEncoderField;
    [SerializeField] private TMP_Dropdown leftActuatorField;
    [SerializeField] private TMP_Dropdown rightActuatorField;
    [SerializeField] private TMP_InputField leftOffsetField;
    [SerializeField] private TMP_InputField rightOffsetField;
    [SerializeField] private TMP_InputField boardResolutionField;
    [SerializeField] private Toggle flippedStylusButtonField;
    
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
        RefreshPorts();
        SetFullConfig(device.ConfigData);
    }

    public void RefreshPorts()
    {
        title.SetText("Live Board Configurator");
        ports = GetAvailablePorts();
        portsDropdown.ClearOptions();
        portsDropdown.AddOptions(new List<string>(ports));
    }

    private void SetFullConfig(DeviceConfig config)
    {
        int portIndex = 0;
        string[] availablePorts = GetAvailablePorts();
        foreach (string port in availablePorts)
        {
            if (port.Equals(board.TargetPort))
            {
                break;
            }
            portIndex++;
        }
        customPortField.value = portIndex < availablePorts.Length ? portIndex : 0;
        boardTypeField.value = config.BoardType == BoardTypes.Gen3 ? 0 : 1;
        leftEncoderField.value = config.EncoderRotations.Rotation1 == Rotation.CW ? 0 : 1;
        rightEncoderField.value = config.EncoderRotations.Rotation2 == Rotation.CW ? 0 : 1;
        leftActuatorField.value = config.ActuatorRotations.Rotation1 == Rotation.CW ? 0 : 1;
        rightActuatorField.value = config.ActuatorRotations.Rotation2 == Rotation.CW ? 0 : 1;
        leftOffsetField.text = config.Offset.Left.ToString();
        rightOffsetField.text = config.Offset.Right.ToString();
        boardResolutionField.text = config.Resolution.ToString();
        flippedStylusButtonField.isOn = config.FlippedStylusButton;
        
        customConfig = ScriptableObject.CreateInstance<DeviceConfig>();
        customConfig.Init(config);
    }

    public void SetPreset(int value)
    {
        SetFullConfig(value == 0 ? gen3Default : gen2Default);
    }

    public void SetBoardType(int value)
    {
        customConfig.BoardType = value == 0 ? BoardTypes.Gen3 : BoardTypes.Gen2;
    }
    
    public void SetCustomPort(int value)
    {
        targetPort = ports[value];
    }
    
    public void SetLeftEncoder(int value)
    {
        customConfig.EncoderRotations.Rotation1 = value == 0 ? Rotation.CW : Rotation.CCW;
    }
    
    public void SetRightEncoder(int value)
    {
        customConfig.EncoderRotations.Rotation2 = value == 0 ? Rotation.CW : Rotation.CCW;
    }

    public void SetLeftActuator(int value)
    {
        customConfig.ActuatorRotations.Rotation1 = value == 0 ? Rotation.CW : Rotation.CCW;
    }
    
    public void SetRightActuator(int value)
    {
        customConfig.ActuatorRotations.Rotation2 = value == 0 ? Rotation.CW : Rotation.CCW;
    }
    
    public void SetLeftOffset(string value)
    {
        customConfig.Offset.Left = int.Parse(value);
        isInvalid = customConfig.Offset.Left is < 0 or > 360;
    }
    
    public void SetRightOffset(string value)
    {
        customConfig.Offset.Right = int.Parse(value);
        isInvalid = customConfig.Offset.Left is < 0 or > 360;
    }

    public void SetBoardResolution(string value)
    {
        customConfig.Resolution = int.Parse(value);
    }

    public void SetStylusButton(bool isFlipped)
    {
        customConfig.FlippedStylusButton = isFlipped;
    }

    public void LiveReload()
    {
        if(isInvalid)
        {
            title.SetText("Invalid! Check Offsets!");
            throw new Exception("Not Valid Config Values!");
        }
        targetPort = GetAvailablePorts()[portsDropdown.value];
        title.SetText("Updating Board!");
        endEffectorManager.ReloadBoard(customConfig, targetPort);
    }
    
    private string [] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }
}