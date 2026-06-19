using UnityEngine;
using System;

public static class ObjectiveEvents
{
    public static event Action<string> OnAnyObjectiveUpdated;
    public static event Action<ObjectiveData> OnObjectiveCompleted; // new

    public static void Raise(string id)
    {
        OnAnyObjectiveUpdated?.Invoke(id);
    }

    public static void RaiseCompleted(ObjectiveData obj)
    => OnObjectiveCompleted?.Invoke(obj); // new
}

