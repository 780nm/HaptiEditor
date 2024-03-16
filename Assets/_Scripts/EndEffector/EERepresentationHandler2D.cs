using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EERepresentationHandler2D : EERepresentationHandler
{
    #region Member Vars

    private Rigidbody2D rb2D;
    private Collider2D col2D;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D()
    {
        NumberOfCollisions++;
        OnCollision?.Invoke(IsTouching());
    }
    
    private void OnCollisionExit2D()
    {
        NumberOfCollisions--;
        OnCollision?.Invoke(IsTouching());
    }
    
    private void OnEnable()
    {
        if (Zoomer.Instance != null) Zoomer.Instance.OnZoom += ToggleCollider;
    }

    private void OnDisable()
    {
        if (Zoomer.Instance != null) Zoomer.Instance.OnZoom -= ToggleCollider;
    }

    #endregion

    #region Protected Functions

    protected override void FollowActualEndEffector()
    {
        Vector3 direction = EndEffectorActual.position - transform.position;
        Vector3 velocity = (direction / Time.fixedDeltaTime);
        rb2D.velocity = velocity;
    }
    
    private void ToggleCollider(bool state)
    {
        col2D.enabled = !state;
    }

    #endregion
}
