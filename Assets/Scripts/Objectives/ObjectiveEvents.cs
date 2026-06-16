using UnityEngine;
using System;

public static class ObjectiveEvents
{
    public static event Action<string> OnAnyObjectiveUpdated;

    public static void Raise(string id)
    {
        OnAnyObjectiveUpdated?.Invoke(id);
    }
}

