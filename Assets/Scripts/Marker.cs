using UC;
using UnityEngine;
using System.Collections.Generic;

public class Marker : MonoBehaviour
{
    public enum Type { Start, Goal };

    [SerializeField] private Type type;
    [field: SerializeField] public float radius { get; private set; } = 0.1f;


    public static Marker FindMarker(Type type)
    {
        var markers = FindMarkers(type);
        if (markers.Count > 0)
        {
            return markers[0];
        }

        return null;
    }

    public static List<Marker> FindMarkers(Type type)
    {
        var foundMarkers = new List<Marker>();
        var markers = FindObjectsByType<Marker>(FindObjectsSortMode.None);
        foreach (var marker in markers)
        {
            if (marker.type == type)
            {
                foundMarkers.Add(marker);
            }
        }
        return foundMarkers;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        if (type == Type.Start)
        {
            Gizmos.color = Color.green;
        }
        else if (type == Type.Goal)
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(transform.position, radius);
        DebugHelpers.DrawTextAt(transform.position, Vector3.up * radius, 12, Color.white, type.ToString(), shadow: true);
    }
}
