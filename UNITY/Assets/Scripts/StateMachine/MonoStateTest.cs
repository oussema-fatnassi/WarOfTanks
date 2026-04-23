using UnityEngine;
using UnityEngine.InputSystem;
using WarOfTanks.StateMachine;

public class MonoStateTest : MonoBehaviour
{
    [SerializeField] private bool _showDebugLogs = false;

    StateMachine<MonoStateTest> _sm;
    public bool ShowDebugLogs => _showDebugLogs;

    private void Awake()
    {
        _sm = new StateMachine<MonoStateTest>(this, new StateA());
    }

    private void Update()
    {
        _sm.Update();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _sm.ChangeState(new StateB());
        }
    }
}
