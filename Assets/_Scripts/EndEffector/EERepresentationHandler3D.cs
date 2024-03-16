﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class EERepresentationHandler3D : EERepresentationHandler 
{
    #region Member Vars

    private Rigidbody rb;
    private Collider col;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnCollisionEnter() 
    {
        NumberOfCollisions++;
        OnCollision?.Invoke(IsTouching());
    }

    private void OnCollisionExit() 
    {
        NumberOfCollisions--;
        OnCollision?.Invoke(IsTouching());
    }

    #endregion

    #region Protected Functions

    protected override void FollowActualEndEffector()
    {
        Vector3 direction = EndEffectorActual.position - transform.position;
        Vector3 velocity = direction / Time.fixedDeltaTime;
        rb.velocity = velocity;
    }

    protected override void ToggleCollider(bool state)
    {
        col.enabled = state;
    }

    #endregion
    
}
