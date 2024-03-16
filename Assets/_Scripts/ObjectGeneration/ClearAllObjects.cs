using UnityEngine;

public class ClearAllObjects : MonoBehaviour
{
    [SerializeField] private Transform objectContainer;

    public void ClearObjects()
    {
        for (int i = objectContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(objectContainer.GetChild(i).gameObject);
        }
    }
}
