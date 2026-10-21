using UnityEngine;

public static class ItemTypeExtensions
{
    public static Color GetColor(this ItemType type)
    {
        return type switch
        {
            ItemType.RobotPart => new Color(0.31f, 0.66f, 0.87f), // #4EA8DE new Color(0.31f, 0.66f, 0.87f)
            ItemType.Tool => new Color(0.31f, 0.66f, 0.87f), // #E09F3E new Color(0.88f, 0.62f, 0.24f)
            ItemType.Document => new Color(0.83f, 0.85f, 0.87f), // #D4D8DD
            ItemType.Consumable => new Color(0.32f, 0.72f, 0.53f), // #52B788
            ItemType.KeyItem => new Color(0.78f, 0.49f, 1.00f), // #C77DFF
            ItemType.Weapon => new Color(0.90f, 0.22f, 0.27f), // #E63946
            ItemType.Currency => new Color(1.00f, 0.82f, 0.40f), // #FFD166
            _ => Color.white
        };
    }

    public static string GetHexColor(this ItemType type)
    {
        return type switch 
        {
            ItemType.RobotPart => "4EA8DE", // 4EA8DE
            ItemType.Tool => "4EA8DE", // E09F3E
            ItemType.Document => "D4D8DD",
            ItemType.Consumable => "52B788",
            ItemType.KeyItem => "C77DFF",
            ItemType.Weapon => "E63946",
            ItemType.Currency => "FFD166",
            _ => "FFFFFF"
        };
    }
}
