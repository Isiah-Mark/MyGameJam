using System;
using UnityEngine;

/// <summary>How a rescue ended, reported back to whoever deployed the lifeguard.</summary>
public enum RescueOutcome
{
    /// <summary>The swimmer was dragged to shore safely.</summary>
    Saved,

    /// <summary>The target was an evil swimmer; the lifeguard is lost.</summary>
    LifeguardLost,

    /// <summary>The target vanished before the rescue finished; the lifeguard comes back.</summary>
    Aborted
}

public class Lifeguard : MonoBehaviour
{
    private float speed;
    private float shoreY;

    private Swimmer targetSwimmer;
    private float rescueX;
    private Action<RescueOutcome> onComplete;

    private enum RescuePhase { None, MoveToSwimmer, DragToShore }
    private RescuePhase phase = RescuePhase.None;

    public bool IsFree => phase == RescuePhase.None;

    void Awake()
    {
        Camera cam = Camera.main;
        if (cam != null) shoreY = -cam.orthographicSize - 2f;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
            sr.sortingOrder += 3;
    }

    void Update()
    {
        switch (phase)
        {
            case RescuePhase.MoveToSwimmer: MoveToTarget(); break;
            case RescuePhase.DragToShore: DragToShore(); break;
        }
    }

    /// <summary>
    /// Sends this lifeguard out to rescue <paramref name="swimmer"/> at the given world speed.
    /// <paramref name="onRescueComplete"/> fires once when the rescue resolves (saved, lost, or
    /// aborted) so the deployer can update the roster and despawn this lifeguard.
    /// </summary>
    public void AssignTarget(Swimmer swimmer, float moveSpeed, Action<RescueOutcome> onRescueComplete)
    {
        if (phase == RescuePhase.DragToShore) return;

        speed = moveSpeed;
        onComplete = onRescueComplete;
        targetSwimmer = swimmer;
        targetSwimmer.MarkRescueAssigned();
        rescueX = swimmer.transform.position.x;
        transform.position = new Vector2(rescueX, transform.position.y);
        phase = RescuePhase.MoveToSwimmer;
    }

    void MoveToTarget()
    {
        if (targetSwimmer == null) { Complete(RescueOutcome.Aborted); return; }

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetSwimmer.transform.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetSwimmer.transform.position) < 1.5f)
        {
            // An evil swimmer drags the lifeguard down: both are lost, no rescue.
            if (targetSwimmer.IsEvil)
            {
                Destroy(targetSwimmer.gameObject);
                targetSwimmer = null;
                Complete(RescueOutcome.LifeguardLost);
                return;
            }

            rescueX = targetSwimmer.transform.position.x;
            targetSwimmer.BeingRescued = true;
            phase = RescuePhase.DragToShore;
        }
    }

    void DragToShore()
    {
        if (targetSwimmer == null) { Complete(RescueOutcome.Aborted); return; }

        Vector2 shorePosition = new Vector2(rescueX, shoreY);
        transform.position = Vector2.MoveTowards(transform.position, shorePosition, speed * Time.deltaTime);

        targetSwimmer.transform.position = (Vector2)transform.position + Vector2.up * 1.5f;

        if (Mathf.Abs(((Vector2)transform.position).y - shoreY) < 0.1f)
        {
            SwimmerManager.Instance?.ReportSaved();
            Destroy(targetSwimmer.gameObject);
            targetSwimmer = null;
            Complete(RescueOutcome.Saved);
        }
    }

    // Fires the completion callback once. The deployer despawns this lifeguard from here.
    void Complete(RescueOutcome outcome)
    {
        phase = RescuePhase.None;
        var callback = onComplete;
        onComplete = null;
        callback?.Invoke(outcome);
    }
}
