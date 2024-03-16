using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Zoomer : SingletonMonoBehavior<Zoomer>
{
    public UnityAction<bool> OnZoom;
    
    [SerializeField] private Transform endEffector;
    [Range(1, 100)] [SerializeField] private float eeScaleFactor;
    [SerializeField] private Transform cam;
    [Range(1, 100)] [SerializeField] private float cameraMoveFactor;
    
    [Space]
    [Tooltip("Leave at 0 to keep the end effector touching the terrain")]
    [SerializeField] private KeyCode zoomInKey;
    [SerializeField] private KeyCode zoomOutKey;

    private int zoomDirection = 1;
    private bool isZooming;
    private Coroutine zoomCoroutine;
    

    private void OnEnable()
    {
        InputHandler.Instance.OnKeyDownEvent += OnKeyPress;
        InputHandler.Instance.OnKeyUpEvent += OnKeyRelease;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnKeyDownEvent -= OnKeyPress;
        InputHandler.Instance.OnKeyUpEvent -= OnKeyRelease;
    }

    private void OnKeyPress(KeyCode key)
    {
        if (isZooming) return;
        if (key == zoomInKey)
        {
            zoomDirection = 1;
        }
        else if (key == zoomOutKey)
        {
            zoomDirection = -1;
        }
        else return;
        zoomCoroutine = StartCoroutine(ZoomCoroutine());
        OnZoom?.Invoke(true);
        isZooming = true;
    }

    private void OnKeyRelease(KeyCode key)
    {
        if (!isZooming) return;
        StopCoroutine(zoomCoroutine);
        OnZoom?.Invoke(false);
        isZooming = false;
    }

    private IEnumerator ZoomCoroutine()
    {
        while (true)
        {
            Zoom();
            yield return null;
        }
    }

    private void Zoom()
    {
        SetEndEffectorScale();
        MoveCamera();
    }

    private void SetEndEffectorScale()
    {
        Vector3 localScale = endEffector.localScale;
        float zoomFactor = zoomDirection * eeScaleFactor * 0.0001f;
        localScale = new Vector3(
            localScale.x + zoomFactor, 
            localScale.y + zoomFactor, 
            localScale.z + zoomFactor);
        endEffector.localScale = localScale;
    }

    private void MoveCamera()
    {
        cam.Translate(Vector3.forward * (Mathf.Pow(cameraMoveFactor, 3) * 0.0001f * -zoomDirection));
    }
}
