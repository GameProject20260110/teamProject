using UnityEngine;

public class FairySpiritSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FairyColorSet
    {
        public string variantName;
        public AnimationClip flapClip;
    }

    [SerializeField] private RuntimeAnimatorController baseController;

    [Header("나비들")]
    [SerializeField] private FairyColorSet[] colorSets;

    [SerializeField] private GameObject fairyPrefab;
    [SerializeField] private Transform spawnParent;

    [Header("소환 수")]
    [SerializeField] private int fairyCount = 3;

    [Header("좌표")]
    [SerializeField] private float leftX = -8f;
    [SerializeField] private float rightX = 8f;
    [SerializeField] private float bottomY = -4f;
    [SerializeField] private float topY = 4f;

    [Header("움직임")]
    [SerializeField] private float minSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 2.2f;
    [SerializeField] private float minWaitAtPoint = 0.5f;
    [SerializeField] private float maxWaitAtPoint = 1.5f;

    private void Start()
    {
        if (fairyPrefab == null || colorSets == null || colorSets.Length == 0 || baseController == null) return;

        for (int i = 0; i < fairyCount; i++)
        {
            SpawnFairy();
        }
    }

    private void SpawnFairy()
    {
        GameObject obj = Instantiate(fairyPrefab, spawnParent != null ? spawnParent : transform);
        FairySpirit fairy = obj.GetComponent<FairySpirit>();

        if (fairy == null)
        {
            Debug.LogWarning("[FairySpiritSpawner] fairyPrefab에 FairySpirit 컴포넌트가 없습니다.");
            Destroy(obj);
            return;
        }

        FairyColorSet set = colorSets[Random.Range(0, colorSets.Length)];

        var overrideController = new AnimatorOverrideController(baseController);
        var overrides = new System.Collections.Generic.List<
            System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(
                overrides[i].Key, set.flapClip);
        }

        overrideController.ApplyOverrides(overrides);

        Bounds bounds = new Bounds();
        bounds.SetMinMax(
            new Vector3(leftX, bottomY, 0f),
            new Vector3(rightX, topY, 0f));

        fairy.Init(
            overrideController,
            bounds,
            minSpeed,
            maxSpeed,
            minWaitAtPoint,
            maxWaitAtPoint);
    }
}
