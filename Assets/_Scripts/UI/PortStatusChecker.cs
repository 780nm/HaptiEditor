using System;
using System.IO.Ports;
using Haply.hAPI;
using TMPro;
using UnityEngine;

public class PortStatusChecker : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private TextMeshProUGUI tmp;
    
    private void OnEnable()
    {
        CheckStatus();
    }

    public void CheckStatus()
    {
        if(GetAvailablePorts().Length<=0)
        {
            tmp.SetText("Please connect your board!");
            return;
        }

        if (!board.HasBeenInitialized)
        {
            tmp.SetText("Please select correct port!");
            return;
        }
        
        tmp.SetText("Board is connected!");
    }
    
    private string [] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }
}
