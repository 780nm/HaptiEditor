using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeCollider : MonoBehaviour
{

    // Start is called before the first frame update
    void Awake()
    {
        Collider objectCollider = GetComponent<TerrainCollider>();
        objectCollider.enabled = false;
        objectCollider.enabled = true; 
    }
}
