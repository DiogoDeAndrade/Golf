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

    public void Start()
    {
        lineRenderer.enabled = false;
        lineRenderer.material = material = new Material(lineRenderer.material);

        rb = GetComponent<BallPhysics>();
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

        float   t = m / maxDistance; t = t * t;
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
        float m = Mathf.Clamp(toHitPos.magnitude, 0, maxDistance);
        float t = m / maxDistance; t = t * t;

        float speed = shootMaxPower * t;

        rb.linearVelocity = toHitPos.normalized * speed;
    }
}
