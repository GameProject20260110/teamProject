using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬에서 맵을 디자인할 때만 쓰는 작업용 컴포넌트.
/// NodeView(런타임 표시용)와는 별개로, "이 노드의 타입은 뭐고 어디와 연결되는지"를
/// 에디터에서 직접 지정하기 위한 용도. 게임 실행 자체에는 관여하지 않음.
///
/// 사용법: 씬에 노드 프리팹을 배치할 때 이 컴포넌트도 같이 붙여서
/// typeData / connectedNodes / isStartNode를 인스펙터에서 채워 넣으면,
/// MapTemplateExporter가 이 정보를 읽어 MapTemplate 애셋으로 저장함.
/// </summary>
public class NodeAuthoring : MonoBehaviour
{
    [Tooltip("비워두면 None(빈 노드)")]
    public NodeTypeData typeData;

    [Tooltip("이 노드와 연결할 다른 노드들을 씬에서 직접 드래그해서 채우세요. " +
             "한쪽에만 등록해도 export 시 자동으로 양방향 처리됩니다.")]
    public List<NodeAuthoring> connectedNodes = new();

    public bool isStartNode;

    [Tooltip("맵 로드 시점의 초기 상태를 씬에서 직접 지정")]
    public NodeState initialState = NodeState.Available;

    // export 시 자동으로 채워지는 id. 인스펙터에서 직접 건드릴 필요 없음.
    [HideInInspector] public int exportedId = -1;

    private RectTransform rectTransform;
    public RectTransform RectTransform =>
        rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 씬 뷰에서 어떤 노드끼리 연결되는지 초록 선으로 미리보기 (선택 시)
        if (connectedNodes == null) return;

        Gizmos.color = Color.green;
        foreach (var other in connectedNodes)
        {
            if (other == null) continue;
            Gizmos.DrawLine(transform.position, other.transform.position);
        }
    }
#endif
}