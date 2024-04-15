using UnityEngine.Events;
using UnityEngine;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEngine.TerrainUtils;
using System;
[RequireComponent(typeof(EndEffectorManager))]
public class TextureSampler : MonoBehaviour
{
    #region Public Vars

    /// <summary>
    /// Fires event on Stylus Button Pressed
    /// </summary>
    public float intensity;
    public float swatchscale;

    [SerializeField] private GameObject endEffectorRepresentation;
    [Range(0.0001f, 0.01f), SerializeField] private float movementThreshold;

    public Terrain terrain;
    
    #endregion

    #region Member Vars

    private EndEffectorManager endEffectorManager;
    private Texture hitTexture;
    RaycastHit hit;
    private Vector2 forces;
    private Vector3 previousPosition = Vector3.zero;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        endEffectorManager = GetComponent<EndEffectorManager>();
    }

    private void Start()
    {
        forces = new Vector2(0, 0);
    }

    private void OnEnable()
    {
        endEffectorManager.OnSimulationStep += AddTextureForce;
    }

    private void OnDisable()
    {
        endEffectorManager.OnSimulationStep -= AddTextureForce;
    }

    private void LateUpdate()
    {
        forces = Vector2.zero;
        Transform eeTransform = endEffectorRepresentation.transform;
        if ((eeTransform.position - previousPosition).magnitude < movementThreshold)
        {
            previousPosition = eeTransform.position;
            return;
        }

        Vector3 terrainLocalPos = endEffectorRepresentation.transform.position - terrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            Mathf.InverseLerp(0, terrain.terrainData.size.x, terrainLocalPos.x),
            0,
            Mathf.InverseLerp(0, terrain.terrainData.size.z, terrainLocalPos.z)
        );


        Vector2 pixelUV = new Vector2(
            normalizedPos.x * terrain.terrainData.alphamapWidth,
            normalizedPos.z * terrain.terrainData.alphamapHeight
        );

        float[,,] swatch = terrain.terrainData.GetAlphamaps((int)pixelUV.x, (int)pixelUV.y, 1,1);
        int maxTex = 0;
        if (swatch[0, 0, 0] < swatch[0, 0, 1]) maxTex = 1;
        if (swatch[0, 0, maxTex] < swatch[0, 0, 2]) maxTex = 2;

        SplatPrototype prot = terrain.terrainData.splatPrototypes[maxTex];

        Vector2 normalUV = new Vector2(
            pixelUV.x * swatchscale % prot.normalMap.width,
            pixelUV.y * swatchscale % prot.normalMap.height
        );


        Color normalPixel = prot.normalMap.GetPixel((int)normalUV.x, (int)normalUV.y);

        forces.x += normalPixel.r - 0.5f;
        forces.y += normalPixel.g - 0.5f;

        forces *= intensity;
        previousPosition = eeTransform.position;
    }

    #endregion

    #region Private Vars

    private void AddTextureForce(float[] sensorData)
    {
        float[] EEforces = endEffectorManager.GetForces();
        endEffectorManager.SetForces(EEforces[0] + forces.x, EEforces[1] + forces.y);
    }

    #endregion
}
