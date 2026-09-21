using UnityEngine;

public class DeckPopupService : MonoBehaviour
{
    [SerializeField] private ConfirmPopup confirmPopup;
    [SerializeField] private ToastNotification toast;
    [SerializeField] private DeckNameInputPopup nameInput;

    public ConfirmPopup Confirm => confirmPopup;
    public ToastNotification Toast => toast;

    public DeckNameInputPopup NameInput => nameInput;

    public bool IsAnyPopupOpen => (confirmPopup != null && confirmPopup.IsOpen) || (nameInput != null && nameInput.IsOpen);

}
