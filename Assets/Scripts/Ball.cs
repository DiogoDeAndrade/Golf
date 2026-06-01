using NaughtyAttributes;
using System;
using UC;
using UC.RPG;
using UnityEditor.ShaderGraph.Internal;
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
    [SerializeField, Header("Attack")]
    private float           minAttackSpeed = 1.0f;
    [SerializeField]
    private float           attackRadius = 1.0f;
    [SerializeField]
    private LayerMask       enemiesLayer;
    [SerializeField]
    private GameObject      attackObjRef;
    [SerializeField]
    private Transform       attackPoint;
    [SerializeField]
    private float           attackTime = 0.3f;
    [SerializeField]
    private float           attackColliderRadius = 0.05f;

    Vector3 hitPos;
    Material        material;
    Material        sourceMaterial;
    BallPhysics     rb;
    float           invulnerabilityTimer;
    float           blinkTimer;
    bool            setVisible = false;
    ResourceHandler attackResource;
    ResourceHandler defenseResource;
    Vector3         prevAttackPos;
    float           attackElapsedTime;

    public float velocity => (rb) ? (rb.linearVelocity.magnitude) : (0.0f);
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
        sourceMaterial = lineRenderer.sharedMaterial;
        lineRenderer.sharedMaterial = material = new Material(lineRenderer.sharedMaterial);

        rb = GetComponent<BallPhysics>();

        var healthResource = this.FindResourceHandler(Globals.healthResource);
        healthResource.onChange += HealthResource_onChange;
        healthResource.canChange += HealthResource_canChange;
        healthResource.onResourceEmpty += HealthResource_onResourceEmpty;

        attackResource = this.FindResourceHandler(Globals.attackResource);
        defenseResource = this.FindResourceHandler(Globals.shieldResource);
    }

    private void OnDestroy()
    {
        var healthResource = this.FindResourceHandler(Globals.healthResource);
        if (healthResource)
        {
            healthResource.onChange -= HealthResource_onChange;
            healthResource.canChange -= HealthResource_canChange;
            healthResource.onResourceEmpty -= HealthResource_onResourceEmpty;
        }

        if (material)
        {
            if (lineRenderer) lineRenderer.sharedMaterial = sourceMaterial;
            material.Delete();
            material = null;
        }
    }

    private void HealthResource_onResourceEmpty(ResourceInstance resourceInstance, GameObject changeSource)
    {
        LevelManager.instance.GameOver();
    }

    private bool HealthResource_canChange(ResourceInstance resource, ChangeData data)
    {
        // Can always heal
        if (data.deltaValue >= 0.0f) return true;

        // Check invulnerability
        if (invulnerabilityTimer > 0.0f) return false;

        // Check if we can mitigate the damage
        var defense = defenseResource.resource;
        var mitigation = Mathf.Min(-data.deltaValue, defense);
        defenseResource.Change(new ChangeData(-mitigation));
        data.deltaValue += mitigation;

        // Is there still damage?
        return data.deltaValue < 0.0f;
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

        if (changeData.knockbackStrength > 0.0f)
        {
            rb.linearVelocity = changeData.changeSrcDirection * changeData.knockbackStrength;
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

        material.SetColor("_EmissionColor", color);
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

        if (attackObjRef.activeInHierarchy)
        {
            attackElapsedTime += Time.deltaTime;
            float t = attackElapsedTime / attackTime;

            if (t < 1.0f)
            {
                attackObjRef.transform.localRotation = Quaternion.Euler(0.0f, t * 360.0f, 0.0f);

                var dir = attackPoint.position - prevAttackPos;
                var dist = dir.magnitude;
                if (dist > 0.0f)
                {
                    dir /= dist;
                    var hits = Physics.SphereCastAll(prevAttackPos, attackColliderRadius, dir, dist, enemiesLayer);
                    foreach (var hit in hits)
                    {
                        ResourceHandler enemyHealth = hit.collider.FindResourceHandler(Globals.healthResource);
                        if (enemyHealth)
                        {
                            float spend = Mathf.Min(enemyHealth.resource, attackResource.resource);

                            Vector3 splatterPos = (hit.point != Vector3.zero) ? (hit.point) : (attackPoint.position);
                            Vector3 splatterDir = (transform.position - hit.point).normalized;
                            enemyHealth.Change(new ChangeData(-spend)
                            {
                                changeSrcPosition = splatterPos,
                                changeSrcDirection = splatterDir
                            });

                            attackResource.Change(new ChangeData(-spend));
                            if (attackResource.isResourceEmpty)
                            {
                                // Stop attack, don't have more resource 
                                CameraShake3d.Shake(0.1f, 0.1f);
                                attackObjRef.SetActive(false);
                            }
                        }
                    }
                }

                prevAttackPos = attackPoint.position;
            }
            else
            {
                attackObjRef.SetActive(false);
            }
        }
        else
        {
            if ((attackResource.resource > 0.0f) && (rb.linearVelocity.magnitude > minAttackSpeed))
            {
                var colliders = Physics.OverlapSphere(transform.position, attackRadius, enemiesLayer);
                foreach (var collider in colliders)
                {
                    ResourceHandler res = collider.FindResourceHandler(Globals.healthResource);
                    if (res)
                    {
                        TriggerAttack();
                    }
                }
            }
        }
    }

    [Button("Attack")]
    void TriggerAttack()
    {
        rb.Stop();
        attackObjRef.SetActive(true);
        prevAttackPos = attackPoint.position;
        attackElapsedTime = 0.0f;
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

    private void OnDrawGizmosSelected()
    {
        if (attackRadius > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }

    public void Stop()
    {
        rb.Stop();
    }
}
