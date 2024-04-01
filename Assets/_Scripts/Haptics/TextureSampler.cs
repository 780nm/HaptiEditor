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
