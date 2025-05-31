using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    [SerializeField]
    public string name;
    public int price;

    public enum ItemType
{
    Helmet,
    Armor,
    Weapon,
    Default
    
}
    public ItemType itemType;
    public Sprite icon;
    public GameObject prefab;
    public int stackMax;
    public float healthBonus;
    public float manaBonus;
    public float healthRegenBonus;
    public float manaRegenBonus;
    public float damageModifier;
    public bool spellschanged=false;
    
    
}
