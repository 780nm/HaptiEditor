using Haply.hAPI;
using UnityEngine;

public class Pantograph : Mechanism
{
    private const float GAIN = 0.1f;

    private float length1 = 0.07f;
    private float length2 = 0.09f;
    private float distance = 0.038f;

    private float theta1, theta2;
    private float omega1, omega2;
    private float tau1, tau2;
    private float fX, fy;
    private float xE, yE;
    private float vxE, vyE;

    private float jT11, jT12, jT21, jT22;

    public override void SetBoardType(BoardTypes boardType)
    {
        distance = boardType == BoardTypes.Gen2 ? 0.0f : 0.038f;
    }

    public override void ForwardKinematics ( float[] angles )
    {
        float l1 = length1;
        float l2 = length1;
        float ll1 = length2;
        float ll2 = length2;
        float d = distance;

        theta1 = Mathf.PI / 180f * angles[0];
        theta2 = Mathf.PI / 180f * angles[1];

        // Forward Kinematics
        float c1 = Mathf.Cos( theta1 );
        float c2 = Mathf.Cos( theta2 );
        float s1 = Mathf.Sin( theta1 );
        float s2 = Mathf.Sin( theta2 );
        float xA = l1 * c1 - d / 2f;
        float yA = l1 * s1;
        float xB = l2 * c2 + d / 2f;
        float yB = l2 * s2;

        float hx = xB - xA;
        float hy = yB - yA;
        float hh = Mathf.Pow( hx, 2 ) + Mathf.Pow( hy, 2 );
        float hm = Mathf.Sqrt( hh );
        float cB = -(Mathf.Pow( ll2, 2 ) - Mathf.Pow( ll1, 2 ) - hh) / (2 * ll1 * hm);

        float h1X = ll1 * cB * hx / hm;
        float h1Y = ll1 * cB * hy / hm;
        float h1H1 = Mathf.Pow( h1X, 2 ) + Mathf.Pow( h1Y, 2 );
        float h1M = Mathf.Sqrt( h1H1 );
        float sB = Mathf.Sqrt( 1 - Mathf.Pow( cB, 2 ) );

        float lx = -ll1 * sB * h1Y / h1M;
        float ly = ll1 * sB * h1X / h1M;

        float xP = xA + h1X + lx;
        float yP = yA + h1Y + ly;

        float phi1 = Mathf.Acos( (xP + d / 2f - l1 * c1) / ll1 );
        float phi2 = Mathf.Acos( (xP - d / 2f - l2 * c2) / ll2 );

        float c11 = Mathf.Cos( phi1 );
        float s11 = Mathf.Sin( phi1 );
        float c22 = Mathf.Cos( phi2 );
        float s22 = Mathf.Sin( phi2 );

        float dn = ll1 * (c11 * s22 - c22 * s11);
        float eta = (-ll1 * c11 * s22 + ll1 * c22 * s11 - c1 * l1 * s22 + c22 * l1 * s1) / dn;
        float nu = l2 * (c2 * s22 - c22 * s2) / dn;

        jT11 = -ll1 * eta * s11 - ll1 * s11 - l1 * s1;
        jT12 = ll1 * c11 * eta + ll1 * c11 + c1 * l1;
        jT21 = -ll1 * s11 * nu;
        jT22 = ll1 * c11 * nu;

        xE = xP;
        yE = yP;
    }

    public override void VelocityCalculation ( float[] angularVelocities )
    {
        omega1 = Mathf.PI / 180 * angularVelocities[0];
        omega2 = Mathf.PI / 180 * angularVelocities[1];

        vxE = jT11 * omega1 + jT12 * omega2;
        vyE = jT21 * omega1 + jT22 * omega2;
    }

    public override void TorqueCalculation ( float[] force )
    {
        fX = force[0];
        fy = force[1];

        tau1 = jT11 * fX + jT12 * fy;
        tau2 = jT21 * fX + jT22 * fy;

        tau1 *= GAIN;
        tau2 *= GAIN;
    }

    public override void ForceCalculation () { }

    public override void PositionControl ( float[] position ) { }

    public override void InverseKinematics () { }

    public override void SetMechanismParameters ( float[] parameters )
    {
        length1 = parameters[0];
        length2 = parameters[1];
        distance = parameters[2];
    }

    public override void SetSensorData ( float[] data ) { }

    public override float[] GetCoordinate ( float[] buffer )
    {
        buffer[0] = xE;
        buffer[1] = yE;
        return buffer;
    }

    public override float[] GetTorque ( float[] buffer )
    {
        buffer[0] = tau1;
        buffer[1] = tau2;
        return buffer;
    }

    public override float[] GetAngle ( float[] buffer )
    {
        buffer[0] = theta1;
        buffer[1] = theta2;
        return buffer;
    }

    public override float[] GetVelocity ( float[] buffer )
    {
        buffer[0] = vxE;
        buffer[1] = vyE;
        return buffer;
    }

    public override float[] GetAngularVelocity ( float[] buffer )
    {
        buffer[0] = omega1;
        buffer[1] = omega2;
        return buffer;
    }
}

