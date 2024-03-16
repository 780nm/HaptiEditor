using UnityEngine;

[RequireComponent(typeof(EndEffectorManager))]
public class ForceFeedbackHandler : MonoBehaviour
{
    protected bool IsTouchingSurface;

    #region Sfield Vars

    [SerializeField] private GameObject endEffectorRepresentation;
    [SerializeField] private GameObject endEffectorActual;
    [SerializeField] private float proportionalGain;
    [SerializeField] private float derivativeGain;
    [SerializeField] private float derivativeSmoothing;

    #endregion

    #region Member Vars

    private EndEffectorManager endEffectorManager;
    private Vector3 representationToActual = Vector3.zero;
    private float distX;
    private float distY;
    private float buffX;
    private float buffY;
    private float diffX;
    private float diffY;
    private float oldX;
    private float oldY;
    private float xForce;
    private float yForce;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        endEffectorManager = GetComponent<EndEffectorManager>();
    }

    private void OnEnable()
    {
        endEffectorRepresentation.GetComponent<EERepresentationHandler>().OnCollision += SetCollisionState;
    }

    private void OnDisable()
    {
        endEffectorRepresentation.GetComponent<EERepresentationHandler>().OnCollision -= SetCollisionState;
    }
    
    private void LateUpdate()
    {
        if (!IsTouchingSurface)
        {
            FlushForces();
            return;
        }
        CalculateForces();
    }

    private void OnDestroy() => FlushForces();

    private void OnApplicationQuit() => FlushForces();

    #endregion

    #region Protected Functions

    protected void CalculateForces(bool is3D = false)
    {
        representationToActual = endEffectorActual.transform.position - endEffectorRepresentation.transform.position;
        distX = representationToActual.x;
        distY = is3D? representationToActual.z : representationToActual.y;
        buffX = (distX - oldX)/Time.deltaTime;
        buffY = (distY - oldY)/Time.deltaTime;
        diffX = derivativeSmoothing * diffX + (1 - derivativeSmoothing) * buffX;
        diffY = derivativeSmoothing * diffY + (1 - derivativeSmoothing) * buffY;
        xForce = proportionalGain * distX + derivativeGain * diffX;
        yForce = proportionalGain * distY + derivativeGain * diffY;
        endEffectorManager.SetForces(xForce, yForce);
        oldX = distX;
        oldY = distY;
    }
    
    protected void FlushForces()
    {
        endEffectorManager.SetForces(0, 0);
    }

    #endregion

    #region Private Functions

    private void SetCollisionState(bool state)
    {
        IsTouchingSurface = state;
    }

    #endregion
}
