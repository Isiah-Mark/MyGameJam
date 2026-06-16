using UnityEngine;

[CreateAssetMenu(fileName = "LifeguardData", menuName = "Lifeguards/Lifeguard Data")]
public class LifeguardData : ScriptableObject
{
    public string lifeguardName;
    public float speed;
    public Color color;
}