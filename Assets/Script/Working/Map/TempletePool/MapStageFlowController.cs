using System.Collections.Generic;

public class MapStageFlowController
{
    private readonly MapProgressStore progressStore;
    private readonly SceneController sceneController;

    public MapStageFlowController(MapProgressStore progressStore, SceneController sceneController)
    {
        this.progressStore = progressStore;
        this.sceneController = sceneController;
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

    public bool TryTransitionToNodeScene(MNodeType type)
    {
        switch (type)
        {
            case MNodeType.Shop:
                sceneController.LoadShopScene();
                return true;

            case MNodeType.Battle:
                sceneController.LoadGameScene();
                return true;

            case MNodeType.Event:
                sceneController.LoadEventScene();
                return true;

            case MNodeType.Boss:
                sceneController.LoadBossScene();
                return true;

            default:
                return false;
        }
    }
}
