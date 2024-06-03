using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "ScriptableObjects/Product")]
public class ProductSO : ScriptableObject, IListable
{
    public string Name { get { return _name; } }
    [SerializeField] private string _name;

    public GameObject prefab;
    public GameObject visualPrefab;

    public Vector3 offset;
    public Sprite icon;

    public int Price { get { return _price; } }
    [SerializeField] private int _price;
    
    public int holdingLimit = 1;

    public bool canBePlacedOnShelf = true;
    public bool isContainer = false;
    public bool canBeSold = true;
}
