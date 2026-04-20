using UnityEngine;

public interface ISelectable
{
    void SetSelected(bool selected);
    bool IsEnemy();
    Vector3 GetWorldPosition();
}
