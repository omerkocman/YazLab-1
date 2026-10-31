using UnityEngine;

public enum RobotObjectType
{
    Door,
    Switch
}

public class RobotDetectable : MonoBehaviour
{
    [SerializeField] private RobotObjectType objectType;

    public RobotObjectType Type => objectType;
}
