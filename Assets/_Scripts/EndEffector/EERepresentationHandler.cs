using System;
using UnityEngine;
using UnityEngine.Events;

public class EERepresentationHandler : MonoBehaviour
{
    #region Public Vars

    /// <summary>
    /// Fires event on collision with other physics objects
    /// </summary>
    public UnityAction<bool> OnCollision;

    #endregion

    #region Sfield Vars

    [SerializeField] protected Transform EndEffectorActual;

    #endregion

    #region Protected Vars

    protected int NumberOfCollisions = 0;

    #endregion

    #region Unity Functions

    private void FixedUpdate()
    {
        Vector3 direction = EndEffectorActual.position - transform.position;
        if (Mathf.Approximately(direction.sqrMagnitude, 0f)) return;
        FollowActualEndEffector();
    }

    #endregion

    #region Protected Functions

    protected virtual void FollowActualEndEffector()
    {
        Debug.LogError("Use the 2D or 3D inherited component, not this!!");
    }
    
    protected bool IsTouching()
    {
        return NumberOfCollisions > 0;
    }

    #endregion
}
