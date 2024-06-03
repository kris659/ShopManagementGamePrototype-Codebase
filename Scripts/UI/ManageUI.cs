
public class ManageUI : WindowsUIManager
{
    public override bool canClose => CanClose();
    public override void OpenUI()
    {
        base.OpenUI();
        if(currentlyOpenWindow == null) {
            OpenWindow(windowsUI[0]);
        }
        currentlyOpenWindow.UpdateOnParentOpen();
    }
    internal override void OpenWindow(WindowUI windowUI)
    {
        if (windowUI.isOpen) {            
            return;
        }
        if (currentlyOpenWindow != null) {
            currentlyOpenWindow.CloseUI();
        }
        windowUI.OpenUI();
        currentlyOpenWindow = windowUI;
    }
    
    bool CanClose()
    {
        foreach (WindowUI windowUI in windowsUI) {
            if (!windowUI.canClose)
                return false;
        }
        return true;
    }
        

    //[SerializeField] private TMP_InputField shopNameInputField;
    //[SerializeField] private TMP_InputField maxCustomersInputField;
    //[SerializeField] private Button unlockLandButton;
    //[SerializeField] private Button shopOpenButton;
    //[SerializeField] private Image openButtonImage;
    //[SerializeField] private TMP_Text openButtonText;

    //[SerializeField] private Image buttonOpenPrefab;
    //[SerializeField] private Image buttonClosedPrefab;

    //public override bool canClose => !(maxCustomersInputField.isFocused || shopNameInputField.isFocused);


    //private float changeDuration = 0.5f;

    //private void Awake()
    //{
    //    shopOpenButton.onClick.AddListener(OnShopOpenButtonClicked);        
    //    DOTween.Init();
    //    UpdateVisual();
    //    maxCustomersInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    //}

    //internal override void Start()
    //{
    //    base.Start();
    //    unlockLandButton.onClick.AddListener(UIManager.landUnlockUI.OpenUI);
    //}


    //private void OnShopOpenButtonClicked()
    //{
    //    ShopData.instance.ChangeShopOpenStatus(!ShopData.instance.isShopOpen);
    //    UpdateVisual();
    //}

    //public override void OpenUI()
    //{
    //    base.OpenUI();
    //    UpdateVisual(true);
    //}

    //private void UpdateVisual(bool instant = false)
    //{
    //    float duration = changeDuration;
    //    if (instant)
    //        duration = 0;
    //    if (ShopData.instance.isShopOpen) {
    //        openButtonImage.transform.DOMove(buttonOpenPrefab.transform.position, duration);
    //        openButtonText.transform.DOMove(buttonOpenPrefab.transform.position, duration);
    //        openButtonImage.DOColor(buttonOpenPrefab.color, duration);
    //        openButtonText.text = "Open";
    //    }
    //    else {
    //        openButtonImage.transform.DOMove(buttonClosedPrefab.transform.position, duration);
    //        openButtonText.transform.DOMove(buttonClosedPrefab.transform.position, duration);
    //        openButtonImage.DOColor(buttonClosedPrefab.color, duration);
    //        openButtonText.text = "Closed";
    //    }
    //}

    //private void OnInputFieldValueChanged(string textValue)
    //{
    //    if (!string.IsNullOrEmpty(textValue)) {
    //        int.TryParse(textValue, out int value);
    //        if (value < 5) value = 5;
    //        if (value > 50) value = 50;
    //        maxCustomersInputField.text = value.ToString();
    //        CustomerManager.instance.UpdateMaxCustomers(value);
    //    }
    //}
}
