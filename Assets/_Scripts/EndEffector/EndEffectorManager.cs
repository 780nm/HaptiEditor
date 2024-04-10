using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Haply.hAPI;
using UnityEngine;
using UnityEngine.Events;

public class EndEffectorManager : MonoBehaviour
{
    #region Public Vars

    /// <summary>
    /// Event fired on every simulation step, with sensor data
    /// </summary>
    public UnityAction<float[]> OnSimulationStep;
    public float positionScale = 1;

    #endregion

    #region Sfield Vars

    [SerializeField] private Board haplyBoard;
    [SerializeField] private Device device;
    [SerializeField] private Pantograph pantograph;
    [SerializeField] private bool is3D;
    [SerializeField] private GameObject endEffectorActual;
    [Range(1,100)] [SerializeField] private float movementScalingFactor;
    
    #endregion

    #region Member Vars
    
    private Task simulationLoopTask;
    private CancellationTokenSource cancellationTokenSource;
    private Vector3 initialOffset;
    private object concurrentDataLock;
    private float[] sensors;
    private float[] angles;
    private float[] endEffectorPosition;
    private float[] endEffectorForce;
    private float[] torques;

    #endregion

    #region UnityFunctions

    private void Awake()
    {
        concurrentDataLock = new object();
        angles = new float[2];
        endEffectorPosition = new float[2];
        endEffectorForce = new float[2];
        torques = new float[2];
        if (haplyBoard == null) FindObjectOfType<Board>();
        if (device == null) FindObjectOfType<Device>();
        if (pantograph == null) FindObjectOfType<Pantograph>();
    }

    private void Start()
    {
        ConfigStorage configStorage = GameObject.FindWithTag("Transition").GetComponent<ConfigStorage>();
        LoadBoard(configStorage.Config, configStorage.Port);
    }

    public void ReloadBoard(DeviceConfig customConfig, string targetPort)
    {
        haplyBoard.DestroyBoard();
        CancelSimulation();
        SetForces(0f, 0f);
        Destroy(pantograph.gameObject.GetComponent<Device>());
        device = pantograph.gameObject.AddComponent<Device>();
        device.Init();
        LoadBoard(customConfig, targetPort);
        ButtonHandler buttonHandler = gameObject.GetComponent<ButtonHandler>();
        if (buttonHandler == null) return;
        buttonHandler.SetButtonState(customConfig.FlippedStylusButton);
    }

    // public void ResetPosition()
    // {
    //     ForceFeedbackHandler handler = gameObject.GetComponent<ForceFeedbackHandler>();
    //     if(handler!=null)
    //     {
    //         handler.enabled = false;
    //         StartCoroutine(EnableForceFeedback(handler));
    //     }
    //     endEffectorActual.transform.position -= initialOffset;
    // }
    //
    // private IEnumerator EnableForceFeedback(ForceFeedbackHandler handler)
    // {
    //     yield return new WaitForSeconds(0.3f);
    //     handler.enabled = true;
    // } 

    private void LoadBoard(DeviceConfig customConfig = null, string targetPort = null)
    {
        device.LoadConfig(customConfig);
        haplyBoard.Initialize(targetPort);
        device.DeviceSetParameters();
        angles = new float[2];
        torques = new float[2];
        sensors = new float[1];
        endEffectorPosition = new float[2];
        endEffectorForce = new float[2];
        GetPosition();
        device.DeviceWriteTorques();
        initialOffset = endEffectorActual.transform.position;
        sensors[0] = 0f;

        simulationLoopTask = new Task( SimulationLoop );
        simulationLoopTask.Start();
    }

    private void LateUpdate()
    {
        if (!haplyBoard.HasBeenInitialized) return;
        UpdateEndEffectorActual();
    }

    #endregion
    
    #region Simulation

    private void SimulationLoop()
    {
        TimeSpan length = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);
        Stopwatch sw = new();
        cancellationTokenSource = new CancellationTokenSource();

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            sw.Start();
            Task simulationStepTask = new(() => SimulationStep(cancellationTokenSource.Token));
            simulationStepTask.Start();
            simulationStepTask.Wait(cancellationTokenSource.Token);
            while (sw.Elapsed < length)
            {
                // limits speed of simulation
            }
            sw.Stop();
            sw.Reset();
        }
    }
    
    private void CancelSimulation()
    {
        cancellationTokenSource?.Cancel();
    }
    
    private void SimulationStep(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        lock (concurrentDataLock)
        {
            GetPosition();
            if (haplyBoard.DataAvailable()) device.GetSensorData(ref sensors);
            // if(buttonHandler!=null) buttonHandler.DoButton(sensors[0]);
            OnSimulationStep?.Invoke(sensors);
            device.SetDeviceTorques(endEffectorForce, torques);
            device.DeviceWriteTorques();
        }
    }

    private void GetPosition()
    {
        if (!haplyBoard.DataAvailable()) return;
        device.DeviceReadData();
        device.GetDeviceAngles(ref angles);
        device.GetDevicePosition(angles, endEffectorPosition);
        endEffectorPosition = DeviceToGraphics(endEffectorPosition);

        endEffectorPosition[1] += 0.03f; // Account for Haply baseline offset
        endEffectorPosition[0] *= positionScale;
        endEffectorPosition[1] *= positionScale;
    }

    #endregion
    
    #region Utils

    /// <summary>
    /// Check if the device button is flipped or not
    /// </summary>
    /// <returns>boolean value of button flipped status</returns>
    public bool GetButtonState() => device.CheckButtonFlipped();

    public float[] GetForces()
    {
        return endEffectorForce;
    }

    /// <summary>
    /// Set End Effector Forces for force feedback
    /// </summary>
    /// <param name="xVal">Forces in the horizontal axis</param>
    /// <param name="yVal">Forces in the vertical axis</param>
    public void SetForces(float xVal, float yVal)
    {
        endEffectorForce[0] = xVal;
        endEffectorForce[1] = yVal;
    }
    
    private void UpdateEndEffectorActual()
    {
        Transform endEffectorTransform = endEffectorActual.transform;
        Vector3 position = endEffectorTransform.position;

        lock (concurrentDataLock)
        {
            position.x = endEffectorPosition[0];
            // position.y = is3D ? position.y: endEffectorPosition[1];
            // position.z = is3D ? endEffectorPosition[1] : position.z;
            if (is3D) position.z = endEffectorPosition[1];
            else position.y = endEffectorPosition[1];
        }

        Vector3 targetPosition = position * movementScalingFactor;
        targetPosition = targetPosition.XZPlane(endEffectorTransform.position.y);
        endEffectorTransform.position = targetPosition;
    }

    private static float[] DeviceToGraphics(float[] position)
    {
        return new[] {-position[0], -position[1]};
    }

    #endregion
}
