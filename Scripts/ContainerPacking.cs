using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContainerPacking : MonoBehaviour
{
    public struct PackingResult
    {
        public bool isPacked;
        public Vector3 position;
        public Quaternion rotation;

        public PackingResult(Vector3 position, Quaternion rotation)
        {
            isPacked = true;
            this.position = position;
            this.rotation = rotation;
        }
    }

    private static float GetVolume(Vector3 size)
    {
        return size.x * size.y * size.z;
    }

    private static Vector3 GetColliderSize(BoxCollider collider)
    {
        if (collider == null) {
            Debug.LogError("BoxCollider is null!");
        }
        //float x = (collider.size.x * collider.transform.localScale.x);
        //float y = (collider.size.y * collider.transform.localScale.y);
        //float z = (collider.size.z * collider.transform.localScale.z);
        return Vector3.Scale(collider.size, collider.transform.lossyScale);
    }

    private static List<Vector3> GetCollidersSizeList(List<BoxCollider> colliders)
    {
        List<Vector3> result = new List<Vector3>();
        foreach (BoxCollider collider in colliders) {
            result.Add(GetColliderSize(collider));
        }
        return result;
    }

    public static List<PackingResult> Pack(BoxCollider containerCollider, List<BoxCollider> itemColliders)
    {
        return Pack(GetColliderSize(containerCollider), GetCollidersSizeList(itemColliders));
    }

    class PackingSpace
    {
        public Vector3 position;
        public Vector3 size;
        public float volume;

        public PackingSpace(Vector3 position, Vector3 size)
        {
            this.position = position;
            this.size = size;
            volume = GetVolume(size);
        }
    }

    class PackingItem {
        public int ID;
        public Vector3 size;
        public float volume;

        public PackingItem(int ID, Vector3 size)
        {
            this.ID = ID;
            this.size = size;
            volume = GetVolume(size);
        }
    }


    private static List<PackingResult> Pack(Vector3 containerSize, List<Vector3> itemSizes)
    {
        List<PackingSpace> packingSpaces = new List<PackingSpace>();
        List<PackingItem> items = new List<PackingItem>();


        packingSpaces.Add(new PackingSpace(Vector3.zero, containerSize));

        for(int i = 0; i < itemSizes.Count; i++) {
            items.Add(new PackingItem(i, itemSizes[i]));
        }

        return Pack(packingSpaces, items);
    }

    private static int CompareVolumes(PackingItem a, PackingItem b)
    {
        if (a.volume == b.volume)
            return 0;
        if (a.volume < b.volume)
            return 1;
        return -1;
    }

    private static List<PackingResult> Pack(List<PackingSpace> packingSpaces, List<PackingItem> items)
    {
        List<PackingResult> result = new List<PackingResult>();

        for(int i = 0; i < items.Count; i++) {
            result.Add(new PackingResult());
        }

        items.Sort((a, b) => CompareVolumes(a,b));
        while (items.Count > 0) {
            GetBestFit(packingSpaces, items, out PackingSpace bestPackingSpace, out PackingItem bestItem, out bool rotate);
            if(bestPackingSpace == null ||  bestItem == null) {
                //Debug.Log("Did not found fit; Items left: " + items.Count);
                break;
            }
            UpdatePackingSpaces(packingSpaces, bestPackingSpace, bestItem, rotate);

            Quaternion rotation = Quaternion.identity;
            Vector3 offset = new Vector3(bestItem.size.x, 0, bestItem.size.z) / 2;

            if (rotate) {
                rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                offset = new Vector3(bestItem.size.z, 0, bestItem.size.x) / 2;
            }
            result[bestItem.ID] = new PackingResult(bestPackingSpace.position + offset, rotation);
            items.Remove(bestItem);

            // Mo¿na usun¹æ PackingSpaces które s¹ za ma³e na jakikolwiek item

        }

        return result;
    }

    private static bool CanFit(PackingSpace packingSpace, PackingItem item, bool rotated)
    {
        if (!rotated)
            return packingSpace.size.x >= item.size.x && packingSpace.size.y >= item.size.y && packingSpace.size.z >= item.size.z;
        return packingSpace.size.x >= item.size.z && packingSpace.size.y >= item.size.y && packingSpace.size.z >= item.size.x;
    }

    private static void GetBestFit(List<PackingSpace> packingSpaces, List<PackingItem> items, out PackingSpace resultPackingSpace, out PackingItem resultPackingItem, out bool rotate)
    {
        rotate = false;
        foreach (PackingItem item in items) {
            resultPackingSpace = null;
            float minLeft = float.MaxValue;
            foreach (PackingSpace space in packingSpaces) {
                if(CanFit(space, item, false)) {
                    float xLeft = space.size.x - item.size.x;
                    float zLeft = space.size.z - item.size.z;

                    if (minLeft > Mathf.Min(xLeft, zLeft)) {
                        resultPackingSpace = space;
                        minLeft = Mathf.Min(xLeft, zLeft);
                        rotate = false;
                    }
                }
                if (CanFit(space, item, true)) {
                    float xLeft = space.size.x - item.size.z;
                    float zLeft = space.size.z - item.size.x;

                    if (minLeft > Mathf.Min(xLeft, zLeft)) {
                        resultPackingSpace = space;
                        minLeft = Mathf.Min(xLeft, zLeft);
                        rotate = true;
                    }
                }
            }

            if (resultPackingSpace != null) {
                resultPackingItem = item;
                return;
            }
        }

        resultPackingSpace = null;
        resultPackingItem = null;
    }

    private static void UpdatePackingSpaces(List<PackingSpace> packingSpaces, PackingSpace selectedPackingSpace, PackingItem selectedItem, bool rotate)
    {
        packingSpaces.Remove(selectedPackingSpace);

        Vector3 itemSize =selectedItem.size;
        if(rotate)
            itemSize = new Vector3(selectedItem.size.z, selectedItem.size.y, selectedItem.size.x);

        // Packing space ON item
        Vector3 position = selectedPackingSpace.position + new Vector3(0, itemSize.y, 0);
        Vector3 size = new Vector3(itemSize.x, selectedPackingSpace.size.y - itemSize.y, itemSize.z);
        packingSpaces.Add(new PackingSpace(position, size));

        // Mo¿na w miarê ³atwo dodaæ ³¹czenie dwóch PackingSpaces obok siebie jeœli maj¹ dotykaj¹cy siê bok tej samej d³ugoœci (chyba najlepiej sprawdzaæ tylko dla tego powy¿szego)

        float additionalSpacing = 0.01f;

        Vector3 position1 = selectedPackingSpace.position + new Vector3(itemSize.x + additionalSpacing, 0, 0);
        Vector3 position2 = selectedPackingSpace.position + new Vector3(0, 0, itemSize.z + additionalSpacing);

        float area1 = (selectedPackingSpace.size.x - itemSize.x) * selectedPackingSpace.size.z;
        float area2 = (selectedPackingSpace.size.z - itemSize.z) * selectedPackingSpace.size.x;

        Vector3 size1;
        Vector3 size2;

        if (area1 > area2) {
            size1 = selectedPackingSpace.size - new Vector3(itemSize.x + additionalSpacing, 0, 0);
            size2 = new Vector3(itemSize.x, selectedPackingSpace.size.y, selectedPackingSpace.size.z - itemSize.z - additionalSpacing);

        }
        else {
            size1 = new Vector3(selectedPackingSpace.size.x - itemSize.x - additionalSpacing, selectedPackingSpace.size.y, itemSize.z);
            size2 = selectedPackingSpace.size - new Vector3(0, 0, itemSize.z + additionalSpacing);
        }
        packingSpaces.Add(new PackingSpace(position1, size1));
        packingSpaces.Add(new PackingSpace(position2, size2));
    }
}
