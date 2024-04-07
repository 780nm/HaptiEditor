using UnityEngine;

public class UserConfigCache : MonoBehaviour
{
    private DeviceConfig config;
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SaveUserConfig(DeviceConfig customConfig)
    {
        config = ScriptableObject.CreateInstance<DeviceConfig>();
        config.Init(customConfig);
    }

    public DeviceConfig FetchUserConfig() => config;
}
