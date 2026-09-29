using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class MapStageFlowController
{
    private readonly MapProgressStore progressStore;
    private readonly SceneController sceneController;
    private readonly EventObjectDatabase eventDatabase;
    private readonly MapEventObjectSpawner objectSpawner;
    private readonly MapCameraController cameraController;

    public MapStageFlowController(MapProgressStore progressStore, SceneController sceneController,
                                   EventObjectDatabase eventDatabase, MapEventObjectSpawner objectSpawner,
                                   MapCameraController cameraController)
    {
        this.progressStore = progressStore;
        this.sceneController = sceneController;
        this.eventDatabase = eventDatabase;
        this.objectSpawner = objectSpawner;
        this.cameraController = cameraController;
    }

    public bool HasSavedProgress => progressStore.HasProgress;
    public MapTemplate SavedTemplate => progressStore.CurrentTemplate;
    public int SavedCurrentNodeId => progressStore.CurrentNodeId;
    public IReadOnlyDictionary<int, NodeState> SavedNodeStates => progressStore.NodeStates;

    public void SaveProgress(MapTemplate template, int currentNodeId, IEnumerable<NodeView> nodeViews)
    {
        progressStore.Save(template, currentNodeId, nodeViews);
    }

    public void ClearProgress()
    {
        progressStore.Clear();
    }

    public async UniTask ShowEventAsync(RectTransform nodeRect)
    {
        var prefab = eventDatabase.RandomEventPick();
        await cameraController.ZoomToAsync(nodeRect.position, 2, 0.5f);
        try
        {
            var eventObject = objectSpawner.Spawn(nodeRect, prefab);
            if (eventObject != null)
                await eventObject.PlayAsync();
        }
        finally
        {
            objectSpawner.Despawn();
            await cameraController.ResetAsync(0.5f);
        }
    }

    public bool TryTransitionToNodeScene(MNodeType type)
    {
        switch (type)
        {
            case MNodeType.Shop: sceneController.LoadShopScene(); return true;
            case MNodeType.Battle: sceneController.LoadGameScene(); return true;
            case MNodeType.Boss: sceneController.LoadBossScene(); return true;
            default: return false;
        }
    }
}
