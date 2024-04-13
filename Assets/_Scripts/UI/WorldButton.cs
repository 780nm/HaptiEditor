using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class WorldButton : MonoBehaviour
{
    public UnityEvent WorldButtonPressed;
    
    [SerializeField] private ButtonHandler buttonHandler;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color activateColor;
    [SerializeField] private float buttonDownAmount = 0.5f;

    private HotSwapColor hotSwap;
    private bool isPressed = false;
    private bool isActivated = false;

    private void OnEnable()
    {
        hotSwap = GetComponent<HotSwapColor>();
        buttonHandler.ButtonPressed.AddListener(ActivateButton);
    }

    private void OnDisable()
    {
        buttonHandler.ButtonPressed.RemoveListener(ActivateButton);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        transform.DOMoveY(transform.position.y - buttonDownAmount, 0.05f).SetEase(Ease.InQuad);
        isPressed = true;
        hotSwap.LerpColor(hoverColor, 0.5f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActivated) return;
        transform.DOMoveY(transform.position.y + buttonDownAmount, 0.05f).SetEase(Ease.OutQuad);
        isPressed = false;
        hotSwap.LerpColor(baseColor, 0.5f);
    }

    private void ActivateButton()
    {
        if (!isPressed || isActivated) return;
        WorldButtonPressed?.Invoke();
        hotSwap.LerpColor(activateColor, 0.5f);
        isActivated = true;
    }
}