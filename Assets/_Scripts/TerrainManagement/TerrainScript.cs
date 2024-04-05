using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainScript : MonoBehaviour
{

    [SerializeField] private Terrain terrain;
    
    [SerializeField] private float brushRadius = 1f;
    [SerializeField] private AnimationCurve brushCurve;
    [Range(0.1f, 5f)][SerializeField] private float scaleLowerBound;
    [Range(0f, 5f)][SerializeField] private float scaleRange;

    [SerializeField] private int terrainLayerIndex; // 0 for dirt, 1 for grass, 2 sand...
    [SerializeField] private float brushStrength = 1f;
    
    [SerializeField] private int treePrototypeIndex;
    [SerializeField] private int numberOfTreePlaced = 1;

    private int terrainIndex;

    private TerrainCollider terrainCollider;
    private TerrainData terrainData;


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
        terrainData = terrain.terrainData;
        terrainCollider = terrain.GetComponent<TerrainCollider>();

        InitDestructibleTrees();
    }

    void InitDestructibleTrees()
    {
        // create capsule collider for every terrain tree
        for (terrainIndex = 0; terrainIndex < terrain.terrainData.treeInstances.Length; terrainIndex++)
        {
            TreeInstance treeInstance = terrain.terrainData.treeInstances[terrainIndex];

            CreateCapsuleCollider(treeInstance);
        }
    }

    void CreateCapsuleCollider(TreeInstance treeInstance)
    {
        GameObject capsule = new GameObject("EmptyGameObject");
        // Add a capsule collider to the empty GameObject
        CapsuleCollider capsuleCollider = capsule.AddComponent<CapsuleCollider>();
        capsuleCollider.center = new Vector3(0, 5, 0);
        capsuleCollider.height = 10;
        capsuleCollider.radius = 0.5f; // Radius of the capsule


        DestroyableTree tree = capsule.AddComponent<DestroyableTree>();
        tree.terrainIndex = terrainIndex;
        tree.terrain = terrain;
        tree.terrainCollider = terrainCollider;
        capsule.transform.position = Vector3.Scale(treeInstance.position, terrain.terrainData.size) + terrain.transform.position;
        capsule.transform.parent = terrain.transform;
    }

    void PaintTexture(Vector3 worldPosition)
    {
        if (terrain.terrainData.alphamapLayers < terrainLayerIndex)
        {
            Debug.LogError("The terrain layer index is higher than the number of Terrain Texture Layers!");
            return;
        }
        // Convert world position to terrain local position
        Vector3 terrainLocalPos = worldPosition - terrain.transform.position;
        Vector2 normalizedPos = new Vector2(terrainLocalPos.x / terrainData.size.x, terrainLocalPos.z / terrainData.size.z);

        // Get the current alphamap
        int mapX = (int)(normalizedPos.x * terrainData.alphamapWidth);
        int mapY = (int)(normalizedPos.y * terrainData.alphamapHeight);
        
        // Apply the brush effect
        int brush_radius = Mathf.RoundToInt(brushRadius * terrainData.alphamapWidth / terrainData.size.x);
        int startX = Mathf.Max(0, mapX - brush_radius);
        int startY = Mathf.Max(0, mapY - brush_radius);
        int endX = Mathf.Min(terrainData.alphamapWidth, mapX + brush_radius);
        int endY = Mathf.Min(terrainData.alphamapHeight, mapY + brush_radius);

        float[,,] splatmapData = terrainData.GetAlphamaps(startX, startY, brush_radius * 2, brush_radius * 2);

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(mapX, mapY));
                float falloff = Mathf.Clamp01(brushCurve.Evaluate(1 - (distance / brush_radius)));

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
        // Define the radius within which trees will be randomly placed
        float placementRadius = 0f; // Adjust this value as needed

        for (int i = 0;i< numberOfTreePlaced; i++) { 
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

                // Create a new TreeInstance
                TreeInstance newTree = new TreeInstance();
                newTree.position = new Vector3(((float)terrainX * terrainData.heightmapScale.x + Random.Range(-brushRadius, brushRadius)) / terrainData.size.x,
                                                terrainHeight / terrainData.size.y, 
                                                ((float)terrainZ * terrainData.heightmapScale.z + Random.Range(-brushRadius, brushRadius)) / terrainData.size.z
                                              );
                newTree.rotation = Random.Range(0f, Mathf.PI*2);
                newTree.prototypeIndex = treePrototypeIndex;
                float randomScale = Random.Range(scaleLowerBound, scaleLowerBound+scaleRange);
                newTree.widthScale = randomScale;
                newTree.heightScale = randomScale;

                // Add the TreeInstance to the terrain data
                terrain.AddTreeInstance(newTree);

                CreateCapsuleCollider(newTree);
                
                terrainIndex++;
            }
        }
        RefreshTerrainCollider();
    }

    private void RefreshTerrainCollider()
    {
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
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
        DestroyableTree.isInDeletingMode = Input.GetMouseButton(2);
    }

    private void OnApplicationQuit()
    {
        List<TreeInstance> trees = new List<TreeInstance>(terrain.terrainData.treeInstances);
        List<TreeInstance> trees_cleaned = new List<TreeInstance>();
        TreeInstance empty_tree = new TreeInstance();
        for (int i = 0; i < trees.Count; i++)
        {
            if (!trees[i].Equals(empty_tree))
            {
                trees_cleaned.Add(trees[i]);
            }
        }
        terrain.terrainData.SetTreeInstances(trees_cleaned.ToArray(), true);
    }
}


public class DestroyableTree : MonoBehaviour
{
    public int terrainIndex;
    public bool isDestroyed = false;
    public Terrain terrain;
    public TerrainCollider terrainCollider;

    public static bool isInDeletingMode = false;

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.body.tag.Equals("EndEffector") && isInDeletingMode) {
            Delete();
            RefreshTerrainCollider();
        }
    }

    private void RefreshTerrainCollider()
    {
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
    }

    public void Delete()
    {

        List<TreeInstance> trees = new List<TreeInstance>(terrain.terrainData.treeInstances);
        trees[terrainIndex] = new TreeInstance();
        isDestroyed = true;
        terrain.terrainData.treeInstances = trees.ToArray();

        Destroy(gameObject);
    }
}