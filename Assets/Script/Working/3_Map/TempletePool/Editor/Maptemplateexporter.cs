#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 씬에 배치된 NodeAuthoring들을 모아 MapTemplate 애셋으로 저장하는 에디터 윈도우.
/// 반드시 "Editor"라는 이름의 폴더 안에 넣어야 빌드에서 제외됩니다.
///
/// 열기: 메뉴바 Tools > MapSystem > Map Template Exporter
/// </summary>
public class MapTemplateExporter : EditorWindow
{
    private Transform mapRoot;
    private int stageIndex;
    private string savePath = "Assets/Script/Working/Map/Data";
    private string fileName = "NewMapTemplate";

    [MenuItem("Tools/MapSystem/Map Template Exporter")]
    private static void Open()
    {
        GetWindow<MapTemplateExporter>("Map Template Exporter");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("씬 → MapTemplate Export", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        mapRoot = (Transform)EditorGUILayout.ObjectField(
            "Map Root (노드들의 부모)", mapRoot, typeof(Transform), true);

        stageIndex = EditorGUILayout.IntField("Stage Index", stageIndex);
        savePath = EditorGUILayout.TextField("Save Folder", savePath);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(mapRoot == null))
        {
            if (GUILayout.Button("Export", GUILayout.Height(30)))
                Export();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Map Root 아래(자식 전체)에서 NodeAuthoring 컴포넌트를 전부 찾아 " +
            "위치/타입/연결/시작노드 정보를 MapTemplate 애셋으로 저장합니다.",
            MessageType.Info);
    }

    private void Export()
    {
        var allAuthorings = mapRoot.GetComponentsInChildren<NodeAuthoring>(includeInactive: true);

        // 활성화된(SetActive true) 노드만 이번 맵에 포함. 꺼둔 노드는 통째로 제외.
        var authorings = allAuthorings.Where(a => a.gameObject.activeInHierarchy).ToArray();

        if (authorings.Length == 0)
        {
            EditorUtility.DisplayDialog("Export 실패",
                "활성화된(SetActive 켜진) NodeAuthoring이 하나도 없습니다.", "확인");
            return;
        }

        // 1) id 자동 부여 (하이어라키 순서 기준, 활성 노드끼리만)
        for (int i = 0; i < authorings.Length; i++)
            authorings[i].exportedId = i;

        var activeSet = new HashSet<NodeAuthoring>(authorings);

        // 2) 양방향 연결 보정: 한쪽에만 등록돼 있어도 서로 다 채워줌.
        //    단, 비활성 노드를 향한 연결은 무시함.
        var bidirectionalLinks = new Dictionary<int, HashSet<int>>();
        foreach (var a in authorings)
            bidirectionalLinks[a.exportedId] = new HashSet<int>();

        int skippedLinks = 0;
        foreach (var a in authorings)
        {
            foreach (var other in a.connectedNodes)
            {
                if (other == null) continue;

                if (!activeSet.Contains(other))
                {
                    skippedLinks++;
                    continue; // 꺼져있는 노드로의 연결은 제외
                }

                bidirectionalLinks[a.exportedId].Add(other.exportedId);
                bidirectionalLinks[other.exportedId].Add(a.exportedId);
            }
        }

        // 3) MapNodeEntry 리스트 생성
        var entries = new List<MapNodeEntry>();
        foreach (var a in authorings)
        {
            entries.Add(new MapNodeEntry
            {
                id = a.exportedId,
                typeData = a.typeData,
                position = a.RectTransform.anchoredPosition,
                connectedNodeIds = bidirectionalLinks[a.exportedId].OrderBy(x => x).ToList(),
                isStartNode = a.isStartNode,
                initialState = a.initialState
            });
        }

        // 4) SO 애셋 생성 및 저장
        var template = ScriptableObject.CreateInstance<MapTemplate>();
        template.EditorSetData(stageIndex, entries);

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            EditorUtility.DisplayDialog("Export 실패", $"저장 폴더가 존재하지 않습니다: {savePath}", "확인");
            return;
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{fileName}.asset");
        AssetDatabase.CreateAsset(template, assetPath);
        AssetDatabase.SaveAssets();

        // 스크립트/타입 연결이 확실히 디스크에 올바르게 반영되도록 강제 재임포트.
        // (CreateAsset 직후 바로 저장할 때 GUID 연결이 불안정하게 기록되는 걸 방지)
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        // 5) 유효성 검증 실행 후 결과 표시
        var errors = template.Validate();
        string skippedNote = skippedLinks > 0
            ? $"\n\n(비활성 노드로 향하던 연결 {skippedLinks}개는 자동으로 제외됨)"
            : "";

        if (errors.Count > 0)
        {
            EditorUtility.DisplayDialog(
                "저장 완료 (경고 있음)",
                $"{assetPath}\n\n다음 문제가 발견되었습니다:\n- " + string.Join("\n- ", errors) + skippedNote,
                "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("저장 완료", $"{assetPath}\n\n검증 통과, 문제 없음.{skippedNote}", "확인");
        }

        Selection.activeObject = template;
        EditorGUIUtility.PingObject(template);
    }
}
#endif