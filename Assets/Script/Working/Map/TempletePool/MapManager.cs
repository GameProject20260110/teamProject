using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class MapManager : MonoBehaviour
{
    [Header("현재 스테이지의 맵 풀 (3개 후보)")]
    [SerializeField] private StageMapPool currentStagePool;

    [Header("Prefabs")]
    [SerializeField] private NodeView nodePrefab;

    [Header("Containers")]
    [SerializeField] private RectTransform nodeContainer;

    private MapPathDrawer pathDrawer;
    private MapPathfinder pathfinder;
    private MapCharacterMover characterMover;
    private MapIntroController introController;
    private MapStageFlowController stageFlow;
    private AudioManager audioManager;

    [Inject]
    public void Construct(MapPathDrawer pathDrawer, MapPathfinder pathfinder, MapCharacterMover characterMover,
                           MapIntroController introController, MapStageFlowController stageFlow,
                           AudioManager audioManager)
    {
        this.pathDrawer = pathDrawer;
        this.pathfinder = pathfinder;
        this.characterMover = characterMover;
        this.introController = introController;
        this.stageFlow = stageFlow;
        this.audioManager = audioManager;
    }

    private readonly Dictionary<int, NodeView> nodeViews = new();
    private readonly Dictionary<int, List<int>> adjacency = new();

    private MapTemplate currentTemplate;
    private int currentNodeId = -1;
    private bool isMoving;
    private CancellationTokenSource loadCts;

    private void Start()
    {
        audioManager.PlayBgm("Map");
        if (stageFlow.HasSavedProgress)
        {
            LoadMap(stageFlow.SavedTemplate, isRestoring: true);
        }
        else
        {
            EnterStage(currentStagePool);
        }
    }

    public void EnterStage(StageMapPool stagePool)
    {
        if (stagePool == null)
        {
            Debug.LogError("MapManager: currentStagePool이 비어있습니다.");
            return;
        }

        LoadRandomFromPool(stagePool);
    }

    public void LoadRandomFromPool(StageMapPool pool)
    {
        var template = pool.PickRandom();
        if (template == null) return;

        LoadMap(template);
    }

    public void LoadMap(MapTemplate template, bool isRestoring = false)
    {
        ClearMap();
        currentTemplate = template;

        // 1) 인접 리스트 구성
        foreach (var entry in template.Nodes)
        {
            adjacency[entry.id] = new List<int>(entry.connectedNodeIds);
        }

        // 2) 노드 생성. 복원 중이면 저장된 상태를, 아니면 데이터의 initialState를 사용.
        MapNodeEntry startEntry = null;
        foreach (var entry in template.Nodes)
        {
            var view = Instantiate(nodePrefab, nodeContainer);
            view.GetComponent<RectTransform>().anchoredPosition = entry.position;

            var initialState = (isRestoring && stageFlow.SavedNodeStates.TryGetValue(entry.id, out var savedState))
                ? savedState
                : entry.initialState;

            view.Init(entry.id, entry.typeData, initialState);
            view.OnNodeClicked += HandleNodeClicked;

            nodeViews[entry.id] = view;

            if (entry.isStartNode)
                startEntry = entry;
        }

        // 3) 선 생성 (양방향 연결이라 같은 쌍을 두 번 그리지 않도록 id가 작은 쪽에서만 그림)
        foreach (var entry in template.Nodes)
        {
            foreach (var targetId in entry.connectedNodeIds)
            {
                if (entry.id < targetId)
                    pathDrawer.DrawLine(entry.position, template.FindNode(targetId).position);
            }
        }

        if (startEntry == null)
        {
            Debug.LogError("MapTemplate에 시작 노드(isStartNode)가 없습니다.");
            return;
        }

        // 4) 현재 위치: 복원 중이면 저장된 위치, 아니면 시작 노드
        currentNodeId = isRestoring ? stageFlow.SavedCurrentNodeId : startEntry.id;

        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        bool playSpawnEffect = !isRestoring;

        introController.PlayIntroAsync(
            nodeViews.Values,
            GetNodeAnchoredPosition(nodeViews[currentNodeId]),
            loadCts.Token,
            playSpawnEffect
        ).Forget();

        if (isRestoring)
            stageFlow.ClearProgress();
    }

    private void ClearMap()
    {
        foreach (var view in nodeViews.Values)
        {
            view.OnNodeClicked -= HandleNodeClicked;
            if (view != null) Destroy(view.gameObject);
        }
        nodeViews.Clear();
        adjacency.Clear();

        pathDrawer.ClearLines();
        characterMover.Despawn();

        currentNodeId = -1;
    }

    private static Vector2 GetNodeAnchoredPosition(NodeView nodeView)
    {
        return nodeView.GetComponent<RectTransform>().anchoredPosition;
    }

    private void RevealNeighbors(int nodeId)
    {
        if (!adjacency.TryGetValue(nodeId, out var neighbors)) return;

        foreach (var neighborId in neighbors)
        {
            if (nodeViews.TryGetValue(neighborId, out var neighborView) &&
                neighborView.State == NodeState.Locked)
            {
                neighborView.SetState(NodeState.Available);
            }
        }
    }

    private void HandleNodeClicked(NodeView clicked)
    {
        if (isMoving) return;
        if (clicked.State == NodeState.Locked) return;

        var path = pathfinder.FindPath(adjacency, currentNodeId, clicked.NodeId);
        if (path == null)
        {
            Debug.LogWarning($"노드 {currentNodeId} -> {clicked.NodeId} 경로를 찾을 수 없습니다.");
            return;
        }

        path = pathfinder.TruncateAtFirstBlocking(path, nodeId =>
        {
            var view = nodeViews[nodeId];
            return view.Type != MNodeType.None && view.State != NodeState.Visited;
        });

        MoveAlongPath(path);
    }

    private void MoveAlongPath(List<int> path)
    {
        isMoving = true;

        var positions = new List<Vector2>(path.Count - 1);
        for (int i = 1; i < path.Count; i++)
            positions.Add(GetNodeAnchoredPosition(nodeViews[path[i]]));

        characterMover.MoveAlongPath(
            positions,
            onStepArrived: stepIndex =>
            {
                int nodeId = path[stepIndex + 1];
                bool isFinalStep = stepIndex == positions.Count - 1;

                if (!isFinalStep)
                    nodeViews[nodeId].SetState(NodeState.Visited);
            },
            onComplete: () =>
            {
                currentNodeId = path[path.Count - 1];
                ArriveAtNode(currentNodeId);
                isMoving = false;
            });
    }

    private void ArriveAtNode(int nodeId)
    {
        var view = nodeViews[nodeId];

        if (view.State == NodeState.Visited) return;

        CompleteCurrentNode(nodeId);

        if (stageFlow.TryTransitionToNodeScene(view.Type))
        {
            stageFlow.SaveProgress(currentTemplate, currentNodeId, nodeViews.Values);
        }
    }

    private void CompleteCurrentNode(int nodeId)
    {
        nodeViews[nodeId].SetState(NodeState.Visited);
        RevealNeighbors(nodeId);
    }
}