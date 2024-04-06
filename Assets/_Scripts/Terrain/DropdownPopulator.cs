using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownPopulator : MonoBehaviour
{
    [SerializeField] private BrushType brushType;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TerrainPainter terrainPainter;

    private void OnValidate()
    {
        PopulateFields();
    }

    public void SetType(int val)
    {
        brushType = (BrushType)val;
        PopulateFields();
    }

    private void PopulateFields()
    {
        dropdown.ClearOptions();
        gameObject.SetActive(true);
        string[] enumStrings = Array.Empty<string>();
        switch (brushType)
        {
            case BrushType.Texture:
                {
                    TextureTypes[] enumNames = (TextureTypes[])Enum.GetValues(typeof(TextureTypes));
                    enumStrings = new string[enumNames.Length];
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        enumStrings[i] = enumNames[i].ToString();
                    }
                    break;
                }
            case BrushType.Objects:
                {
                    ObjectTypes[] enumNames = (ObjectTypes[])Enum.GetValues(typeof(ObjectTypes));
                    enumStrings = new string[enumNames.Length];
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        enumStrings[i] = enumNames[i].ToString();
                    }  
                    break;
                }
            case BrushType.ObjectEraser:
                { 
                    gameObject.SetActive(false);
                    break;
                }
            default:
                Debug.LogError("DropdownPopulator Error: BrushType not handled");
                break;
        }
        dropdown.AddOptions(new List<string>(enumStrings));
        dropdown.SetValueWithoutNotify(terrainPainter.GetBrushSpecifics(brushType));
    }

}
