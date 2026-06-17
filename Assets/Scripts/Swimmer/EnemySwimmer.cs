using UnityEngine;

public class EnemySwimmer : Swimmer
{
    [Header("Enemy Specifics")]
    public static int caughtLifeguardsCount = 0; // Global score tracker

    // We use 'new' to hijack the property the Lifeguard sets upon arrival
    public new bool BeingRescued
    {
        get => false; // Keep returning false internally so our Update loops stay alive
        set
        {
            if (value == true)
            {
                SpringTheTrap();
            }
        }
    }

    private void SpringTheTrap()
    {
        // 1. Find the lifeguard that just arrived to grab us
        Lifeguard[] lifeguards = FindObjectsOfType<Lifeguard>();
        foreach (Lifeguard lg in lifeguards)
        {
            // If the lifeguard is right on top of us trying to rescue us
            if (Vector2.Distance(transform.position, lg.transform.position) < 2.5f)
            {
                // 2. Increment score and log it
                caughtLifeguardsCount++;
                Debug.Log($"Trap sprung! Lifeguard eliminated. Total caught: {caughtLifeguardsCount}");

                // 3. Destroy the lifeguard asset instantly
                Destroy(lg.gameObject);
                
                // 4. Recover instantly from drowning back to normal swimming state
                ResetToHealthySwimmer();
                break;
            }
        }
    }

    private void ResetToHealthySwimmer()
    {
        // Reset our state flags (enabled via the protected keyword)
        isDrowning = false;
        isSinking = false;
        isIdle = false;
        
        // Reset the scale back to full size just in case sinking started shrinking us
        transform.localScale = baseScale;

        // Reset the drowning clock so we wander normally before acting up again
        drownTimer = Random.Range(minTimeUntilDrown, maxTimeUntilDrown);

        // Clear the assignment flags so the click engine allows future interactions
        // We use Reflection here since these specific backing fields remain private in Swimmer
        var type = typeof(Swimmer);
        var rescueAssignedField = type.GetField("_rescueAssigned", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (rescueAssignedField != null)
        {
            rescueAssignedField.SetValue(this, false);
        }

        // Return animation state back to regular treading/swimming water loop
        // (ANIM_TREADING = 0)
        GetComponentInChildren<Animator>()?.SetInteger("State", 0);

        // Instantly pick a new position and swim away safely
        // We call this via reflection since PickNewTarget is private in the base class
        var pickTargetMethod = type.GetMethod("PickNewTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        pickTargetMethod?.Invoke(this, null);
    }
}