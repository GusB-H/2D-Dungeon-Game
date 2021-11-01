using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewLootTable", menuName = "ScriptableObjects/LootTable", order = 3)]

public class LootTable : ScriptableObject
{
    public enum Rarity { Common, Uncommon, Rare, Wonderous };
    [System.Serializable]
    public struct Drop
    {
        public Rarity rarity;
        public int weight;
        public GameObject dropObject;
    }

    public Drop[] drops;

    public GameObject[] PullDrops(int count)
    {
        int totalWeight = 0;

        foreach(Drop drop in drops)
        {
            totalWeight += drop.weight;
        }

        GameObject[] returnObjects = new GameObject[count];

        for(int i = 0; i < count; i++)
        {
            int randomValue = Random.Range(0, totalWeight);
            GameObject pullObject = null;
            foreach(Drop drop in drops)
            {
                randomValue -= drop.weight;
                if(randomValue <= 0)
                {
                    pullObject = drop.dropObject;
                    break;
                }
            }
            returnObjects[i] = pullObject;
        }

        return returnObjects;
    }
}
