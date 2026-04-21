using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; protected set; }
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            throw new System.Exception("Singleton instance already exists, destroying duplicate.");
        }
        else
        {
            Instance = (T)this;
        }
    }
}
