//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//[System.Serializable]
//public class SaveData
//{
//    public List<int> playerInventoryIDs;
//    public Dictionary<int, List<int>> containerItemIDs;
//}

//public class SaveManager : MonoBehaviour
//{
//    //public Inventory playerInventory;
//    //public List<Container> containers;

//    public void SaveGame()
//    {
//        SaveData saveData = new SaveData
//        {
//            playerInventoryIDs = new List<int>(),
//            containerItemIDs = new Dictionary<int, List<int>>()
//        };

//        // Spara spelarens inventory
//        foreach (var item in playerInventory.items)
//        {
//            saveData.playerInventoryIDs.Add(item.itemData.itemID); 
//        }

//        // Spara containrar
//        for (int i = 0; i < containers.Count; i++)
//        {
//            saveData.containerItemIDs[i] = new List<int>();
//            foreach (var itemInstance in containers[i].containerItems)
//            {
//                saveData.containerItemIDs[i].Add(itemInstance.itemData.itemID); 
//            }
//        }

//        string json = JsonUtility.ToJson(saveData, true);
//        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
//        Debug.Log("Game saved!");
//    }


//}
