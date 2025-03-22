using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "ScriptableObjects/Buildable")]
public class BuildingSO : ScriptableObject, IListable
{
    public string Name { get { return _name; } }
    [SerializeField] private string _name;
    public string Description { get { return _description; } }
    [SerializeField] private string _description;

    public GameObject Prefab { get { return _prefab; } }
    [SerializeField] private GameObject _prefab;

    public Sprite Icon { get { return _icon; } }
    [SerializeField] private Sprite _icon;

    public int Price { get { return _price; } }
    [SerializeField] private int _price;

    public FurnitureBoxSO furnitureBoxSO;
}