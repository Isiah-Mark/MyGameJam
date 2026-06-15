using UnityEngine;

public class SwimmerSpawner : MonoBehaviour
{
    public GameObject swimmerPrefab;
    public int count = 10;

    [HideInInspector] public Vector2 areaMin;
    [HideInInspector] public Vector2 areaMax;

    [Header("Swim Area Padding")]
    public float padLeft = 0f;
    public float padRight = 0f;
    public float padTop = 0f;
    public float padBottom = 0f;

    void Start()
    {
        Camera cam = Camera.main;
        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        areaMin = new Vector2(-width + padLeft, -height + padBottom);
        areaMax = new Vector2(width - padRight, height - padTop);

        for (int i = 0; i < count; i++)
        {
            SpawnSwimmer();
        }
    }

    void SpawnSwimmer()
    {
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);

        GameObject s = Instantiate(swimmerPrefab, new Vector3(x, y, 0), Quaternion.identity);

        Swimmer swimmer = s.GetComponent<Swimmer>();
        swimmer.swimAreaMin = areaMin;
        swimmer.swimAreaMax = areaMax;
    }
}