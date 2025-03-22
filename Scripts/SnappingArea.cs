using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingArea : PlacingTriggerArea
{
    [SerializeField] private List<ProductSO> productTypes = new();
    private Transform[] positions;

    private void Awake()
    {
        positions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) {
            positions[i] = transform.GetChild(i).GetComponent<Transform>();
        }
    }

    public bool CanPlacePickable(IPickable pickable)
    {
        if(pickable.PickableTypeID < 0 || pickable.PickableTypeID >= 999)
            return false;
        //Debug.Log(CanPlaceProduct(SOData.productsList[pickable.PickableID]));
        return CanPlaceProduct(ProductsData.instance.productsSpawned[pickable.PickableID].productType);
    }

    public bool CanPlaceProduct(ProductSO productType)
    {
        return productTypes.Contains(productType);
    }
    public Transform GetSnappedTransform(Vector3 hitPosition)
    {
        Transform position = null;
        float distance =  float.PositiveInfinity;
        for (int i = 0; i < positions.Length; i++) {
            if(Vector3.Distance(hitPosition, positions[i].position) < distance) {
                distance = Vector3.Distance(hitPosition, positions[i].position);
                position = positions[i];
            }
        }        
        return position;
    }

    public override void UpdateCurrentProduct()
    {
        currentProductPlacingPositions = new List<Vector3>();
        currentProductPlacingRotations = new List<Quaternion>();
        if (!CanPlaceProduct(currentProduct))
            return;

        for (int i = 0; i < positions.Length; i++) {
            currentProductPlacingPositions.Add(positions[i].position);// + new Vector3(0, trigger.size.y * 0.5f, 0));
            currentProductPlacingRotations.Add(positions[i].rotation);// + new Vector3(0, trigger.size.y * 0.5f, 0));
        }
        currentProductCapacity = currentProductPlacingPositions.Count;
    }
}
