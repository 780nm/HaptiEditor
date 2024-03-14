using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainScript : MonoBehaviour
{

    [SerializeField] private Terrain terrain;
    [SerializeField] private int terrainLayerIndex; // 0 for dirt, 1 for grass
    [SerializeField] private float brushSize = 5f;
    [SerializeField] private float brushStrength = 1f;
    [SerializeField] private AnimationCurve brushCurve;


    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain reference is not set!");
            return;
        }
        if (terrain.terrainData.alphamapLayers == 0)
        {
            Debug.LogError("No Terrain Layer set!");
            return;
        }
    }

    void PaintTexture(Vector3 worldPosition)
    {
        if (terrain.terrainData.alphamapLayers < terrainLayerIndex)
        {
            Debug.LogError("The terrain layer index is higher than the number of Terrain Texture Layers!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // Convert world position to terrain local position
        Vector3 terrainLocalPos = worldPosition - terrain.transform.position;
        Vector2 normalizedPos = new Vector2(terrainLocalPos.x / terrainData.size.x, terrainLocalPos.z / terrainData.size.z);

        // Get the current alphamap
        int mapX = (int)(normalizedPos.x * terrainData.alphamapWidth);
        int mapY = (int)(normalizedPos.y * terrainData.alphamapHeight);
        

        // Apply the brush effect
        int brushRadius = Mathf.RoundToInt(brushSize * terrainData.alphamapWidth / terrainData.size.x);
        int startX = Mathf.Max(0, mapX - brushRadius);
        int startY = Mathf.Max(0, mapY - brushRadius);
        int endX = Mathf.Min(terrainData.alphamapWidth, mapX + brushRadius);
        int endY = Mathf.Min(terrainData.alphamapHeight, mapY + brushRadius);

        float[,,] splatmapData = terrainData.GetAlphamaps(startX, startY, brushRadius*2, brushRadius*2);

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(mapX, mapY));
                float falloff = Mathf.Clamp01(brushCurve.Evaluate(1 - (distance / brushRadius)));

                // For each texture layer of the terrain
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    // Linear Interpolation between the current value of the spot 1 (if the texture == i) or 0 (else)
                    // with t = brushStrength * falloff
                    splatmapData[y - startY, x - startX, i] = Mathf.Lerp(splatmapData[y - startY, x - startX, i],
                        (i == terrainLayerIndex) ? 1.0f : 0.0f, brushStrength * falloff);
                }
            }
        }

        // Apply the new alphamap
        terrainData.SetAlphamaps(startX, startY, splatmapData);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                PaintTexture(hit.point);
            }
        }
    }
}
