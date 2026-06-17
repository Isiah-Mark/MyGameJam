using UnityEngine;

[CreateAssetMenu(fileName = "DayConfig", menuName = "Days/Day Config")]
public class DayConfig : ScriptableObject
{
    public int dayNumber;
    public int swimmerCount;
    public float dayDuration = 180f; // 3 minutes
}