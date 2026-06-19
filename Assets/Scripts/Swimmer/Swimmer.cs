using UnityEngine;

public class Swimmer : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 0.4f;
    public float maxSpeed = 1.2f;
    private float speed;
    private float baseSpeed;

    [Header("Swim Area")]
    public Vector2 swimAreaMin;
    public Vector2 swimAreaMax;

    [Header("Idle Behaviour")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 4f;

    [Header("Avoidance")]
    public float avoidRadius = 1.2f;
    public float avoidStrength = 2f;

    [Header("Drowning")]
    public float minTimeUntilDrown = 10f;
    public float maxTimeUntilDrown = 30f;
    public float sinkDuration = 2f;
    public static int maxDrowning = 2;
    public bool IsDrowning => isDrowning;

    [Tooltip("Set by the evil-swimmer system. Deploying a lifeguard to an evil swimmer loses it.")]
    [SerializeField] private bool _isEvil;
    public bool IsEvil => _isEvil;

    public bool BeingRescued
    {
        get => _beingRescued;
        set
        {
            _beingRescued = value;
            if (value)
            {
                _cacheTime = -1f;
                SetAnimationState(ANIM_SAVED);
            }
        }
    }

    [Header("Panic")]
    [Range(0f, 1f)]
    public float panicChance = 0.3f;
    
    [Header("Audio")]
    [Tooltip("The drowning sound effect")]
    public AudioClip drownSound;
    private AudioSource audioSource;

    private Vector2 target;
    protected bool isIdle = false;
    protected float idleTimer;
    protected float drownTimer;
    protected bool isDrowning = false;
    protected bool isSinking = false;
    protected float sinkTimer;
    protected Vector3 baseScale;

    private Animator animator;
    private SwimmerSpawner spawner;

    private static Swimmer[] _allSwimmers;
    private static float _cacheTime = -1f;
    private const float CacheInterval = 0.5f;
    private const float borderBuffer = 0.5f;

    private const int ANIM_TREADING = 0;
    private const int ANIM_DROWNING = 1;
    private const int ANIM_SAVED = 2;

    public bool IsBeingRescued => _beingRescued || _rescueAssigned;
    private bool _rescueAssigned = false;
    private bool _beingRescued;

    public void MarkRescueAssigned()
    {
        _rescueAssigned = true;
    }

    void Start()
    {
        if (swimAreaMin == Vector2.zero && swimAreaMax == Vector2.zero)
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize;
            float width = height * cam.aspect;
            swimAreaMin = new Vector2(-width + 1.5f, -height + 2f);
            swimAreaMax = new Vector2(width - 1.5f, height - 1.5f);
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
            sr.sortingOrder += 1;

        speed = Random.Range(minSpeed, maxSpeed);
        if (_isEvil) speed *= 0.4f;
        baseSpeed = speed;

        animator = GetComponentInChildren<Animator>();
        drownTimer = Random.Range(minTimeUntilDrown, maxTimeUntilDrown);
        spawner = FindAnyObjectByType<SwimmerSpawner>();
        baseScale = transform.localScale;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetAnimationState(ANIM_TREADING);
        PickNewTarget();
    }

    void Update()
    {
        if (BeingRescued) return;

        if (isSinking)
        {
            HandleSinking();
            return;
        }

        if (isIdle)
        {
            HandleIdle();
            return;
        }

        if (!isDrowning)
        {
            drownTimer -= Time.deltaTime;
            if (drownTimer <= 0f)
            {
                TryStartDrowning();
            }
        }
        else
        {
            drownTimer -= Time.deltaTime;
            if (drownTimer <= 0f)
            {
                StartSinking();
            }
        }

        Move();
    }

    void SetAnimationState(int state)
    {
        if (animator == null) return;
        animator.SetInteger("State", state);
    }

    void TryStartDrowning()
    {
        int drowningCount = 0;
        foreach (Swimmer s in GetAllSwimmers())
        {
            if (s.isDrowning) drowningCount++;
        }

        if (drowningCount < maxDrowning)
        {
            isDrowning = true;
            drownTimer = Random.Range(5f, 10f);
            speed = baseSpeed * 0.1f;
            SetAnimationState(ANIM_DROWNING);
            
            if (audioSource != null && drownSound != null)
            {
                audioSource.PlayOneShot(drownSound);
            }
        }
        else
        {
            drownTimer = Random.Range(minTimeUntilDrown, maxTimeUntilDrown);
        }
    }

    void StartSinking()
    {
        isSinking = true;
        sinkTimer = sinkDuration;

        Swimmer[] all = GetAllSwimmers();
        Swimmer[] candidates = System.Array.FindAll(all, s => s != this && !s.isDrowning && !s.isSinking);

        if (candidates.Length > 0 && Random.value < panicChance)
        {
            Swimmer victim = candidates[Random.Range(0, candidates.Length)];
            victim.drownTimer = Random.Range(3f, 8f);
        }
    }

    void HandleSinking()
    {
        sinkTimer -= Time.deltaTime;
        float scale = Mathf.Clamp01(sinkTimer / sinkDuration);
        transform.localScale = baseScale * scale;

        if (sinkTimer <= 0f)
        {
            SwimmerManager.Instance?.ReportDrowned(IsEvil);
            Destroy(gameObject);
        }
    }

    bool IsNearBorder()
    {
        Vector2 pos = transform.position;
        return pos.x <= swimAreaMin.x + borderBuffer ||
               pos.x >= swimAreaMax.x - borderBuffer ||
               pos.y <= swimAreaMin.y + borderBuffer ||
               pos.y >= swimAreaMax.y - borderBuffer;
    }

    void Move()
    {
        Vector2 pos = transform.position;
        Vector2 toTarget = (Vector2)target - pos;

        if (toTarget.magnitude < 0.2f)
        {
            if (isDrowning)
            {
                PickNewTarget();
            }
            else
            {
                StartIdle();
            }
            return;
        }

        Vector2 goalDir = toTarget.normalized;
        Vector2 separation = GetSeparationVector();
        Vector2 finalDirection = (goalDir + separation * avoidStrength).normalized;

        transform.position = pos + finalDirection * speed * Time.deltaTime;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, swimAreaMin.x, swimAreaMax.x),
            Mathf.Clamp(transform.position.y, swimAreaMin.y, swimAreaMax.y),
            transform.position.z
        );

        if (IsNearBorder() && !isDrowning)
        {
            float x = Random.Range(swimAreaMin.x + 2f, swimAreaMax.x - 2f);
            float y = Random.Range(swimAreaMin.y + 2f, swimAreaMax.y - 2f);
            target = new Vector2(x, y);
        }

        EnforceMinDistance();
    }

    void EnforceMinDistance()
    {
        Vector2 pos = transform.position;

        foreach (Swimmer other in GetAllSwimmers())
        {
            if (other == null || other == this) continue;

            Vector2 diff = pos - (Vector2)other.transform.position;
            float distance = diff.magnitude;

            if (distance < 0.5f && distance > 0.0001f)
            {
                transform.position = (Vector2)other.transform.position + diff.normalized * 0.5f;
            }
        }
    }

    Vector2 GetSeparationVector()
    {
        Vector2 force = Vector2.zero;
        Vector2 pos = transform.position;

        foreach (Swimmer other in GetAllSwimmers())
        {
            if (other == null || other == this) continue;

            Vector2 diff = pos - (Vector2)other.transform.position;
            float distance = diff.magnitude;

            if (distance < avoidRadius && distance > 0.0001f)
            {
                Vector2 lateral = Vector2.Perpendicular(diff.normalized);
                Vector2 push = (diff.normalized + lateral * 0.6f).normalized;
                force += push * (1f - distance / avoidRadius);
            }
        }

        return force;
    }

    Swimmer[] GetAllSwimmers()
    {
        if (Time.time - _cacheTime > CacheInterval)
        {
            _allSwimmers = System.Array.FindAll(
                FindObjectsByType<Swimmer>(FindObjectsSortMode.None),
                s => s != null
            );
            _cacheTime = Time.time;
        }
        return _allSwimmers;
    }

    void StartIdle()
    {
        isIdle = true;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        SetAnimationState(ANIM_TREADING);
    }

    void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            isIdle = false;
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        if (isDrowning)
        {
            Vector2 thrash = Random.insideUnitCircle * 0.5f;
            target = (Vector2)transform.position + thrash;
            target.x = Mathf.Clamp(target.x, swimAreaMin.x, swimAreaMax.x);
            target.y = Mathf.Clamp(target.y, swimAreaMin.y, swimAreaMax.y);
        }
        else
        {
            SetAnimationState(ANIM_TREADING);
            float x = Random.Range(swimAreaMin.x, swimAreaMax.x);
            float y = Random.Range(swimAreaMin.y, swimAreaMax.y);
            target = new Vector2(x, y);
        }
    }

    void OnDestroy()
    {
        _cacheTime = -1f;
    }
}