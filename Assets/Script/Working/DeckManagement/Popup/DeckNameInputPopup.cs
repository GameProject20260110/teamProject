using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckNameInputPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [Min(1)][SerializeField] private int maxLength = 20;

    private Action<string> _onConfirm;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (nameField != null) nameField.characterLimit = maxLength;
        nameField?.onValueChanged.AddListener(UpdateCounter);
        confirmButton?.onClick.AddListener(HandleConfirm);
        cancelButton?.onClick.AddListener(Hide);
        Hide();
    }

    public void Show(string currentName, Action<string> onConfirm)
    {
        _onConfirm = onConfirm;
        string value = currentName ?? "";
        if (nameField != null) nameField.text = currentName ?? "";
        UpdateCounter(value);
        root?.SetActive(true);
        nameField.Select();
    }
    
    private void UpdateCounter(string value)
    {
        if (counterText != null) counterText.text = $"{value.Length} / {maxLength}";
    }

    public void Hide()
    {
        root?.SetActive(false);
        _onConfirm = null;
    }

    private void HandleConfirm()
    {
        string value = nameField != null ? nameField.text : "";
        Action<string> callback = _onConfirm;
        Hide();
        callback?.Invoke(value);
    }
}


