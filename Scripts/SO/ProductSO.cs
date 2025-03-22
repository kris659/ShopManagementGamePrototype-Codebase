using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "ScriptableObjects/Product")]
public class ProductSO : ScriptableObject, IListable
{
    public string Name { get { return _name; } }
    [SerializeField] private string _name;

    public GameObject prefab;
    //public GameObject visualPrefab;

    public Vector3 holdingPosition;
    public Vector3 holdingRotation;

    public Sprite Icon { get { return _icon; } }
    [SerializeField] private Sprite _icon;

    public int Price { get { return _price; } }
    [SerializeField] private int _price;

    public int Popularity { get { return _popularity; } }
    [SerializeField] private int _popularity;

    public ContainerSO containerType;

    public int holdingLimit = 1;

}
