using UnityEngine;

public class WindowUI: MonoBehaviour
{
    internal GameObject UIGameObject;
    public bool isOpen { get { return UIGameObject.activeSelf; } }
    public virtual bool canClose { get { return true; } }

    internal WindowsUIManager windowsManager;

    internal virtual void Awake()
    {
        UIGameObject = transform.GetChild(0).gameObject;
        UIGameObject.SetActive(false);
    }

    internal virtual void Init(WindowsUIManager windowsManager)
    {
        this.windowsManager = windowsManager;        
    }

    public virtual void OpenUI()
    {
        if(windowsManager != null) {
            if (windowsManager.currentlyOpenWindow != null && windowsManager.currentlyOpenWindow != this)
                windowsManager.currentlyOpenWindow.CloseUI();
            windowsManager.currentlyOpenWindow = this;
        }
        UIGameObject.SetActive(true);
        PlayerInteractions.Instance.LockCameraForUI();
        Cursor.lockState = CursorLockMode.None;
    }
    public virtual void CloseUI()
    {
        UIGameObject.SetActive(false);
        PlayerInteractions.Instance.UnlockCameraForUI();
        if(!MainMenu.isMainMenuOpen)
            Cursor.lockState = CursorLockMode.Locked;
        if(windowsManager != null) {
            windowsManager.currentlyOpenWindow = null;
        }
    }
    public virtual void UpdateOnParentOpen()
    {

    }

    public static void DestroyChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).gameObject.activeSelf)
                Destroy(transform.GetChild(i).gameObject);
        }
    }
}
