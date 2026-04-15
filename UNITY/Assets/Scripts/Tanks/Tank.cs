using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] private ETankTeam _team;

    private HealthSystem _healthSystem;

    public bool IsAlive => !_healthSystem?.IsDead ?? false;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Die()
    {
        //TODO : handle death for now just destroy the game object
        Destroy(gameObject);
    }
}
