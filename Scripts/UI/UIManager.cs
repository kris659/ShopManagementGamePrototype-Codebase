using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextUI _textUI;
    public static TextUI textUI;

    [SerializeField] private MoneyUI _moneyUI;
    public static MoneyUI moneyUI;

    [SerializeField] private OrdersUI _ordersUI;
    public static OrdersUI ordersUI;

    [SerializeField] private SavingUI _savingUI;
    public static SavingUI savingUI;

    [SerializeField] private BuildingUI _buildingUI;
    public static BuildingUI buildingUI;

    [SerializeField] private HoldingUI _holdingUI;
    public static HoldingUI holdingUI;

    [SerializeField] private TimeUI _timeUI;
    public static TimeUI timeUI;

    [SerializeField] private PossibleActionsUI _possibleActionsUI;
    public static PossibleActionsUI possibleActionsUI;

    void Awake()
    {
        textUI = _textUI;
        moneyUI = _moneyUI;
        ordersUI = _ordersUI;
        savingUI = _savingUI;
        buildingUI = _buildingUI;
        holdingUI = _holdingUI;
        timeUI = _timeUI;
        possibleActionsUI = _possibleActionsUI;
    }
}
