using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] private ETankTeam _team;
    [SerializeField] private GameObject _tankBody;

    private bool _isAlive = true;
    private HealthSystem _healthSystem;
    public ETankTeam Team => _team;
    public bool IsAlive => _isAlive;

    // Start is called before the first frame update
    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDeath += Die;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Die()
    {
        _isAlive = false;
        _tankBody.SetActive(false);
    }
}
