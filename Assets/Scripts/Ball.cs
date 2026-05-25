using System;
using UC;
using UC.RPG;
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
    [SerializeField]
    private float           invulnerabilityDuration = 2.0f;
    [SerializeField]
    private float           blinkDuration = 0.2f;
    [SerializeField]
    private GameObject      bloodFX;

    Vector3     hitPos;
    Material    material;
    BallPhysics rb;
    float       invulnerabilityTimer;
    float       blinkTimer;
    bool        setVisible = false;

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

        var healthResource = this.FindResourceHandler(Globals.healthResource);
        healthResource.onChange += HealthResource_onChange;
        healthResource.canChange += HealthResource_canChange;
        healthResource.onResourceEmpty += HealthResource_onResourceEmpty;
    }

    private void HealthResource_onResourceEmpty(ResourceInstance resourceInstance, GameObject changeSource)
    {
        LevelManager.instance.GameOver();
    }

    private bool HealthResource_canChange(ResourceInstance resource, ChangeData data)
    {
        if (data.deltaValue >= 0.0f)
            return true;

        return invulnerabilityTimer <= 0.0f;
    }

    private void HealthResource_onChange(ResourceInstance resourceInstance, ChangeData changeData)
    {
        if (changeData.deltaValue >= 0.0f)
            return;

        invulnerabilityTimer = invulnerabilityDuration;
        blinkTimer = blinkDuration;
        setVisible = true;

        if (bloodFX)
        {
            Instantiate(bloodFX, changeData.changeSrcPosition, Quaternion.LookRotation(changeData.changeSrcDirection, Vector3.up));
        }
    }

    private float ComputePowerCurve(float t)
    {
        return t * t;
    }

    public bool CanSelect()
    {
        return (!rb.isMoving) && (invulnerabilityTimer <= 0);
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

    private void Update()
    {
        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer > 0.0f)
            {
                blinkTimer -= Time.deltaTime;
                if (blinkTimer <= 0.0f)
                {
                    blinkTimer = blinkDuration;
                    setVisible = !setVisible;
                    SetVisible(setVisible);
                }
            }
            else
            {
                SetVisible(true);
            }

        }
    }

    void SetVisible(bool vis)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // Ignore the line renderer on this
            if (renderer == lineRenderer) continue;
            renderer.enabled = vis;
        }
    }
}
