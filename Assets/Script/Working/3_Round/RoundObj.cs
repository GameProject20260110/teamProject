using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class RoundObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Sprite NormalImg;
    [SerializeField] private Sprite PointerImg;
    
    private Image image;
    private SceneController sceneController;

    [Inject]
    public void Construct(SceneController sceneController)
    {
        this.sceneController = sceneController;
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = NormalImg;
        image.alphaHitTestMinimumThreshold = 0.1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        sceneController.LoadMapScene();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = PointerImg;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = NormalImg;
    }
}
