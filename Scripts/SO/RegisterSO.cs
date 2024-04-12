using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Register", menuName = "ScriptableObjects/Register")]
public class RegisterSO : ScriptableObject, IBuildableSO
{
    public string Name { get { return _name; } }
    [SerializeField] private string _name;

    public GameObject Prefab { get { return _prefab; } }
    [SerializeField] private GameObject _prefab;
    public GameObject PreviewPrefab { get { return _previewPrefab; } }
    [SerializeField] private GameObject _previewPrefab;



    public Sprite icon;

    public int Price { get { return _price; } }
    [SerializeField] private int _price;
}
