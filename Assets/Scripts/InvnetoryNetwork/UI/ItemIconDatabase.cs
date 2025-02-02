using System.Collections.Generic;
using UnityEngine;

public class ItemIconDatabase : MonoBehaviour
{
    public static ItemIconDatabase Instance;

    [System.Serializable]
    public struct ItemIconEntry
    {
        public string itemName;
        public Sprite icon;
    }

    [Header("All itmes and there names")]
    public List<ItemIconEntry> entries;

    private Dictionary<string, Sprite> iconDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        iconDictionary = new Dictionary<string, Sprite>();
        foreach (var entry in entries)
        {
            if (!iconDictionary.ContainsKey(entry.itemName))
                iconDictionary.Add(entry.itemName, entry.icon);
        }
    }

    public Sprite GetSpriteForItem(string itemName)
    {
        if (iconDictionary.TryGetValue(itemName, out Sprite sprite))
            return sprite;
        return null;
    }
}
