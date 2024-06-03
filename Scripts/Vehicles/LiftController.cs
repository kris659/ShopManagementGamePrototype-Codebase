using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    [SerializeField] Transform lift;
    [SerializeField] Transform liftVisual;
    [SerializeField] float liftSpeed;
    [SerializeField] float maxHeight;
    [SerializeField] float minHeight;
    float currentHeight { get; set; } = 2.4f;

    private void Awake()
    {
        CarPhysx carPhysx = GetComponent<CarPhysx>();
        carPhysx.OnVehicleEnter += () => { 
            UIManager.possibleActionsUI.AddAction("Q - move lift up");
            UIManager.possibleActionsUI.AddAction("E - move lift down");
        };
        carPhysx.OnVehicleLeave += () => { 
            UIManager.possibleActionsUI.RemoveAction("Q - move lift up");         
            UIManager.possibleActionsUI.RemoveAction("E - move lift down");
        };
    }

    public void HandleInput(bool buttonUp, bool buttonDown){
        if (buttonDown == buttonUp) return;

        if (buttonUp && currentHeight < maxHeight) currentHeight += Time.deltaTime * liftSpeed;
        if (buttonDown && currentHeight > minHeight) currentHeight -= Time.deltaTime * liftSpeed;

        lift.transform.localPosition = new Vector3(lift.transform.localPosition.x, currentHeight, lift.transform.localPosition.z);
        liftVisual.transform.localPosition = lift.transform.localPosition;
    }
}
