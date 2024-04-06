using UnityEngine.Events;
using UnityEngine;
using Unity.Burst.CompilerServices;
[RequireComponent(typeof(EndEffectorManager))]
public class TextureSampler : MonoBehaviour
{
    #region Public Vars

    /// <summary>
    /// Fires event on Stylus Button Pressed
    /// </summary>
    public float intensity;

    [SerializeField] private GameObject endEffectorRepresentation;
    [Range(0.0001f, 0.001f), SerializeField] private float movementThreshold;

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
        /*forces = Vector2.zero;
        Transform eeTransform = endEffectorRepresentation.transform;
        if ((eeTransform.position - previousPosition).magnitude < movementThreshold)
        {
            previousPosition = eeTransform.position;
            return;
        }
        Vector3 ray = eeTransform.TransformDirection(Vector3.down);
        bool isSurfacePresent = Physics.Raycast(eeTransform.position, ray, out hit, 1f);
        if (!isSurfacePresent) return;
        MeshRenderer hitRenderer = hit.transform.gameObject.GetComponent<MeshRenderer>();
        if (hitRenderer.material.mainTexture is not Texture2D texture) return;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2 direction = new Vector2(i, j);
                direction.Normalize();
                float mag = texture.GetPixel((int)pixelUV.x + i, (int)pixelUV.y + j).grayscale;
                forces.x += direction.x * (0.5f - mag);
                forces.y += direction.y * (0.5f - mag);
            }
        }
        forces *= intensity;
        previousPosition = eeTransform.position;*/
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
        

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                float grayscale = 
                     terrain.terrainData.alphamapTextures[0].GetPixel((int)pixelUV.x + i, (int)pixelUV.y + j).grayscale;

                Vector2 direction = new Vector2(i, j);
                direction.Normalize();
                forces.x += direction.x * (0.5f - grayscale);
                forces.y += direction.y * (0.5f - grayscale);
            }
        }

        

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
