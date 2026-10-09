using UnityEngine;

public interface IDoorMovementStrategy
{
    void Animate(Transform doorMesh, bool isOpen, float speed);
    void ResetState();
}
