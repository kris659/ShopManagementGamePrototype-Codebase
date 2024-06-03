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

    [SerializeField] private ManageUI _manageUI;
    public static ManageUI manageUI;

    [SerializeField] private ConfirmUI _confirmUI;
    public static ConfirmUI confirmUI;

    [SerializeField] private LandUnlockUI _landUnlockUI;
    public static LandUnlockUI landUnlockUI;

    [SerializeField] private LeftPanelUI _leftPanelUI;
    public static LeftPanelUI leftPanelUI;
    public static bool BlockInput => leftPanelUI.blockInput;

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
        manageUI = _manageUI;
        confirmUI = _confirmUI;
        landUnlockUI = _landUnlockUI;
        leftPanelUI = _leftPanelUI;
    }
}
