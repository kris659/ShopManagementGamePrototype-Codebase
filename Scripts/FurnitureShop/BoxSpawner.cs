using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public Transform spawnPos;
    public BuildingSO buildingSO;
    void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            FurnitureBox newBox = new FurnitureBox(buildingSO, spawnPos.position, Quaternion.Euler(0, 0, 0));
            //Product newBox = new Product(16, true, spawnPos.position, Quaternion.Euler(0, 0, 0));
        }
    }
}
