using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    void Start();
    void Tick();
    void Cancel();
}
