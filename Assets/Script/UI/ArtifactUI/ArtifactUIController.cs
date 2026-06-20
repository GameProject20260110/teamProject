using UnityEngine;
using System.Collections.Generic;

public class ArtifactUIController : MonoBehaviour
{
    public static ArtifactUIController instance;

    [SerializeField] private Transform artifactContainer;
    [SerializeField] private GameObject artifactIconPrefab;
    [SerializeField] private ParticleSystem healParticle;

    private List<ArtifactIconUI> _icons = new List<ArtifactIconUI>();

    private void Awake()
    {
        if (instance == null) instance = this;
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

        foreach(var artifact in ItemManager.instance.artifacts)
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

    public void PlayEffect(BattleItemSo artifact)
    {
        int index = ItemManager.instance.artifacts.IndexOf(artifact);
        if (index < 0 || index >= _icons.Count) return;
        _icons[index].PlayTriggerEffect();
    }

    public void PlayHealParticle()
    {
        if (healParticle != null)
            healParticle.Play();
    }
}
