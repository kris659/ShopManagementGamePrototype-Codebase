using UnityEngine;

public class PhysicsMaterialFix : MonoBehaviour
{
    // Bez tego jest bardzo du¿e tarcie

    [SerializeField] private PhysicMaterial material;
    void Awake()
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        //collider.material.frictionCombine = PhysicMaterialCombine.Minimum;
        //PhysicMaterial material = new PhysicMaterial();
        //material.dynamicFriction = 0;
        //material.staticFriction = 0;
        //material.frictionCombine = PhysicMaterialCombine.Minimum;
        collider.material = material;
    }

    private void Start()
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = true;
    }
}
