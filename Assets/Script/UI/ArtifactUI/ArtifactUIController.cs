using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class ArtifactUIController : MonoBehaviour
{
    public static ArtifactUIController instance;

    [SerializeField] private Transform artifactContainer;
    [SerializeField] private GameObject artifactIconPrefab;
    //[SerializeField] private ParticleSystem healParticle;

    private List<ArtifactIconUI> _icons = new List<ArtifactIconUI>();

    private ItemManager _itemManager;

    [Inject]
    public void Construct(ItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        foreach (var icon in _icons)
            Destroy(icon.gameObject);
        _icons.Clear();

        foreach(var artifact in _itemManager.artifacts)
        {
            GameObject obj = Instantiate(artifactIconPrefab, artifactContainer);
            ArtifactIconUI iconUi = obj.GetComponent<ArtifactIconUI>();
            iconUi.SetUp(artifact);
            _icons.Add(iconUi);
        }
    }

    public void AddArtifactIcon(BattleItemSo artifact)
    {
        CreateIcon(artifact);
    }

    private void CreateIcon(BattleItemSo artifact)
    {
        GameObject obj = Instantiate(artifactIconPrefab, artifactContainer);
        ArtifactIconUI iconUI = obj.GetComponent<ArtifactIconUI>();
        iconUI.SetUp(artifact);
        _icons.Add(iconUI);
    }

    public void PlayTelegraph(BattleItemSo artifact, bool active)
    {
        Find(artifact).SetParticleActive(active);
    }

    public void PlayEffect(BattleItemSo artifact)
    {
        Find(artifact).PlayTriggerEffect();
    }

    private ArtifactIconUI Find(BattleItemSo artifact)
    {
        int index = _itemManager.artifacts.IndexOf(artifact);
        if (index < 0 || index >= _icons.Count) return null;
        else return _icons[index];
    }

    //public void PlayHealParticle()
    //{
    //    if (healParticle != null)
    //        healParticle.Play();
    //}
}
