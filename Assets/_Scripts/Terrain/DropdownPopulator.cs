﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownPopulator : MonoBehaviour
{
    [SerializeField] private BrushType brushType;
    [SerializeField] private TMP_Dropdown dropdown;

    private void OnValidate()
    {
        PopulateFields();
    }

    public void SetType(int val)
    {
        brushType = val == 0 ? BrushType.Texture : BrushType.Objects;
        PopulateFields();
    }

    private void PopulateFields()
    {
        dropdown.ClearOptions();
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
                break;
        }
        dropdown.AddOptions(new List<string>(enumStrings));
    }

}