using System.Collections;
using UnityEngine;

public class EESprinkling : MonoBehaviour
{
    [Space]
    [SerializeField] private Transform placingPosition;
    [SerializeField] private Transform objectContainer;
    
    [Space]
    [SerializeField] private GameObject prefabCircle;
    [SerializeField] private float placingTimeInterval = 0.5f;
    [SerializeField] private bool isColliderEnabledByDefault;
    [SerializeField] private Color placingColor = Color.gray;

    private bool isPlacing;
    private float lastPlacedTime;

    private ArrayList freshlyPlaced;

    void Start()
    {
        freshlyPlaced = new ArrayList();
        lastPlacedTime = Time.time;
    }

    void Update()
    {
        if ( isPlacing || Input.GetKey(KeyCode.Space)) {
            if (Time.time - lastPlacedTime > placingTimeInterval)
            {
                lastPlacedTime = Time.time;

                // Creating a circle
                GameObject circle = Instantiate(prefabCircle, placingPosition.position, Quaternion.identity, objectContainer);
                circle.transform.localScale = Vector3.one * Random.Range(0.1f, 2f);
                circle.GetComponent<CircleCollider2D>().enabled = isColliderEnabledByDefault;
                circle.GetComponent<SpriteRenderer>().color = placingColor;

                // Adding the circle to the list of recently Placed circle
                freshlyPlaced.Add(circle);
            }
        }

        else if ( !isPlacing && !Input.GetKey(KeyCode.Space)) {
            if ( freshlyPlaced.Count > 0)
            {
                foreach (GameObject circle in freshlyPlaced)
                {
                    circle.GetComponent<CircleCollider2D>().enabled = true;
                    circle.GetComponent<SpriteRenderer>().color = Color.black;
                }
                freshlyPlaced.Clear();
            }
        }
        
    }

    public void PlaceCircles(bool state) => isPlacing = state;

}
