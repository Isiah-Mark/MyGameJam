using System;
using UnityEngine;

public class BeachAmbiance : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static BeachAmbiance instance;

    public void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
