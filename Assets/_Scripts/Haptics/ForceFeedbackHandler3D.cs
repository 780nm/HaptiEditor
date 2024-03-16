public class ForceFeedbackHandler3D : ForceFeedbackHandler
{
    private void LateUpdate()
    {
        if (!IsTouchingSurface)
        {
            FlushForces();
            return;
        }
        CalculateForces(is3D:true);
    }
}
