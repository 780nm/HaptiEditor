using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TerrainPainter : MonoBehaviour
{

    [SerializeField] private Terrain terrain;
    
    [SerializeField] private float brushRadius = 1f;
    [SerializeField] private AnimationCurve brushCurve;
    [Range(0.1f, 5f)][SerializeField] private float scaleLowerBound;
    [Range(0f, 5f)][SerializeField] private float scaleRange;

    [SerializeField] private TextureTypes terrainLayerIndex; // 0 for dirt, 1 for grass, 2 sand...
    [SerializeField] private float brushStrength = 1f;
    [SerializeField] private float texturePaintingTimeInterval = 0.05f;

    [SerializeField] private ObjectTypes treePrototypeIndex;
    [SerializeField] private float objectPaintingTimeInterval = 0.5f;
    [SerializeField] private int numberOfTreePlaced = 1;

    [SerializeField] private Transform EndEffectorTransform;


    [SerializeField] private BrushType brushType;

    private int terrainIndex;

    private TerrainCollider terrainCollider;
    private TerrainData terrainData;

    private IEnumerator objectPainterCoroutine;
    private IEnumerator texturePainterCoroutine;

    #region Unity Functions
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

        objectPainterCoroutine = PaintingObject();
        texturePainterCoroutine = PaintingTexture();
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
    #endregion

    #region Texture Painting
    IEnumerator PaintingTexture()
    {
        while (true)
        {
            PaintTexture();
            yield return new WaitForSeconds(texturePaintingTimeInterval);
        }
    }
    void PaintTexture()
    {
        if (terrain.terrainData.alphamapLayers < (int)terrainLayerIndex)
        {
            Debug.LogError("The terrain layer index is higher than the number of Terrain Texture Layers!");
            return;
        }
        // Convert world position to terrain local position
        Vector3 terrainLocalPos = EndEffectorTransform.position - terrain.transform.position;
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
                        (i == (int)terrainLayerIndex) ? 1.0f : 0.0f, brushStrength * falloff);
                }
            }
        }

        // Apply the new alphamap
        terrainData.SetAlphamaps(startX, startY, splatmapData);
    }

    #endregion

    #region Object Painting
    IEnumerator PaintingObject()
    {
        while (true)
        {
            PaintObject();
            yield return new WaitForSeconds(objectPaintingTimeInterval);
        }
    }
    void PaintObject()
    {
        // Define the radius within which trees will be randomly placed
        float placementRadius = 0f; // Adjust this value as needed

        for (int i = 0;i< numberOfTreePlaced; i++) { 
            // Calculate a random offset within the placement radius
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * placementRadius;

            // Calculate the new position by adding the random offset to the clicked position
            Vector3 newPosition = EndEffectorTransform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

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
                newTree.position = new Vector3(((float)terrainX * terrainData.heightmapScale.x + UnityEngine.Random.Range(-brushRadius, brushRadius)) / terrainData.size.x,
                                                terrainHeight / terrainData.size.y, 
                                                ((float)terrainZ * terrainData.heightmapScale.z + UnityEngine.Random.Range(-brushRadius, brushRadius)) / terrainData.size.z
                                              );
                newTree.rotation = UnityEngine.Random.Range(0f, Mathf.PI*2);
                newTree.prototypeIndex = (int)treePrototypeIndex;
                float randomScale = UnityEngine.Random.Range(scaleLowerBound, scaleLowerBound+scaleRange);
                newTree.widthScale = randomScale;
                newTree.heightScale = randomScale;

                // Add the TreeInstance to the terrain data
                terrain.AddTreeInstance(newTree);

                CreateCapsuleCollider(newTree);
                
                terrainIndex++;
            }
        }
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

    #endregion

    #region Stylus Actions And Coroutines
    public void StartPainting()
    {
        switch (brushType)
        {
            case BrushType.Texture:
                StartCoroutine(texturePainterCoroutine);
                break;
            case BrushType.Objects:
                StartCoroutine(objectPainterCoroutine);
                break;
            case BrushType.ObjectEraser:
                DestroyableTree.isInDeletingMode = true;
                break;
            default:
                Debug.LogError("TerrainScript Error: BrushType not supported/implemented");
                break;
        }
    }

    public void StopPainting()
    {
        switch (brushType)
        {
            case BrushType.Texture:
                StopCoroutine(texturePainterCoroutine);
                break;
            case BrushType.Objects:
                StopCoroutine(objectPainterCoroutine);
                RefreshTerrainCollider();
                break;
            case BrushType.ObjectEraser:
                DestroyableTree.isInDeletingMode = false;
                break;
            default:
                Debug.LogError("TerrainScript Error: BrushType not supported/implemented");
                break;
        }
    }

    #endregion

    #region Dropdown Functions

    public void SetBrushType(TMP_Dropdown dropDown)
    {
        brushType = (BrushType)dropDown.value;
    }

    public void SetBrushSpecifics(TMP_Dropdown dropDown)
    {
        Debug.Log("SET");
        switch (brushType)
        {
            case (BrushType.Texture):
                terrainLayerIndex = (TextureTypes)dropDown.value;
                break;
            case (BrushType.Objects):
                treePrototypeIndex = (ObjectTypes)dropDown.value;
                break;
            default :
                Debug.LogError("TerrainScript Error: This brushtype doesn't have anything to update");
                break;
        }
    }

    public int GetBrushType()
    {
        return (int)brushType;
    }

    public int GetBrushSpecifics(BrushType brush)
    {
        switch (brush)
        {
            case (BrushType.Texture):
                return (int)terrainLayerIndex;
            case (BrushType.Objects):
                return (int)treePrototypeIndex;
            default:
                return 0;
        }
    }


    #endregion

    #region Utility 
    private void RefreshTerrainCollider()
    {
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
    }

    #endregion
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

public enum ObjectTypes
{
    Trees = 0,
    DeadTrees = 1,
    Rocks = 2
}

public enum TextureTypes
{
    Dirt = 0,
    Grass = 1,
    Stone = 2
}


public enum BrushType
{
    Texture = 0,
    Objects = 1, 
    ObjectEraser = 2
}