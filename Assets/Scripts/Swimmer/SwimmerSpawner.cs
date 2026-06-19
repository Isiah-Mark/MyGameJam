using UnityEngine;
using System.Collections.Generic;

public class SwimmerSpawner : MonoBehaviour
{
    [Header("Swimmer Prefabs")]
    public GameObject[] swimmerPrefabs;
    public GameObject[] evilSwimmerPrefabs;

    [HideInInspector] public Vector2 areaMin;
    [HideInInspector] public Vector2 areaMax;

    [Header("Swim Area Padding")]
    public float padLeft = 0f;
    public float padRight = 0f;
    public float padTop = 0f;
    public float padBottom = 0f;

    [Header("Swim Area")]
    public SpriteRenderer backgroundSprite;

    private List<GameObject> activeSwimmers = new List<GameObject>();

    void Start()
    {
        if (backgroundSprite != null)
        {
            areaMin = new Vector2(backgroundSprite.bounds.min.x + padLeft, backgroundSprite.bounds.min.y + padBottom);
            areaMax = new Vector2(backgroundSprite.bounds.max.x - padRight, backgroundSprite.bounds.max.y - padTop);
        }
        else
        {
            areaMin = new Vector2(-17f, -5.5f);
            areaMax = new Vector2(18f, 8f);
        }

        Debug.Log($"Spawner area: {areaMin} to {areaMax}");
    }

    public void Spawn(int count)
    {
        int evilCount = Mathf.RoundToInt(count * 0.2f);
        List<int> evilIndices = new List<int>();

        while (evilIndices.Count < evilCount)
        {
            int index = Random.Range(0, count);
            if (!evilIndices.Contains(index))
                evilIndices.Add(index);
        }

        for (int i = 0; i < count; i++)
        {
            SpawnSwimmer(evilIndices.Contains(i));
        }
    }

    void SpawnSwimmer(bool isEvil = false)
    {
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);

        GameObject prefab = isEvil
            ? evilSwimmerPrefabs[Random.Range(0, evilSwimmerPrefabs.Length)]
            : swimmerPrefabs[Random.Range(0, swimmerPrefabs.Length)];

        GameObject s = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        Swimmer swimmer = s.GetComponent<Swimmer>();
        swimmer.swimAreaMin = areaMin;
        swimmer.swimAreaMax = areaMax;

        if (isEvil)
        {
            var field = typeof(Swimmer).GetField("_isEvil",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(swimmer, true);
        }

        activeSwimmers.Add(s);
    }

    public void ClearSwimmers()
    {
        foreach (GameObject s in activeSwimmers)
        {
            if (s != null) Destroy(s);
        }
        activeSwimmers.Clear();
    }
}