using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField]
    [Range(-1, 60)]
    private int _introDuration = -1;
    public int introDuration
    {
        get { return _introDuration >= 0 ? _introDuration : GameSystem.Instance.introDuration; }
        set { _introDuration = Math.Clamp(value, -1, 60); }
    }
    [SerializeField]
    [Range(-1, 10)]
    private float _enemyDelay = -1;
    public float enemyDelay
    {
        get { return _enemyDelay >= 0 ? _enemyDelay : GameSystem.Instance.enemyDelay; }
        set { _enemyDelay = Math.Clamp(value, -1, 10); }
    }
    [SerializeField]
    [Range(-1, 5)]
    private float _enemySpeed = -1;
    public float enemySpeed
    {
        get { return _enemySpeed >= 0 ? _enemySpeed : GameSystem.Instance.enemySpeed; }
        set { _enemySpeed = Math.Clamp(value, -1, 5); }
    }

    [Header("Colors")]
    [SerializeField]
    private Color _lightOn = Color.clear;
    public Color lightOn
    {
        get { return _lightOn != Color.clear ? _lightOn : GameSystem.Instance.lightOn; }
        set { _lightOn = value; }
    }
    [SerializeField]
    private Color _lightOff = Color.clear;
    public Color lightOff
    {
        get { return _lightOff != Color.clear ? _lightOff : GameSystem.Instance.lightOff; }
        set { _lightOff = value; }
    }
    [SerializeField]
    private Color _playerLight = Color.clear;
    public Color playerLight
    {
        get { return _playerLight != Color.clear ? _playerLight : GameSystem.Instance.playerLight; }
        set { _playerLight = value; }
    }
    [SerializeField]
    private Color _maze = Color.clear;
    public Color maze
    {
        get { return _maze != Color.clear ? _maze : GameSystem.Instance.maze; }
        set { _maze = value; }
    }

    [HideInInspector]
    public GameObject spawn;
    [HideInInspector]
    public GameObject goal;
    [HideInInspector]
    public GameObject enemyTrigger;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    private bool _started = false;
    private DateTime _startTime;
    private bool _finished = false;
    private bool _cancelled = false;
    private Action<Level> _finishCallback;
    private Queue<Vector2> _path = new();
    private GameObject _pathGameObject;

    public bool started { get { return _started; } }
    public bool finished { get { return _finished; } }
    public bool cancelled { get { return _cancelled; } }
    public bool running { get { return _started && !_finished && !cancelled; } }

    public static Sprite goalSprite;

    void Awake()
    {
        goalSprite = Resources.Load<Sprite>("goal");
    }

    void Start()
    {
        if (!GameSystem.Instance.levels.Find((level) => level.gameObject == gameObject))
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = maze;
        var extents = spriteRenderer.bounds.extents;
        var center = spriteRenderer.bounds.center;

        var offset = 0.5f;
        var spawnCenter = new Vector2(center.x, center.y - extents.y - offset);

        spawn = new GameObject("spawn");
        spawn.transform.parent = transform;
        spawn.transform.position = spawnCenter;
        var spawnCollider = spawn.AddComponent<PolygonCollider2D>();
        var width = 0.1f;
        spawnCollider.points = new Vector2[8] {
            // right
            new Vector2(extents.x - width, -offset),
            new Vector2(extents.x - width, offset),
            new Vector2(extents.x, offset),
            new Vector2(extents.x, -offset - width),
            // left
            new Vector2(-extents.x, -offset - width),
            new Vector2(-extents.x, offset),
            new Vector2(-extents.x + width, offset),
            new Vector2(-extents.x + width, -offset),
        };

        goal = Instantiate(spawn, new Vector2(spawnCenter.x, -spawnCenter.y), Quaternion.Euler(0, 0, 180), transform);
        goal.name = "goal";
        var goalIcon = new GameObject("icon");
        goalIcon.transform.parent = goal.transform;
        goalIcon.transform.localPosition = Vector3.zero;
        goalIcon.transform.localScale = Vector3.one * 0.15f;
        goalIcon.AddComponent<SpriteRenderer>().sprite = goalSprite;

        enemyTrigger = new GameObject("enemyTrigger");
        enemyTrigger.transform.parent = transform;
        enemyTrigger.transform.position = new Vector2(center.x, center.y - extents.y + 2 * offset);

        _pathGameObject = new GameObject("path");
        _pathGameObject.transform.parent = transform;
    }

    public void Run(Action<Level> finishCallback = null)
    {
        var game = GameSystem.Instance;

        game.levels.ForEach((level) => level.gameObject.SetActive(false));
        gameObject.SetActive(true);

        game.bNext.gameObject.SetActive(false);
        game.bRetry.gameObject.SetActive(false);

        game.score.gameObject.SetActive(!game.disableScore);
        game.score.text = "00:00.000";

        game.player.gameObject.SetActive(true);
        game.player.transform.position = spawn.transform.position;

        game.enemy.gameObject.SetActive(false);
        game.enemy.transform.position = spawn.transform.position;

        enemyTrigger.SetActive(!game.disableEnemy);

        game.score.color = playerLight;

        _finishCallback = finishCallback;

        _started = false;
        _finished = false;
        _cancelled = false;
        Camera.main.backgroundColor = lightOn;

        _path.Clear();
        foreach (Transform child in _pathGameObject.transform) { Destroy(child.gameObject); }

        FitCamera();

        StartCoroutine(Intro());
    }

    public void AddPathPoint(Vector2 point)
    {
        _path.Enqueue(point);

        var game = GameSystem.Instance;
        var pathPoint = new GameObject("point" + _pathGameObject.transform.childCount);
        pathPoint.transform.position = point;
        pathPoint.transform.localScale = Vector3.one * (game.showPlayerTrail ? 0.1f : 0);
        var pathPointSpriteRenderer = pathPoint.AddComponent<SpriteRenderer>();
        pathPointSpriteRenderer.sprite = game.player.GetComponent<SpriteRenderer>().sprite;
        pathPointSpriteRenderer.color = game.playerTrailColor;
        pathPoint.transform.parent = _pathGameObject.transform;
    }

    public Vector2 GetNextTarget()
    {
        return _path.Count > 0 ? _path.Dequeue() : GameSystem.Instance.player.transform.position;
    }

    void FitCamera()
    {
        Camera cam = Camera.main;
        Bounds levelBounds = spriteRenderer.bounds;
        Bounds spawnBounds = spawn.GetComponent<PolygonCollider2D>().bounds;
        Bounds goalBounds = goal.GetComponent<PolygonCollider2D>().bounds;

        float spriteHeight = (levelBounds.size.y + spawnBounds.size.y + goalBounds.size.y) * (1 + GameSystem.Instance.levelPadding);
        float spriteWidth = levelBounds.size.x;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = spriteWidth / spriteHeight;

        if (screenRatio >= targetRatio)
        {
            cam.orthographicSize = spriteHeight / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = spriteHeight / 2 * differenceInSize;
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(introDuration);
        OnStart();
    }

    void OnStart()
    {
        var game = GameSystem.Instance;

        Camera.main.backgroundColor = lightOff;
        _startTime = DateTime.Now;
        _started = true;
        game.player.lightSpriteRenderer.color = playerLight;
    }

    void Finish()
    {
        End();
        _finished = true;
        _finishCallback?.Invoke(this);
        GameSystem.Instance.score.color = Color.green;
        GameSystem.Instance.bNext.gameObject.SetActive(true);
    }

    void End()
    {
        var game = GameSystem.Instance;
        Camera.main.backgroundColor = lightOn;
        game.player.gameObject.SetActive(false);
        game.enemy.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        End();
        _cancelled = true;
        GameSystem.Instance.score.color = Color.red;
        GameSystem.Instance.bRetry.gameObject.SetActive(true);
    }

    void Update()
    {
        var game = GameSystem.Instance;
        if (running)
        {
            game.score.text = (DateTime.Now - _startTime).ToString("mm\\:ss\\.fff");
            // goal.transform.GetComponent<PolygonCollider2D>().IsTouching(game.player.GetComponent<PolygonCollider2D>());
            if (Vector3.Distance(game.player.transform.position, goal.transform.position) < game.goalDistance)
            {
                Finish();
            }
            if (
                enemyTrigger.activeSelf &&
                Math.Abs(game.player.transform.position.y - enemyTrigger.transform.position.y) < 0.1f
            )
            {
                StartCoroutine(game.enemy.StartFollowing(this));
                enemyTrigger.SetActive(false);
            }
        }
        if (finished && Input.GetKeyDown(KeyCode.Return)) { game.NextLevel(); }
        if (cancelled && Input.GetKeyDown(KeyCode.Return)) { game.Run(); }
    }

}
