using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Map[] levels;

    int currentLevel = 0;

    public static GameManager instance
    {
        get
        {
            if (!_instance) _instance = FindFirstObjectByType<GameManager>();

            return _instance;
        }
    }

    private static GameManager _instance;

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
