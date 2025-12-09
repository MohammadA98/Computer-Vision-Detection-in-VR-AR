using UnityEngine;

public class DrawSurface : MonoBehaviour
{
    public Transform brushTip;  // assign BrushTip in inspector
    public float drawDistance = 0.02f;

    private Plane drawPlane;

    void Start()
    {
        // Plane facing forward from the panel
        drawPlane = new Plane(transform.forward, transform.position);
    }

    public bool IsBrushCloseEnough()
    {
        float distance = Mathf.Abs(drawPlane.GetDistanceToPoint(brushTip.position));
        return distance < drawDistance;
    }
}
