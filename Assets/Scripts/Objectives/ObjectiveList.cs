using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lifeguard/Objective List")]
public class ObjectiveList : ScriptableObject
{
    public int day;
    public List<ObjectiveData> objectives;
}