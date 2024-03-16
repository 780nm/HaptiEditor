using UnityEngine;
using DG.Tweening;

public class SlidePanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelToSlide;
    [SerializeField] private Vector2 slideDirection;
    [SerializeField] private float slideDistance;
    [SerializeField] private float slideDuration;

    private bool isPanelVisible;
    private Vector2 initialPosition;

    private void Start()
    {
        initialPosition = panelToSlide.anchoredPosition;
    }

    public void TogglePanel()
    {
        if (!isPanelVisible)
        {
            panelToSlide.DOAnchorPos(initialPosition - slideDirection * slideDistance, slideDuration);
        }
        else
        {
            panelToSlide.DOAnchorPos(initialPosition, slideDuration);
        }
        isPanelVisible = !isPanelVisible;
    }
}