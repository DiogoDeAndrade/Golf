using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLine : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private List<GameObject> prefabs = new();
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private Vector3 noiseOffset = Vector3.zero;
    [Header("Null Prefab Wait"), MinMaxSlider(0.1f, 5.0f)]
    [SerializeField] private Vector2 nullPrefabWaitTime = new Vector2(0.5f, 1.5f);

    [Header("Movement")]
    [SerializeField] private Vector3 speed = new Vector3(0, 0, 1);

    [Header("Prewarm")]
    [SerializeField] private float prewarmTime = 0.0f;
    [SerializeField] private float prewarmStep = 0.02f;

    [Header("Collider Setup")]
    [SerializeField] private bool createBoxColliderIfMissing = true;
    [SerializeField] private bool useMeshRendererBounds = true;

    private SpawnLineObject waitingObject;
    private SpawnLineObject previousObject;
    private float spawnTimer;

    private readonly List<SpawnLineObject> activeObjects = new();

    private void Start()
    {
        if (prewarmTime > 0.0f)
        {
            Prewarm(prewarmTime);
        }
    }

    private void Update()
    {
        Step(Time.deltaTime, moveObjects: false);
    }

    private void Prewarm(float time)
    {
        float remaining = time;

        while (remaining > 0.0f)
        {
            float dt = Mathf.Min(prewarmStep, remaining);

            Step(dt, moveObjects: true);

            remaining -= dt;
        }

        Physics.SyncTransforms();
    }

    private void Step(float dt, bool moveObjects)
    {
        CleanupDestroyedObjects();

        spawnTimer -= dt;

        if ((waitingObject == null) && (spawnTimer <= 0.0f))
        {
            bool spawnedObject = SpawnWaitingObject();

            if (spawnedObject)
                spawnTimer = spawnInterval;
        }

        if (waitingObject != null)
        {
            Physics.SyncTransforms();

            if (CanStartMoving(waitingObject, previousObject))
            {
                waitingObject.StartMoving(speed);
                previousObject = waitingObject;
                waitingObject = null;
            }
        }

        if (moveObjects)
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                if (activeObjects[i] == null)
                {
                    activeObjects.RemoveAt(i);
                    continue;
                }

                activeObjects[i].Move(dt);
            }

            Physics.SyncTransforms();
        }
    }

    private void CleanupDestroyedObjects()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (activeObjects[i] == null)
                activeObjects.RemoveAt(i);
        }

        if (previousObject == null)
            previousObject = null;

        if (waitingObject == null)
            waitingObject = null;
    }

    private bool SpawnWaitingObject()
    {
        if (prefabs == null || prefabs.Count == 0)
            return false;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

        if (prefab == null)
        {
            spawnTimer = Random.Range(nullPrefabWaitTime.x, nullPrefabWaitTime.y);
            return false;
        }

        Vector3 spawnPosition = transform.position + GetRandomNoiseOffset();
        GameObject obj = Instantiate(prefab, spawnPosition, transform.rotation);

        Collider col = EnsureTriggerCollider(obj);

        if (col == null)
        {
            Debug.LogWarning($"SpawnLine: Spawned object '{obj.name}' has no collider and no collider could be created.");
            Destroy(obj);
            return false;
        }

        var mover = obj.GetComponent<SpawnLineObject>();
        if (mover == null)
            mover = obj.AddComponent<SpawnLineObject>();

        mover.Initialize(col);

        waitingObject = mover;
        activeObjects.Add(mover);

        Physics.SyncTransforms();

        return true;
    }

    private Vector3 GetRandomNoiseOffset()
    {
        return new Vector3(
            Random.Range(-noiseOffset.x, noiseOffset.x),
            Random.Range(-noiseOffset.y, noiseOffset.y),
            Random.Range(-noiseOffset.z, noiseOffset.z)
        );
    }

    private Collider EnsureTriggerCollider(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        Collider col = obj.GetComponentInChildren<Collider>();

        if (col != null)
        {
            if (col as MeshCollider)
            {
                Destroy(col);
            }
            else
            {
                col.isTrigger = true;
                return col;
            }
        }

        if (!createBoxColliderIfMissing)
            return null;

        MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();

        if (renderer == null)
            return null;

        GameObject colliderObject = renderer.gameObject;
        BoxCollider box = colliderObject.AddComponent<BoxCollider>();
        box.isTrigger = true;

        if (useMeshRendererBounds)
        {
            Bounds localBounds = renderer.localBounds;
            box.center = localBounds.center;
            box.size = localBounds.size;
        }

        return box;
    }

    private bool CanStartMoving(SpawnLineObject current, SpawnLineObject previous)
    {
        if (previous == null)
            return true;

        if (previous == current)
            return true;

        if (current.Collider == null || previous.Collider == null)
            return true;

        return !CollidersOverlap(current.Collider, previous.Collider);
    }

    private bool CollidersOverlap(Collider a, Collider b)
    {
        Vector3 direction;
        float distance;

        bool overlap = Physics.ComputePenetration(
            a, a.transform.position, a.transform.rotation,
            b, b.transform.position, b.transform.rotation,
            out direction, out distance
        );

        return overlap;
    }
}