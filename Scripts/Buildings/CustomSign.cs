using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomSign : Decoration, IInteractable
{
    public string InteractionText => "F - Change text";
    public int InteractionTextSize => 60;

    [SerializeField] TMP_Text signText;

    private void Awake()
    {
        GetComponentInChildren<InteractableCollider>().Init(this);
    }

    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }

    public void OnPlayerButtonInteract()
    {
        UIManager.inputFieldUI.OpenUI("Enter text:", signText.text, "", (string x) => { return false;}, OnTextChanged);
    }

    private void OnTextChanged(string newText)
    {
        signText.text = newText;
    }

    public override int[] GetAdditionalSaveData()
    {
        return signText.text.ToIntArray();
    }

    public override void LoadAdditionalSaveData(int[] additionalSaveData)
    {
        signText.text =  additionalSaveData.IntToString();  
    }
}
