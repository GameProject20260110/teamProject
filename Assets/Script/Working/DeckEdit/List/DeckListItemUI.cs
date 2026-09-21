using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckListItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject normalContent;
    [SerializeField] private GameObject addTileContent;
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private Image deckImage;
    [SerializeField] private GameObject selectionBorder;
    [SerializeField] private DeckManagementConfig config;

    private DeckListPanelController _owner;
    private DeckIndexEntry _entry;
    private bool _isAddTile;
    private bool _deleteModeOn;
    private bool _isSelected;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    public void SetUpAsAddTile(DeckListPanelController owner)
    {
        _owner = owner;
        _entry = null;
        _isAddTile = true;

        normalContent?.SetActive(false);
        addTileContent?.SetActive(true);
        ApplySelectionVisual(false, instant: true);
    }

    public void Setup(DeckListPanelController owner, DeckIndexEntry entry, CardDatabase cardDatabase)
    {
        _owner = owner;
        _entry = entry;
        _isAddTile = false;

        normalContent.SetActive(true);
        addTileContent?.SetActive(false);

        if (deckNameText != null)
            deckNameText.text = string.IsNullOrEmpty(entry.deckName) ? (config != null ? config.defaultDeckName : "»õ µ¦") : entry.deckName;

        SetDeckImage(entry, cardDatabase);
    }

    private void SetDeckImage(DeckIndexEntry entry, CardDatabase cardDatabase)
    {
        if (deckImage == null) return;

        CardData card = entry.coverCardId >= 0 && cardDatabase != null ? cardDatabase.FindById(entry.coverCardId) : null;
        Sprite sprite = card != null && card.diceSkin != null ? card.diceSkin.GetSprite(1) : null;

        deckImage.sprite = sprite;
        deckImage.enabled = sprite != null;
    }

    public void SetDeleteMode(bool on, bool isChecked)
    {
        if (_isAddTile) return;
        _deleteModeOn = on;
        _isSelected = on && isChecked;
        ApplySelectionVisual(_isSelected, instant: true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isAddTile) { _owner?.OnAddTileClicked(); return; }
        _owner?.OnDeckTileClicked(_entry);

        if(_deleteModeOn)
        {
            _isSelected = !_isSelected;
            ApplySelectionVisual(_isSelected, instant: false);
            _owner?.OnDeckCheckToggled(_entry.deckId, _isSelected);
            return;
        }
        _owner?.OnDeckTileClicked(_entry);
    }

    private void ApplySelectionVisual(bool selected, bool instant)
    {
        selectionBorder?.SetActive(selected);

        float scale = selected && config != null ? config.hoverScale : 1f;
        transform.DOKill();
        if (instant)
            transform.localScale = _baseScale * scale;
        else
            transform.DOScale(_baseScale * scale, config.SelectScaleDuration).SetEase(Ease.OutQuad);
    }
}
