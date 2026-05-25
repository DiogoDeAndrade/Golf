using System;
using UC;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float           shootMaxPower = 5.0f;
    [SerializeField] 
    private float           minDistance = 0.1f;
    [SerializeField] 
    private float           maxDistance = 4.0f;
    [SerializeField] 
    private LineRenderer    lineRenderer;
    [SerializeField, GradientUsage(true)]
    private Gradient        lineGradient;

    Vector3     hitPos;
    Material    material;
    BallPhysics rb;

    public bool isMoving => rb.isMoving;
    public float velocity => rb.linearVelocity.magnitude;
    public float potentialVelocity
    {
        get
        {
            if (lineRenderer.enabled)
            {
                Vector3 toHitPos = hitPos - transform.position;
                float m = Mathf.Clamp(toHitPos.magnitude, 0, maxDistance);

                if (m >= minDistance)
                {
                    float t = ComputePowerCurve(m / maxDistance);

                    return shootMaxPower * t;
                }
            }

            return 0.0f;
        }
    }

    public void Start()
    {
        lineRenderer.enabled = false;
        lineRenderer.material = material = new Material(lineRenderer.material);

        rb = GetComponent<BallPhysics>();
    }

    private float ComputePowerCurve(float t)
    {
        return t * t;
    }

    public void Hold(Vector3 position)
    {
        hitPos = position;

        Vector3 toHitPos = hitPos - transform.position;
        float   m = Mathf.Clamp(toHitPos.magnitude, 0, maxDistance);

        if (m < minDistance)
        {
            lineRenderer.enabled = false;
            return;
        }

        hitPos = transform.position + toHitPos.normalized * m;

        float   t = ComputePowerCurve(m / maxDistance); 
        var     color = lineGradient.Evaluate(t);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, hitPos);

        lineRenderer.material.SetColor("_EmissionColor", color);
    }

    public bool Release()
    {
        bool isActive = lineRenderer.enabled;
        if (isActive)
        {
            Shoot();
        }
        lineRenderer.enabled = false;

        return isActive;
    }

    void Shoot()
    {
        Vector3 toHitPos = transform.position - hitPos;
        rb.linearVelocity = toHitPos.normalized * potentialVelocity;
    }
}
