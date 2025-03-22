using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureShopUI : WindowUI
{
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField quantityInput;

    [SerializeField] private GameObject description;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text descriptionName;

    private int selectedQuantity = 1;
    private int itemPrice;
    private string itemName;
    private System.Action<int> confirmAction;
    private System.Action cancelAction;

    internal override void Awake()
    {
        base.Awake();
        confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        cancelButton.onClick.AddListener(OnCancelButtonPressed);
        quantityInput.onValueChanged.AddListener(ValidateQuantity);
    }

    public void ChangeQuantity(int Difference)
    {
        selectedQuantity += Difference;
        quantityInput.text = selectedQuantity.ToString();
        UpdateUI();
    }

    public void OpenUI(int price, BuildingSO buildingSO, System.Action<int> confirmAction, System.Action cancelAction, bool isConfirmEnabled)
    {
        this.itemPrice = price;
        this.itemName = buildingSO.Name;
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;
        this.descriptionText.text = buildingSO.Description;
        this.descriptionName.text = this.itemName;
        selectedQuantity = 1;
        quantityInput.text = selectedQuantity.ToString();
        confirmButton.interactable = isConfirmEnabled;
        description.SetActive(false);
        UpdateUI();
        OpenUI();
    }

    public void OpenDescription()
    {
        if(description.activeSelf == false)
        {
            description.SetActive(true);
        }
        else
        {
            description.SetActive(false);
        }
    }

    private void ValidateQuantity(string value)
    {
        if (int.TryParse(value, out int quantity))
        {
            quantity = Mathf.Clamp(quantity, 0, 10);
        }
        else
        {
            quantity = 1;
        }

        selectedQuantity = quantity;
        quantityInput.text = selectedQuantity.ToString();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if(selectedQuantity == 1)
        {
            mainText.text = $"Do you want to buy {selectedQuantity} {itemName} for: ${selectedQuantity * itemPrice}?";
        }
        else
        {
            mainText.text = $"Do you want to buy {selectedQuantity} {itemName}s for: ${selectedQuantity * itemPrice}?";

        }
    }

    public override void CloseUI()
    {
        description.SetActive(false);
        base.CloseUI();
    }

    private void OnConfirmButtonPressed()
    {
        CloseUI();
        confirmAction?.Invoke(selectedQuantity);
    }

    private void OnCancelButtonPressed()
    {
        CloseUI();
        cancelAction?.Invoke();
    }
}
