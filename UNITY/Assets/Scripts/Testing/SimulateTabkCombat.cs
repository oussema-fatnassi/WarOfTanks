using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateTabkCombat : MonoBehaviour
{
    [SerializeField] private TurretController _playerTurretController;
    [SerializeField] private TurretController _enemyTurretController;

    private Vector3 _positionRegistered;
    // Start is called before the first frame update
    void Start()
    {
        _positionRegistered = _enemyTurretController.transform.position;
        _playerTurretController.RotateTo(_enemyTurretController.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        _positionRegistered = _enemyTurretController.transform.position;
        _playerTurretController.RotateTo(_enemyTurretController.transform.position);
        _playerTurretController.Fire();
    }
}
