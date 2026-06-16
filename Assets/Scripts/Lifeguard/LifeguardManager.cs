using UnityEngine;

[System.Serializable]
public struct LifeguardSpawnEntry
{
    public Transform spawnPoint;
    public LifeguardData data;
}

public class LifeguardManager : MonoBehaviour
{
    public static LifeguardManager Instance { get; private set; }

    [Header("Lifeguards")]
    public GameObject lifeguardPrefab;
    public LifeguardSpawnEntry[] lifeguards;

    private Lifeguard[] spawnedLifeguards;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        spawnedLifeguards = new Lifeguard[lifeguards.Length];

        for (int i = 0; i < lifeguards.Length; i++)
        {
            Debug.Log($"Spawning lifeguard {i} with data: {lifeguards[i].data?.lifeguardName}");
            GameObject lg = Instantiate(lifeguardPrefab, lifeguards[i].spawnPoint.position, Quaternion.identity);
            Lifeguard lifeguard = lg.GetComponent<Lifeguard>();
            lifeguard.Initialise(lifeguards[i].data);
            spawnedLifeguards[i] = lifeguard;
        }
    }

    public bool HasFreeLifeguard()
    {
        foreach (Lifeguard lg in spawnedLifeguards)
            if (lg != null && lg.IsFree) return true;
        return false;
    }

    public Lifeguard[] GetLifeguards() => spawnedLifeguards;
}