using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureShop : MonoBehaviour
{
    [SerializeField] private Transform _magazineSpawn;
    public static Transform magazineSpawn;

    private void Awake()
    {
        magazineSpawn = _magazineSpawn;
    }

    public static void SpawnFurnitureBox(BuildingSO buildingSO)
    {
        FurnitureBox newBox = new FurnitureBox(buildingSO, magazineSpawn.position, magazineSpawn.rotation);
    }
}