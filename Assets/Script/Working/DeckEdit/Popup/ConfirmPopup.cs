using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonLabel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI cancelButtonLabel;

    private Action _onConfirm;
    private Action _onCancel;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        confirmButton?.onClick.AddListener(HandleConfirm);
        cancelButton?.onClick.AddListener(HandleCancel);
        Hide();
    }

    public void Show(string message, Action onConfirm, Action onCancel, string title = "", string confirmLabel = "예", string cancelLabel = "아니요")
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        SetTexts(title, message);
        if (confirmButtonLabel != null) confirmButtonLabel.text = confirmLabel;
        if(cancelButtonLabel != null) cancelButtonLabel.text = cancelLabel;
        cancelButton?.gameObject.SetActive(true);

        root?.SetActive(true);
    }

    // 확인 버튼만 있는 단순 알림창
    public void ShowAlert(string title, string message, Action onConfirm = null)
    {
        _onConfirm = onConfirm;
        _onCancel = null;

        SetTexts(title, message);
        if (confirmButtonLabel != null) confirmButtonLabel.text = "확인";
        cancelButton?.gameObject.SetActive(false);

        root?.SetActive(true);
    }

    public void Hide()
    {
        root?.SetActive(false);
        _onConfirm = null;
        _onCancel = null;
    }

    private void SetTexts(string title, string message)
    {
        if(titleText != null)
        {
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            titleText.text = title;
        }
        if (messageText != null) messageText.text = message;
    }

    private void HandleConfirm()
    {
        Action callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    private void HandleCancel()
    {
        Action callback = _onCancel;
        Hide();
        callback?.Invoke();
    }
}
