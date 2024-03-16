using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPlacer3D : MonoBehaviour
{
    [Space]
    [SerializeField] private Transform endEffectorRepresentation;
    [SerializeField] private Transform objectContainer;
    [Space]
    [SerializeField] private GameObject objectToPlace;
    [Range(0.5f, 2.5f)] [SerializeField] private float scaleLowerBound;
    [Range(5f, 2.5f)][SerializeField] private float scaleHigherBound;
    [SerializeField] private float placingTimeInterval = 0.5f;
    
    private bool isPlacing;
    private IEnumerator placerCoroutine;

    private readonly List<GameObject> instantiatedObjects = new List<GameObject>();


    private void Start()
    {
        placerCoroutine = GenerateObjects();
    }

    public void StartObjectPlacement()
    {
        StartCoroutine(placerCoroutine);
    }

    public void SetObjects()
    {
        StopCoroutine(placerCoroutine);
        foreach (GameObject instantiatedObject in instantiatedObjects)
        {
            instantiatedObject.GetComponent<Collider>().enabled = true;
            instantiatedObject.GetComponent<HotSwapColor>().SetValue(2f);
        }
        instantiatedObjects.Clear();
    }
    
    private IEnumerator GenerateObjects()
    {
        while (true)
        {
            GenerateNewObject();
            yield return new WaitForSeconds(placingTimeInterval);
        }
    }

    private void GenerateNewObject()
    {
        Vector3 targetPosition = endEffectorRepresentation.position;
        targetPosition.SetY(objectToPlace.transform.position.y);
        float randomScale = Random.Range(scaleLowerBound, scaleHigherBound);
        Vector3 targetScale = new(randomScale,objectToPlace.transform.localScale.y, randomScale);
        GameObject newObject = Instantiate(objectToPlace, targetPosition, Quaternion.identity, objectContainer);
        newObject.transform.localScale = targetScale;
        instantiatedObjects.Add(newObject);
    }
}
