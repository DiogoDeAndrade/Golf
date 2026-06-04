using System.Collections;
using UC;
using UC.RPG;
using UnityEngine;

public class ExplodeBehaviour : AgentBehaviour
{
    [SerializeField] protected Vector3          shakeMaxAmplitude;
    [SerializeField] protected CooldownTimer    buildupTime;
    [SerializeField] protected float            blastTime = 1.0f;
    [SerializeField] protected float            maxBlastRange = 2.5f;
    [SerializeField] protected float            blastDamage = 2.0f;
    [SerializeField] protected GameObject       mainRenderObject;
    [SerializeField] protected Transform        shockwaveTransform;
    [SerializeField] protected MeshRenderer     blastRenderer;
    [SerializeField] protected SoundDef         chargeSound;
    [SerializeField] protected SoundDef         explodeSound;

    Vector3     initialPosition;
    Hypertag    playerTag;
    AudioSource currentChargeSound;

    public override void Enter(Agent agent)
    {
        initialPosition = transform.position;
        buildupTime.Start();
        if (!currentChargeSound)
            currentChargeSound = chargeSound.FadeIn(buildupTime.cooldown);
        playerTag = agent.GetPlayerTag();
    }

    public override void Exit(Agent agent)
    {
        transform.position = initialPosition;
    }

    public override void Tick(Agent agent)
    {
        if (buildupTime.Update())
        {
            transform.position = initialPosition;
            Explode();
        }
        else if (buildupTime.isRunning)
        {
            var displacement = Random.insideUnitSphere * (1.0f - buildupTime.normalizedTime);
            transform.position = initialPosition + new Vector3(shakeMaxAmplitude.x * displacement.x, shakeMaxAmplitude.y * displacement.y, shakeMaxAmplitude.z * displacement.z);

            // Check if player has gone out of range
            var player = playerTag.FindFirst<Ball>();
            if (player)
            {
                float d = Vector3.Distance(player.transform.position.x0z(), transform.position.x0z());
                if (d > maxBlastRange)
                {
                    buildupTime.Stop();
                    transform.position = initialPosition;
                    (agent as AgentFSM).ResetBehaviour();
                    currentChargeSound.FadeTo(0.0f, 0.1f);
                    currentChargeSound = null;
                }
            }
        }
    }

    void Explode()
    {
        StartCoroutine(BlastCR());
    }

    IEnumerator BlastCR()
    {
        currentChargeSound.FadeTo(0.0f, 0.1f);
        currentChargeSound = null;

        explodeSound?.Play();

        float elapsedTime = 0.0f;
        var players = playerTag.FindAll<Ball>();
        float prevDist = 0.0f;
        var mpb = new MaterialPropertyBlock();
        blastRenderer.GetPropertyBlock(mpb);
        var blastColor = blastRenderer.sharedMaterial.GetColor("_EmissionColor");
        blastColor.Normalize();

        while (elapsedTime < blastTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / blastTime);
            t = Mathf.Pow(t, 0.5f);

            float currentDist = maxBlastRange * t;

            foreach (var player in players)
            {
                float d = Vector3.Distance(player.transform.position.x0z(), transform.position.x0z());
                if ((d >= prevDist) && (d < currentDist))
                {
                    var res = player.FindResourceHandler(Globals.healthResource);
                    res.Change(new ChangeData(-blastDamage)
                    {
                        changeSrcPosition = player.transform.position,
                        changeSrcDirection = (player.transform.position - transform.position).normalized,
                        knockbackStrength = 2.0f
                    });
                }
            }

            shockwaveTransform.localScale = currentDist * Vector3.one * 2.0f;
            mpb.SetColor("_EmissionColor", blastColor * 2.5f * (1.0f - t));
            blastRenderer.SetPropertyBlock(mpb);

            prevDist = currentDist;

            mainRenderObject.SetActive(t < 0.5f);

            yield return null;
        }

        Destroy(gameObject);
    }
}
