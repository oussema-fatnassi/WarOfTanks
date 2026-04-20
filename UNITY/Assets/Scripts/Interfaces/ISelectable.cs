using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public interface ISelectable
{
    void SetSelected(bool selected);
    bool IsEnemy();
    Vector3 GetWorldPosition();
}
