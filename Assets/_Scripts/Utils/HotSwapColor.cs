using UnityEngine;

public class HotSwapColor : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private MeshRenderer mr;
    
    private MaterialPropertyBlock mpb;
    private static readonly int shaderProp = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock Mpb => mpb ??= new MaterialPropertyBlock();

    private void OnEnable()
    {
        mr = GetComponent<MeshRenderer>();
    }

    private void OnValidate()
    {
        ApplyColor();
    }

    public void SetRandomColor()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        Color newColor = new(r, g, b);
        SetColor(newColor);
    }

    public void SetValue(float factor)
    {
        Color currentColor = Mpb.GetColor(shaderProp);
        Color.RGBToHSV(currentColor, out float hue, out float sat, out float value);
        value *= factor;
        Color targetColor = Color.HSVToRGB(hue, sat, value);
        Mpb.SetColor(shaderProp, targetColor);
        mr.SetPropertyBlock(Mpb);
    }
    
    private void SetColor(Color newColor)
    {
        Mpb.SetColor(shaderProp, newColor);
        mr.SetPropertyBlock(Mpb);
    }

    private void ApplyColor()
    {
        Mpb.SetColor(shaderProp, color);
        mr.SetPropertyBlock(Mpb);
    }
}