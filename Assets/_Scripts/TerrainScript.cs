using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainScript : MonoBehaviour
{

    [SerializeField] private Terrain terrain;
    [SerializeField] private int terrainLayerIndex; // 0 for dirt, 1 for grass
    [SerializeField] private float brushSize = 5f;
    [SerializeField] private float brushStrength = 1f;
    [SerializeField] private AnimationCurve brushCurve;

    [SerializeField] private int treePrototypeIndex;
    [SerializeField] private float offsetRadius = 0.75f;


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

    void PaintTree(Vector3 worldPosition)
    {

        TerrainData terrainData = terrain.terrainData;
        // Define the radius within which trees will be randomly placed
        float placementRadius = 0f; // Adjust this value as needed

        // Calculate a random offset within the placement radius
        Vector2 randomOffset = Random.insideUnitCircle * placementRadius;

        // Calculate the new position by adding the random offset to the clicked position
        Vector3 newPosition = worldPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);

        // Convert world position to terrain local position
        Vector3 terrainLocalPos = newPosition - terrain.transform.position;

        // Calculate the terrain cell coordinates
        int terrainX = (int)((terrainLocalPos.x / terrainData.size.x) * terrainData.heightmapResolution);
        int terrainZ = (int)((terrainLocalPos.z / terrainData.size.z) * terrainData.heightmapResolution);


        if (terrainX >= 0 && terrainX < terrainData.heightmapResolution &&
            terrainZ >= 0 && terrainZ < terrainData.heightmapResolution)
        {
            // Get the terrain height at the given coordinates
            float terrainHeight = terrainData.GetHeight((int)terrainX, (int)terrainZ);

            // Set the position for placing the tree
            //Vector3 treePosition = new Vector3(terrainLocalPos.x, terrainHeight, terrainLocalPos.z);

            // Create a new TreeInstance
            TreeInstance newTree = new TreeInstance();
            newTree.position = new Vector3(((float)terrainX * terrainData.heightmapScale.x + Random.Range(-offsetRadius, offsetRadius)) / terrainData.size.x,
                                            terrainHeight / terrainData.size.y, 
                                            ((float)terrainZ * terrainData.heightmapScale.z + Random.Range(-offsetRadius, offsetRadius)) / terrainData.size.z
                                          );
            Debug.Log(newTree.position.x);
            newTree.prototypeIndex = treePrototypeIndex;
            newTree.widthScale = 2;
            newTree.heightScale = 2;
            newTree.color = Color.white;
            newTree.lightmapColor = Color.white;

            // Add the TreeInstance to the terrain data
            terrain.AddTreeInstance(newTree);
        }


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
        if (Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                PaintTree(hit.point);
            }
        }
    }
}
