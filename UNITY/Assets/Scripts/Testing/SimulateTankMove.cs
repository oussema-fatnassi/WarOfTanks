using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateTankMove : MonoBehaviour
{
    [SerializeField] private TankController _tankController;

    private Vector2 _moveTo = new Vector2(10f, 0f);
    private float _timeToReachDestination;
    private float _timer;

    private bool _status = true;
    // Start is called before the first frame update
    void Start()
    {   _timeToReachDestination = CalculateTimeToReachDestination();
        _tankController.Move(_moveTo);
        _tankController.Rotate(25f);
        _timer = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        if (!_status) { return; }

        if (Vector2.Distance(_tankController.transform.position, _moveTo) < 0.1f)
        {
            _tankController.Stop();
            Debug.Log("Tank reached the destination.");
            _status = false;
        }
        if (Time.time - _timer > _timeToReachDestination + 1f)
        {
            Debug.LogWarning("Tank did not reach the destination within the expected time.");
            _tankController.Stop();
            _status = false;
        }
    }

    private float CalculateTimeToReachDestination()
    {
        float distance = Vector2.Distance(_tankController.transform.position, _moveTo);
        return distance / (_tankController.MoveSpeed * Time.fixedDeltaTime);
    }
}
