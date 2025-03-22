using UnityEngine;

public class Purchasable : MonoBehaviour, IInteractable, IInformationDisplay
{
    public string InteractionText => "F - Buy";
    public int InteractionTextSize => 60;

    public BuildingSO buildingSO;

    public string InformationDisplayText => buildingSO.Name;

    public void OnPlayerButtonInteract()
    {
        int price = buildingSO.Price;
        UIManager.furnitureShopUI.OpenUI(
            price,
            buildingSO,
            (quantity) => OnConfirmPurchase(quantity),
            null,
            PlayerData.instance.CanAfford(price)
        );
    }

    private void OnConfirmPurchase(int number)
    {
        for (int i = 0; i < number; i++)
        {
            if (PlayerData.instance.CanAfford(buildingSO.Price))
            {
                PlayerData.instance.TakeMoney(buildingSO.Price);
                FurnitureShop.SpawnFurnitureBox(buildingSO);
            }
            else
            {
                Debug.Log("Not enough money!");
                break;
            }
        }
    }

    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }
}
