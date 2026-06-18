using UnityEngine;
using System.Collections.Generic;

public class SwimmerSpawner : MonoBehaviour
{
    [Header("Swimmer Prefabs")]
    public GameObject grandmaPrefab;
    public GameObject womanPrefab;

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
            areaMin = new Vector2(backgroundSprite.bounds.min.x, backgroundSprite.bounds.min.y);
            areaMax = new Vector2(backgroundSprite.bounds.max.x, backgroundSprite.bounds.max.y);
        }
        else
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            areaMin = new Vector2(-width + padLeft, -height + padBottom);
            areaMax = new Vector2(width - padRight, height - padTop);
        }
    }

    public void Spawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSwimmer();
        }
    }

    public void ClearSwimmers()
    {
        foreach (GameObject s in activeSwimmers)
        {
            if (s != null) Destroy(s);
        }
        activeSwimmers.Clear();
    }

    void SpawnSwimmer()
    {
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);

        GameObject prefab = Random.value > 0.5f ? grandmaPrefab : womanPrefab;

        GameObject s = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        Swimmer swimmer = s.GetComponent<Swimmer>();
        swimmer.swimAreaMin = areaMin;
        swimmer.swimAreaMax = areaMax;
        activeSwimmers.Add(s);
    }
}