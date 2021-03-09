using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinaxObj : MonoBehaviour
{
    public static TrinaxObj Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
