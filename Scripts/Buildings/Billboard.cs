using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Billboard : WindowUI, IInteractable
{
    public static Billboard instance;

    [SerializeField] private CinemachineVirtualCamera billboardCamera;
    [SerializeField] private Sprite[] uiElements;
    [SerializeField] private SpriteRenderer photoRenderer;
    [SerializeField] private GameObject BillboardCanvas;
    [SerializeField] private List<TMP_FontAsset> FontsByIndex = new();
    [SerializeField] private List<Vector3> PositionByIndex = new();
    [SerializeField] private List<Color32> ColorByIndex = new();
    [SerializeField] public TextMeshPro ShopName;

    public override bool canClose => true;

    public int currentUIIndex = 0;
    public bool canCycleUI = false;

    public string InteractionText => "F - Change Display";
    public int InteractionTextSize => 60;

    internal override void Awake()
    {
        instance = this;
        SceneLoader.OnUISceneLoaded += () =>
        {
            Init(UIManager.leftPanelUI);
            UIGameObject = BillboardCanvas;
        };
    }

    public void OnPlayerButtonInteract()
    {
        if (!isOpen)
        {
            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }

    public override void OpenUI()
    {
        ActivateBillboardCamera();
        base.OpenUI();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        ConfirmSelection();
    }

    private void ActivateBillboardCamera()
    {
        billboardCamera.enabled = true;
        canCycleUI = true;
        ActivateCurrentUI(currentUIIndex);
    }


    private void Update()
    {
        if (!canCycleUI) return;
        HandleUIInput();
    }

    private void HandleUIInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CycleUI(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleUI(1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            CloseUI();
        }
    }

    public void CycleUI(int direction)
    {
        currentUIIndex = (currentUIIndex + direction + uiElements.Length) % uiElements.Length;
        ActivateCurrentUI(currentUIIndex);
    }

    public void ActivateCurrentUI(int UIIndex)
    {
        if (uiElements.Length == 0) return;
        photoRenderer.sprite = uiElements[UIIndex];
        ShopName.text = ShopData.instance.shopName;
        ShopName.font = FontsByIndex[UIIndex];
        ShopName.color = ColorByIndex[UIIndex];
        ShopName.transform.localPosition = PositionByIndex[UIIndex];
    }

    private void ConfirmSelection()
    {
        canCycleUI = false;
        billboardCamera.enabled = false;
    }


    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }
}