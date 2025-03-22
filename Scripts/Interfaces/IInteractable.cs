
public interface IInteractable
{
    public void OnPlayerButtonInteract();
    public string InteractionText { get; }
    public int InteractionTextSize { get; }

    public void OnMouseButtoDown();
    public void OnMouseButton();
    public void OnMouseButtonUp();
}
