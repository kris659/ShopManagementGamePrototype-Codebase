using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOData : MonoBehaviour
{
    [SerializeField] private ProductSO[] _productsList;
    public static ProductSO[] productsList;

    [SerializeField] private ContainerSO[] _containersList;
    public static ContainerSO[] containersList;

    [SerializeField] private BuildingSO[] _buildingsList;
    public static BuildingSO[] buildingsList;

    [SerializeField] private FurnitureBoxSO[] _furnitureBoxList;
    public static FurnitureBoxSO[] furnitureBoxList;

    void Awake()
    {
        productsList = _productsList;
        containersList = _containersList;
        buildingsList = _buildingsList;
        furnitureBoxList = _furnitureBoxList;
    }    

    public static int GetProductIndex(ProductSO product)
    {
        return Array.IndexOf(productsList, product);
    }

    public static int GetContainerIndex(ContainerSO continer)
    {
        return Array.IndexOf(containersList, continer);
    }

    public static int GetBuildingIndex(BuildingSO building)
    {
        return Array.IndexOf(buildingsList, building);
    }
    public static int GetFurnitureBoxIndex(FurnitureBoxSO furnitureBox)
    {
        return Array.IndexOf(furnitureBoxList, furnitureBox);
    }
}
