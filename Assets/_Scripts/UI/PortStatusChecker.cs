using System;
using System.IO.Ports;
using Haply.hAPI;
using TMPro;
using UnityEngine;

public class PortStatusChecker : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private string noPortsMessage;
    [SerializeField] private string incorrectPortMessage;
    [SerializeField] private string activeConnectionMessage;
    
    private void OnEnable()
    {
        CheckStatus();
    }

    public void CheckStatus()
    {
        if(GetAvailablePorts().Length<=0)
        {
            tmp.SetText(noPortsMessage);
            return;
        }

        if (!board.HasBeenInitialized)
        {
            tmp.SetText(incorrectPortMessage);
            return;
        }
        
        tmp.SetText(activeConnectionMessage);
    }
    
    private string [] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }
}
