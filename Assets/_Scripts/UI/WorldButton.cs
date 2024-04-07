using System;
using UnityEngine;
using DG.Tweening;

public class WorldButton : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private ButtonHandler buttonHandler;
    
    [SerializeField] private float buttonDownAmount = 0.5f;

    private HotSwapColor hotSwap;
    private bool isPressed = false;

    private void OnEnable()
    {
        hotSwap = GetComponent<HotSwapColor>();
        hotSwap.SetValue(0.5f);
        buttonHandler.ButtonPressed.AddListener(ActivateButton);
    }

    private void OnDisable()
    {
        buttonHandler.ButtonPressed.RemoveListener(ActivateButton);
    }

    private void OnTriggerEnter(Collider other)
    {
        transform.DOMoveY(transform.position.y - buttonDownAmount, 0.05f).SetEase(Ease.InQuad);
        isPressed = true;
        hotSwap.SetValue(2f);
    }

    private void OnTriggerExit(Collider other)
    {
        transform.DOMoveY(transform.position.y + buttonDownAmount, 0.05f).SetEase(Ease.OutQuad);
        isPressed = false;
        hotSwap.SetValue(0.5f);
    }

    private void ActivateButton()
    {
        if (!isPressed) return;
        sceneManager.LoadNextScene();
        hotSwap.SetValue(2f);
    }
}