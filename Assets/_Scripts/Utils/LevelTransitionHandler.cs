using UnityEngine;
using DG.Tweening;

public class LevelTransitionHandler : MonoBehaviour
{
    [SerializeField] private GameObject cover;
    [SerializeField] private float loadTime = 1.2f;

    public float LoadTime => loadTime;

    private Vector3 originalScale;

    private void Start()
    {
        cover.SetActive(true);
        StartLevel();
    }

    public void StartLevel()
    {
        originalScale = cover.transform.localScale;
        cover.transform.DOScale(Vector3.zero, loadTime).SetEase(Ease.InOutExpo);
    }

    public void EndLevel()
    {
        cover.transform.DOScale(originalScale, loadTime).SetEase(Ease.InOutExpo);
    }
}