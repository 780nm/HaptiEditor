using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrushTypeScript : MonoBehaviour
{

    [SerializeField] private TerrainScript terrainScript;
    [SerializeField] private TMP_Dropdown typeDropdown;
    // Start is called before the first frame update
    void Start()
    {
       typeDropdown.value = terrainScript.GetBrushType();
    }
}
