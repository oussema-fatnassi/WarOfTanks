using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignletonBehaviour<T> : MonoBehaviour where T : SignletonBehaviour<T>
{
    private T _instance;
    public static T Instance { get; protected set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            throw new System.Exception("Singleton instance already exists, destroying duplicate.");
        }
        else
        {
            _instance = (T)this;
        }
    }
}
