using UnityEngine;

[DisallowMultipleComponent]
public class Lock : MonoBehaviour
{
    [SerializeField] private bool isLocked = true;

    public bool IsLocked => isLocked;

    public void Unlock()
    {
        isLocked = false;
    }
}
