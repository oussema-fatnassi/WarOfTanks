using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateTabkCombat : MonoBehaviour
{
    [SerializeField] private TurretController _playerTurretController;
    [SerializeField] private TurretController _enemyTurretController;
    // Start is called before the first frame update
    void Start()
    {
        _playerTurretController.RotateTo(_enemyTurretController.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        _playerTurretController.Fire(true);
    }
}
