using UnityEngine;

public class ConfigStorage : MonoBehaviour
{
    public DeviceConfig Config { get; set; }

    public string Port { get; set; }

    private void Start()
    {
        DontDestroyOnLoad(this);
        print(Config);
    }
}
