using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextUI _textUI;
    public static TextUI textUI;

    [SerializeField] private MoneyUI _moneyUI;
    public static MoneyUI moneyUI;

    [SerializeField] private OrdersUI _ordersUI;
    public static OrdersUI ordersUI;

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

    [SerializeField] private NextDayUI _nextDayUI;
    public static NextDayUI nextDayUI;

    [SerializeField] private SettingsUI _settingsUI;
    public static SettingsUI settingsUI;

    [SerializeField] private WarningsUI _warningsUI;
    public static WarningsUI warningsUI;

    [SerializeField] private InfoUI _infoUI;
    public static InfoUI infoUI;

    [SerializeField] private TasksUI _tasksUI;
    public static TasksUI tasksUI;

    [SerializeField] private LicencesUI _licencesUI;
    public static LicencesUI licencesUI;

    [SerializeField] private InputFieldUI _inputFieldUI;
    public static InputFieldUI inputFieldUI;

    [SerializeField] private InformationDisplayUI _informationDisplayUI;
    public static InformationDisplayUI informationDisplayUI;

    [SerializeField] private ManageShopUI _manageShopUI;
    public static ManageShopUI manageShopUI;

    [SerializeField] private OnlineOrdersUI _onlineOrdersUI;
    public static OnlineOrdersUI onlineOrdersUI;

    [SerializeField] private FurnitureShopUI _furnitureShopUI;
    public static FurnitureShopUI furnitureShopUI;

    [SerializeField] private MapUI _mapUI;
    public static MapUI mapUI;

    public static bool BlockInput => leftPanelUI == null || leftPanelUI.blockInput;


    void Awake()
    {
        textUI = _textUI;
        moneyUI = _moneyUI;
        ordersUI = _ordersUI;
        buildingUI = _buildingUI;
        holdingUI = _holdingUI;
        timeUI = _timeUI;
        possibleActionsUI = _possibleActionsUI;
        manageUI = _manageUI;
        confirmUI = _confirmUI;
        landUnlockUI = _landUnlockUI;
        leftPanelUI = _leftPanelUI;
        nextDayUI = _nextDayUI;
        settingsUI = _settingsUI;
        warningsUI = _warningsUI;
        infoUI = _infoUI;
        tasksUI = _tasksUI;
        licencesUI = _licencesUI;
        inputFieldUI = _inputFieldUI;
        informationDisplayUI = _informationDisplayUI;
        manageShopUI = _manageShopUI;
        onlineOrdersUI = _onlineOrdersUI;
        furnitureShopUI = _furnitureShopUI;
        mapUI = _mapUI;
    }
}
