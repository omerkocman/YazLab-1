using UnityEngine;

public enum ItemType
{
    Currency,
    KeyItem,
    Consumable,
    Tool,
    Gun
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] public string Id;
    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public ItemType ItemType;
    [SerializeField] public Sprite Icon;
    [SerializeField] public bool isStackable = false;
    [SerializeField, Range(1,128)] public int maxStackSize = 1;


}
