using UnityEngine;
using UnityEngine.InputSystem;
using WarOfTanks.StateMachine;

public class MonoStateTest : MonoBehaviour
{
    StateMachine<MonoStateTest> _sm;

    private void Awake()
    {
        _sm = new StateMachine<MonoStateTest>(this);
    }

    private void Start()
    {
        _sm.SetState(new StateA());
    }

    private void Update()
    {
        _sm.Update();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _sm.SetState(new StateB());
        }
    }
}
        