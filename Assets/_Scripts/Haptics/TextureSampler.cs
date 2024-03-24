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

    #endregion

    #region Member Vars

    private EndEffectorManager endEffectorManager;
    private GameObject hitObject;
    private Texture hitTexture;
    RaycastHit hit;
    private Vector2 forces;

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
        var ray = endEffectorRepresentation.transform.TransformDirection(Vector3.down);
        Physics.Raycast(endEffectorRepresentation.transform.position, ray, out hit, 1f);
        Debug.DrawLine(endEffectorRepresentation.transform.position, hit.point, Color.red);
        hitObject = hit.transform.gameObject;
        MeshRenderer hitRenderer = hitObject.GetComponent<MeshRenderer>();
        Texture2D tex = hitRenderer.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;

        print(hit.textureCoord);
        for(int i = -1; i <=1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2 direction = new Vector2(i, j);
                direction.Normalize();
                float mag = tex.GetPixel((int)pixelUV.x + i, (int)pixelUV.y + j).grayscale;
                forces.x += direction.x * (0.5f - mag);
                forces.y += direction.y * (0.5f - mag);
            }
        }

        forces *= intensity;
        print(forces);

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
