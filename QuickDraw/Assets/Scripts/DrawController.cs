using UnityEngine;

public class DrawController : MonoBehaviour
{
    public TrailRenderer drawTrail;
    public ParticleSystem dotStamp;
    public DrawSurface drawSurface;

    public Transform controller; // The real raycast origin

    private Vector3 lastPos;
    private float stillThreshold = 0.001f;

    void Start()
    {
        drawTrail.emitting = false;
        lastPos = transform.position;
    }

    public bool IsDrawing()
    {
        return drawTrail.emitting;
    }

    void Update()
    {
        bool drawHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        RaycastHit hit;
        bool hitPanel = Physics.Raycast(
            controller.position,
            controller.forward,
            out hit,
            0.3f,
            LayerMask.GetMask("DrawSurface")
        );

        // If controller is pointing at panel
        if (hitPanel)
        {
            if (drawHeld)
            {
                // ONLY lock to the panel while drawing
                transform.position = hit.point + hit.normal * 0.001f;

                drawTrail.emitting = true;

                float moved = Vector3.Distance(transform.position, lastPos);
                if (moved < stillThreshold)
                    dotStamp.Play();

                lastPos = transform.position;
            }
            else
            {
                // Not drawing → do NOT stick to panel
                drawTrail.emitting = false;

                // Brush returns to controller naturally
                transform.position = controller.position;
            }
        }
        else
        {
            // Not pointing at panel → brush follows controller normally
            drawTrail.emitting = false;
            transform.position = controller.position;
        }
    }

    public void ClearPanel()
    {
        drawTrail.Clear();
        dotStamp.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
