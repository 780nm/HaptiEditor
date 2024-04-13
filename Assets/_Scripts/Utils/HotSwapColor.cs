using System.Collections;
using UnityEngine;

public class HotSwapColor : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private MeshRenderer mr;
    
    private Coroutine colorTransitionCoroutine;
    private MaterialPropertyBlock mpb;
    private static readonly int shaderProp = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock Mpb => mpb ??= new MaterialPropertyBlock();

    private void OnEnable()
    {
        mr = GetComponent<MeshRenderer>();
        ApplyColor();
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
    
    public void LerpColor(Color targetColor, float transitionDuration)
    {
        if (colorTransitionCoroutine != null)
            StopCoroutine(colorTransitionCoroutine);
    
        colorTransitionCoroutine = StartCoroutine(ColorTransitionCoroutine(targetColor, transitionDuration));
    }

    private IEnumerator ColorTransitionCoroutine(Color targetColor, float transitionDuration)
    {
        Color startColor = Mpb.GetColor(shaderProp);
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            Color lerpedColor = Color.Lerp(startColor, targetColor, t);
            SetColor(lerpedColor);
            yield return null;
        }

        SetColor(targetColor);
        colorTransitionCoroutine = null;
    }
}