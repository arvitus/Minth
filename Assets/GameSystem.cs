using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class PythonExtensions
{
    public static void Enumerate<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        int i = 0;
        foreach (var thing in source)
        {
            action(thing, i);
            i++;
        }
    }
}
public class GameSystem : MonoBehaviour
{
    [Header("Global")]
    public List<GameObject> levels;
    public int currentLevel = 0;
    public bool disableScore = true;

    [Header("Player")]
    public float playerSpeed = 5;
    public bool showPlayerTrail = false;
    public Color playerTrailColor = Color.red;

    [Header("Level")]
    [Range(0, 0.5f)]
    public float levelPadding = 0.25f;
    [Range(0.05f, 1)]
    public float goalDistance = 0.4f;
    [Range(0, 60)]
    public int introDuration = 5;

    [Header("Enemy")]
    [Range(0, 10)]
    public float enemyDelay = 0.5f;
    [Range(1, 5)]
    public float enemySpeed = 3;
    public bool disableEnemy = false;

    [Header("Colors")]
    public Color lightOn = Color.white;
    public Color lightOff = Color.black;
    public Color playerLight = Color.yellow;
    public Color maze = Color.black;

    public static GameSystem Instance { get; private set; }
    [HideInInspector]
    public Player player;
    [HideInInspector]
    public Enemy enemy;
    [HideInInspector]
    public TMP_Text score;
    [HideInInspector]
    public Button bNext;
    [HideInInspector]
    public Button bRetry;

    public Level GetCurrentLevel()
    {
        return levels[currentLevel].GetComponent<Level>();
    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levels.Count) currentLevel = 0;
        Run();
    }

    public void Run()
    {
        GetCurrentLevel().Run();
    }

    void Awake()
    {
        Debug.Log("Initializing GameSystem");
        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }
        Instance = this;

        player = FindObjectOfType<Player>();
        enemy = FindObjectOfType<Enemy>();
        var canvas = FindObjectOfType<Canvas>();
        score = canvas.transform.Find("score").GetComponent<TMP_Text>();
        bNext = canvas.transform.Find("bNext").GetComponent<Button>();
        bRetry = canvas.transform.Find("bRetry").GetComponent<Button>();
        currentLevel = Math.Clamp(currentLevel, 0, levels.Count - 1);

        var disabledLevels = FindObjectsOfType<Level>().Where(level => !levels.Contains(level.gameObject));
        foreach (var level in disabledLevels) { Destroy(level.gameObject); }
    }

    void Start()
    {
        GetCurrentLevel().Run();
    }
}