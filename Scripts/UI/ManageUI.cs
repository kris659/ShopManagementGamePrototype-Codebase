
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
}
