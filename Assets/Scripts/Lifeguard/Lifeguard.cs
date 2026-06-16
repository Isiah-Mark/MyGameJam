using UnityEngine;

public class Lifeguard : MonoBehaviour
{
    [Header("Data")]
    public LifeguardData data;

    private float speed;
    private float shoreY;

    private Swimmer targetSwimmer;
    private float rescueX;

    private enum RescuePhase { None, MoveToSwimmer, DragToShore }
    private RescuePhase phase = RescuePhase.None;

    public bool IsFree => phase == RescuePhase.None;

    void Start()
    {
        Camera cam = Camera.main;
        shoreY = -cam.orthographicSize - 2f;
    }

    public void Initialise(LifeguardData lifeguardData)
    {
        data = lifeguardData;
        speed = data.speed;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            sr.sortingOrder += 3;
        }

        Camera cam = Camera.main;
        shoreY = -cam.orthographicSize - 2f;
    }

    void Update()
    {
        switch (phase)
        {
            case RescuePhase.MoveToSwimmer: MoveToTarget(); break;
            case RescuePhase.DragToShore: DragToShore(); break;
        }
    }

    public void AssignTarget(Swimmer swimmer)
    {
        if (phase == RescuePhase.DragToShore) return;

        targetSwimmer = swimmer;
        targetSwimmer.MarkRescueAssigned();
        rescueX = swimmer.transform.position.x;
        transform.position = new Vector2(rescueX, transform.position.y);
        phase = RescuePhase.MoveToSwimmer;
    }

    void MoveToTarget()
    {
        if (targetSwimmer == null) { phase = RescuePhase.None; return; }

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetSwimmer.transform.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetSwimmer.transform.position) < 1.5f)
        {
            rescueX = targetSwimmer.transform.position.x;
            targetSwimmer.BeingRescued = true;
            phase = RescuePhase.DragToShore;
        }
    }

    void DragToShore()
    {
        if (targetSwimmer == null) { phase = RescuePhase.None; return; }

        Vector2 shorePosition = new Vector2(rescueX, shoreY);
        transform.position = Vector2.MoveTowards(transform.position, shorePosition, speed * Time.deltaTime);

        targetSwimmer.transform.position = (Vector2)transform.position + Vector2.up * 1.5f;

        if (Mathf.Abs(((Vector2)transform.position).y - shoreY) < 0.1f)
        {
            SwimmerManager.Instance?.ReportSaved();
            Destroy(targetSwimmer.gameObject);
            targetSwimmer = null;
            phase = RescuePhase.None;
        }
    }
}