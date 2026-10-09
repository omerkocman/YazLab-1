using UnityEngine;

public class ItemBase : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string itemName;

    [TextArea]
    public string description;
}
