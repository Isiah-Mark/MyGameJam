using UnityEngine;

[CreateAssetMenu(fileName = "DayConfig", menuName = "Days/Day Config")]
public class DayConfig : ScriptableObject
{
    public int dayNumber;
    public int swimmerCount;
    public float dayDuration = 180f; // 3 minutes
    public int drownLimit = 5; // non-evil drownings allowed before the day is failed
}