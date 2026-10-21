using UnityEngine;

public enum ItemType
{
    RobotPart,
    Tool,
    Document,
    Consumable,
    KeyItem,
    Weapon,
    Currency
}

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string description;
    public ItemType itemType;
    public Sprite icon;
    
}
