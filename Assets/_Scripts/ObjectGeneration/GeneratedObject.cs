using UnityEngine;

public class GeneratedObject : MonoBehaviour
{
    private HotSwapColor hotSwapColor;

    private void Awake()
    {
        hotSwapColor = GetComponent<HotSwapColor>();
    }

    private void OnEnable()
    {
        hotSwapColor.SetRandomColor();
        hotSwapColor.SetValue(0.5f);
    }
}
