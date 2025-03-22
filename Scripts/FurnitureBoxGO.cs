using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FurnitureBoxGO : MonoBehaviour, IPickableGO, IInformationDisplay
{
    public FurnitureBox furnitureBox;
    public string InformationDisplayText => SOData.buildingsList[furnitureBox.buildingSaveData.buildingIndex].name;

    public IPickable pickable => furnitureBox;

    //public SpriteRenderer productIcon;

    public bool isPhysixSpawned;
    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }

    public static FurnitureBoxGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, FurnitureBox furnitureBox, bool isColliderActive = false)
    {
        GameObject productGO;
        FurnitureBoxGO productGOScript;
        if (isOnlyVisual)
        {
            productGO = furnitureBox.PreviewGameObject; //Instantiate(product.productType.visualPrefab, position, rotation, parent);
            productGO.transform.SetParent(parent);
            productGO.transform.transform.position = position;
        }
        else
        {
            productGO = Instantiate(furnitureBox.furnitureBoxSO.prefab, position, rotation, parent);
        }

        productGOScript = productGO.GetComponent<FurnitureBoxGO>();
        productGOScript.furnitureBox = furnitureBox;
        productGOScript.isPhysixSpawned = !isOnlyVisual;

        //productGOScript.productIcon = productGO.GetComponentInChildren<SpriteRenderer>();
        //productGOScript.productIcon.sprite = SOData.buildingsList[furnitureBox.buildingSaveData.buildingIndex].Icon;
        //productGOScript.productIcon.sprite = furnitureBox.buildingSO.Icon;

        return productGOScript;
    }

    public void Init()
    {
        DOTween.Init();
    }
}