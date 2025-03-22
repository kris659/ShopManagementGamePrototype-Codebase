using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Facility : Decoration
{
    public Transform customerDestination;
    public bool isFree = true;
    public Customer myCustomer;

    public override void Build()
    {
        base.Build();
        ShopData.instance.AddFacility(this);
    }
    public virtual void UseFacility(){}
    public virtual void ExitFacility() 
    {
        myCustomer = null;
        isFree = true;
    }

    private void OnDestroy()
    {
        ShopData.instance.RemoveFacility(this);
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
