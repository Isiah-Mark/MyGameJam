using UnityEngine;

[CreateAssetMenu(menuName = "Lifeguard/Objective")]
public class ObjectiveData : ScriptableObject
{
    public string id;           // e.g. "save_swimmers"
    public string label;        // e.g. "Save 2 drowning swimmers"
    public ObjectiveCategory category; // from class created
    public int goal;            // target number
    [HideInInspector] public int current; // runtime progress, hidden in inspector

    public bool IsComplete => current >= goal;

    public void Increment()
    {
        if (IsComplete) return;
        current++;
        ObjectiveEvents.Raise(id);
    }
}
