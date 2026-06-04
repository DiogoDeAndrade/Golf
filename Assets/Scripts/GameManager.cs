using System;
using UC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum State { MainMenu, Playing, Debug, EndGame };

    [field: SerializeField] public State state { get; private set; } = State.Debug;
    [field: SerializeField] public int startLevel { get; private set; } = -1;
    [SerializeField] private Map[] levels;
    [SerializeField] private AudioClip softMusic;
    [SerializeField] private AudioClip adventureMusic;
    [SerializeField] private AudioClip finalMusic;

    public int currentLevel { get; private set; }
    public Map currentMap { get; private set; }

    public static GameManager instance
    {
        get
        {
            if (!_instance) _instance = FindFirstObjectByType<GameManager>();

            return _instance;
        }
    }

    private static GameManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SoundManager.PlayMusic(softMusic);
    }

    public void LoadLevel(int levelIndex)
    {
        // Delete any existing level
        var maps = FindObjectsByType<Map>(FindObjectsSortMode.None);
        foreach (var map in maps)
        {
            Destroy(map.gameObject);
        }
        currentMap = null;

        if (levels.Length <= levelIndex)
        {
            levelIndex = levels.Length - 1;
        }

        // Instance current level
        currentMap = Instantiate(levels[levelIndex], Vector3.zero, Quaternion.identity);
        currentLevel = levelIndex;

        var cameraCtrl = FindFirstObjectByType<TacticalCameraController>();
        cameraCtrl.targetBoundsCollider = currentMap.GetComponent<BoxCollider>();

        if (currentMap.healthDisplay)
            SoundManager.PlayMusic(adventureMusic);
        else
            SoundManager.PlayMusic(softMusic);
    }

    public void StartGame()
    {
        state = State.Playing;
        currentLevel = 0;
        PlayerPrefs.SetInt("MaxLevel", currentLevel);
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void RetryLevel()
    {
        state = State.Playing;
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void NextLevel()
    {
        state = State.Playing;
        currentLevel++;

        int levelData = PlayerPrefs.GetInt("MaxLevel", 0);
        if (levelData < currentLevel) PlayerPrefs.SetInt("MaxLevel", currentLevel);

        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            if (currentLevel >= levels.Length)
            {
                state = State.EndGame;
                SceneManager.LoadScene("EndGame");
                SoundManager.PlayMusic(finalMusic);
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        });
    }

    public void MainMenu()
    {
        state = State.MainMenu;
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }


    public void ResetLevel()
    {
        LoadLevel(currentLevel);
    }

    public void SetDebugLevel(int level)
    {
        if (level == -1)
        {
            currentMap = FindFirstObjectByType<Map>();

            if (!currentMap)
            {
                currentLevel = -1;
                return;
            }

            currentLevel = FindLevelIndexFromSceneMap(currentMap);

            var cameraCtrl = FindFirstObjectByType<TacticalCameraController>();
            cameraCtrl.targetBoundsCollider = currentMap.GetComponent<BoxCollider>();
        }
        else
        {
            currentLevel = level;
            ResetLevel();
        }

        if (currentMap.healthDisplay)
            SoundManager.PlayMusic(adventureMusic);
        else
            SoundManager.PlayMusic(softMusic);
    }

    private int FindLevelIndexFromSceneMap(Map sceneMap)
    {
        if (!sceneMap) return -1;

        for (int i = 0; i < levels.Length; i++)
        {
            if (!levels[i]) continue;

            if (levels[i].levelGUID == sceneMap.levelGUID) return i;
        }

        return -1;
    }

    internal void ContinueGame()
    {
        state = State.Playing;
        currentLevel = PlayerPrefs.GetInt("MaxLevel", 0);
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }
}
