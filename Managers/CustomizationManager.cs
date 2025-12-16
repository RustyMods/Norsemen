using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public static class CustomizationManager
{
    public static readonly List<string> beards = new();
    public static readonly List<string> hairs = new();
    
    public static readonly List<Color> hairColors = new List<Color>()
    {
        Color.black,                                    // Black
        new (0.98f, 0.94f, 0.75f, 1f),                 // Platinum Blonde
        new (0.63f, 0.36f, 0f, 1f),                    // Brown
        new (0.15f, 0.08f, 0.05f, 1f),                 // Dark Brown
        new (0.35f, 0.25f, 0.15f, 1f),                 // Medium Brown
        new (0.55f, 0.45f, 0.35f, 1f),                 // Light Brown
        new (0.95f, 0.87f, 0.51f, 1f),                 // Golden Blonde
        new (0.85f, 0.75f, 0.45f, 1f),                 // Dirty Blonde
        new (0.72f, 0.65f, 0.52f, 1f),                 // Sandy Blonde
        new (0.45f, 0.18f, 0.08f, 1f),                 // Auburn
        new (0.55f, 0.12f, 0.05f, 1f),                 // Dark Red
        new (0.72f, 0.25f, 0.12f, 1f),                 // Ginger
        new (0.40f, 0.40f, 0.40f, 1f),                 // Dark Gray
        new (0.65f, 0.65f, 0.65f, 1f),                 // Silver Gray
        new (0.88f, 0.88f, 0.90f, 1f),                 // White/Silver
        new (0.25f, 0.25f, 0.28f, 1f),                 // Charcoal
    };
    
    public static readonly List<Color> skinColors = new List<Color>()
    {
        new (1f, 1f, 1f, 1f),                          // Base/Pale (no tint)
        new (1f, 0.95f, 0.92f, 1f),                    // Very Fair
        new (1f, 0.92f, 0.86f, 1f),                    // Fair with Pink undertone
        new (1f, 0.94f, 0.88f, 1f),                    // Fair
        new (1f, 0.90f, 0.82f, 1f),                    // Light
        new (1f, 0.88f, 0.78f, 1f),                    // Light Medium
        new (0.98f, 0.85f, 0.72f, 1f),                 // Medium
        new (0.95f, 0.82f, 0.68f, 1f),                 // Medium Tan
        new (0.92f, 0.78f, 0.62f, 1f),                 // Tan
        new (0.88f, 0.72f, 0.56f, 1f),                 // Deep Tan
        new (0.85f, 0.68f, 0.52f, 1f),                 // Light Brown
        new (0.78f, 0.62f, 0.48f, 1f),                 // Medium Brown
        new (1f, 0.88f, 0.75f, 1f),                    // Warm Beige
        new (0.98f, 0.90f, 0.80f, 1f),                 // Peachy Fair
        new (0.95f, 0.86f, 0.76f, 1f),                 // Golden Light
    };

    public static Color GetRandomSkinColor()
    {
        int index = UnityEngine.Random.Range(0, skinColors.Count);
        return skinColors[index];
    }

    public static void GetHairAndBeards(ObjectDB db)
    {
        foreach (GameObject? prefab in db.m_items)
        {
            ItemDrop component = prefab.GetComponent<ItemDrop>();
            if (component.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Customization) continue;
            if (prefab.name.CustomStartsWith("Beard"))
            {
                beards.Add(prefab.name);
            }
            else if (prefab.name.CustomStartsWith("Hair"))
            {
                hairs.Add(prefab.name);
            }
        }

        beards.RemoveAll(x => x.Contains("_"));
        hairs.RemoveAll(x => x.Contains("_"));
    }
}