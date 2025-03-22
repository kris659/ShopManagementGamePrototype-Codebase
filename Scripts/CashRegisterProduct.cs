using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class CashRegisterProduct : MonoBehaviour, IInteractable
{
    public string InteractionText { get { 
            if(CanPlayerInteract())
                return "LMB - scan product";
            return "";
        } }
    public int InteractionTextSize => 60;


    private CashRegister cashRegister;
    private Product product;
    public void Init(CashRegister cashRegister, Product product)
    {
        this.cashRegister = cashRegister;
        this.product = product;
        product.productGO.gameObject.layer = LayerMask.NameToLayer("IInteractable");
        product.productGO.transform.GetComponentsInChildren<Collider>().ToList().ForEach((col) => {col.enabled = true; });
        transform.localEulerAngles = Vector3.zero;
    }

    private bool CanPlayerInteract()
    {
        return cashRegister.isPlayer && !cashRegister.isWorker;
    }

    public void OnPlayerButtonInteract() { }
    public void OnMouseButtoDown() {
        if (!CanPlayerInteract())
            return;
        cashRegister.PlayerRemoveProduct(product);
    }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }
}
