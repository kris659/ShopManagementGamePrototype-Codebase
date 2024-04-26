using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SOData : MonoBehaviour
{
    [SerializeField] private ProductSO[] _productsList;
    public static ProductSO[] productsList;

    [SerializeField] private ContainerSO[] _containersList;
    public static ContainerSO[] containersList;

    public static List<ProductSO> sellableProductsList = new List<ProductSO>();

    [SerializeField] private ShelfSO[] _shelvesList;
    public static ShelfSO[] shelvesList;

    [SerializeField] private RegisterSO[] _registersList;
    public static RegisterSO[] registersList;

    [SerializeField] private WallSO[] _wallsList;
    public static WallSO[] wallsList;

    public enum BuildingTypes
    {
        Shelf,
        Register,
        Wall,
        Decoration,
    }

    private static string[] buildingTypesNames = {"Shelves", "Registers", "Walls", "Decorations"};

    public static BuildingTypes[] buildingTypesOrder;
    [SerializeField] private BuildingTypes[] _buildingTypesOrder;

    void Awake()
    {
        productsList = _productsList;
        containersList = _containersList;
        shelvesList = _shelvesList;
        registersList = _registersList;
        wallsList = _wallsList;
        buildingTypesOrder = _buildingTypesOrder;

        for(int i = 0; i < productsList.Length; i++) {
            if (productsList[i].canBeSold)
                sellableProductsList.Add(productsList[i]);
        }
    }

    public static int GetProductIndex(ProductSO product)
    {
        return Array.IndexOf(productsList, product);
    }

    public static IBuildableSO GetBuildableSO(int buildingTypeIndex, int buildingIndex)
    {
        Debug.Log(buildingTypeIndex + "  " + buildingIndex);
        int index = (int)buildingTypesOrder[buildingTypeIndex];

        return index switch {
            0 => shelvesList[buildingIndex],
            1 => registersList[buildingIndex],
            2 => wallsList[buildingIndex],
            _ => null,
        };
    }

    public static IListable[] GetListables(int buildingTypeIndex)
    {
        int index = (int)buildingTypesOrder[buildingTypeIndex];

        return index switch {
            0 => shelvesList,
            1 => registersList,
            2 => wallsList,
            _ => new IListable[0],
        };
    }

    public static string[] GetBuildingTypesNames()
    {
        string[] names = new string[buildingTypesNames.Length];

        for(int i = 0; i < names.Length; i++) {
            int index = (int)buildingTypesOrder[i];
            names[i] = buildingTypesNames[index];
        }
        return names;
    }
}
