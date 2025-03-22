using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Container", menuName = "ScriptableObjects/Container")]
public class ContainerSO : ScriptableObject, IListable
{
    public string Name { get { return _name; } }
    [SerializeField] private string _name;

    public GameObject prefab;

    public Vector3 offset;
    public Sprite Icon { get { return _icon; } }
    [SerializeField] private Sprite _icon;

    public int Price { get { return _price; } }
    [SerializeField] private int _price;
}
