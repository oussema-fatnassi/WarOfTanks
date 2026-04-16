using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateTankMove : MonoBehaviour
{
    [SerializeField] private TankController _tankController;

    private Vector2 _moveTo = new Vector2(5f, 0f);
    private float _timeToReachDestination;
    private float _timer;
    private int _moveAttempts = 0;

    private bool _status = true;
    void Start()
    {   _timeToReachDestination = CalculateTimeToReachDestination();
        _tankController.Move(_moveTo);
        _tankController.Rotate(1f);
        _timer = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        if (_moveAttempts > 301)
        {
            return;
        }
        _moveAttempts++;
        if (_moveAttempts > 300)
        {
            Debug.LogWarning("Too many move attempts, stopping the test.");
            _tankController.Stop();
            //_tankController.Rotate(50f);
        }
        if (!_status) { return; }

        //if (Time.time - _timer > _timeToReachDestination + 1f)
        //{
        //    Debug.LogWarning("Tank did not reach the destination within the expected time.");
        //    _tankController.Stop();
        //    _status = false;
        //}
    }

    private float CalculateTimeToReachDestination()
    {
        float distance = Vector2.Distance(_tankController.transform.position, _moveTo);
        return distance / (_tankController.MoveSpeed * Time.fixedDeltaTime);
    }
}
