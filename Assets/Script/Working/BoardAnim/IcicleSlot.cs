using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class IcicleSlot : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private IcicleStageConfig stageConfig;

    [Header("고드름 프리팹")]
    [SerializeField] private SpriteRenderer piecePrefab;

    [Header("타이밍")]
    [SerializeField] private int turnsPerStage = 1;
    [SerializeField] private int turnsToRegrow = 1;

    [Header("떨어지는 이펙트 연출")]
    [SerializeField] private float fallDistance = 3f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private int shardCountPerPiece = 4;

    private readonly List<SpriteRenderer> _pieces = new List<SpriteRenderer>();
    private int _stage = -1;
    private int _turnCounter = 0;
    private bool _isFalling = false;


    public void AdvanceGrowth()
    {
        if (stageConfig == null || _isFalling || _stage >= stageConfig.StageCount - 1) return;

        _turnCounter++;
        if (_turnCounter >= turnsPerStage)
        {
            _turnCounter = 0;
            GoToStage(_stage + 1, animate: true);
        }
    }

    private void GoToStage(int newStage, bool animate)
    {
        if (stageConfig == null || piecePrefab == null) return;

        _stage = newStage;
        var config = stageConfig.GetStage(_stage);

        for (int i = 0; i < config.Count; i++)
        {
            bool isNew = i >= _pieces.Count;

            if (isNew)
            {
                SpriteRenderer sr = Instantiate(piecePrefab, transform);
                if (sr.GetComponent<Collider2D>() == null && sr.sprite != null)
                {
                    var col = sr.gameObject.AddComponent<BoxCollider2D>();
                    col.size = sr.sprite.bounds.size;
                    col.offset = sr.sprite.bounds.center;
                }

                var relay = sr.gameObject.AddComponent<IciclePieceClickRelay>();
                relay.Init(this);

                sr.transform.localPosition = new Vector3(config[i].localX, 0f, 0f);
                _pieces.Add(sr);

                if (animate)
                {
                    sr.transform.localScale = Vector3.zero;
                    sr.transform.DOScale(config[i].scale, 0.4f).SetEase(Ease.OutBack);
                }
                else
                {
                    sr.transform.localScale = Vector3.one * config[i].scale;
                }
            }
            else
            {
                SpriteRenderer sr = _pieces[i];
                if (animate)
                {
                    sr.transform.DOScale(config[i].scale, 0.4f).SetEase(Ease.OutBack);
                    sr.transform.DOLocalMoveX(config[i].localX, 0.4f);
                }
                else
                {
                    sr.transform.localScale = Vector3.one * config[i].scale;
                    sr.transform.localPosition = new Vector3(config[i].localX, 0f, 0f);
                }
            }
        }
    }

    public void OnPieceClicked()
    {
        if (_isFalling || _pieces.Count == 0) return;
        DropAll();
    }

    private void DropAll()
    {
        _isFalling = true;
        Sequence group = DOTween.Sequence();

        foreach (var piece in _pieces)
        {
            Vector3 startPos = piece.transform.position;

            Sequence seq = DOTween.Sequence().SetLink(piece.gameObject);
            seq.Append(piece.transform.DOMoveY(startPos.y - fallDistance, fallDuration).SetEase(Ease.InQuad));
            seq.AppendCallback(() => SpawnShards(piece.transform.position));
            seq.Append(piece.DOFade(0f, 0.15f));
            group.Join(seq);
        }

        group.OnComplete(RegrowFromStart);
    }

    private void SpawnShards(Vector3 pos)
    {
        if (shardPrefab == null) return;

        for (int i = 0; i < shardCountPerPiece; i++)
        {
            GameObject shard = Instantiate(shardPrefab, pos, Quaternion.identity);
            Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.2f, 1f), 0).normalized;

            Sequence shardSeq = DOTween.Sequence().SetLink(shard);
            shardSeq.Join(shard.transform.DOMove(shard.transform.position + dir * Random.Range(0.5f, 1.5f), 0.4f).SetEase(Ease.OutQuad));
            shardSeq.Join(shard.transform.DORotate(new Vector3(0, 0, Random.Range(-180f, 180f)), 0.4f));
            shardSeq.OnComplete(() => Destroy(shard));
        }
    }

    private void RegrowFromStart()
    {
        foreach (var piece in _pieces)
            if (piece != null) Destroy(piece.gameObject);

        _pieces.Clear();
        _stage = -1;
        _turnCounter = -turnsToRegrow;
        _isFalling = false;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Advance Turn")]
    private void DebugAdvanceGrowth() => AdvanceGrowth();

    [ContextMenu("Debug: Force Drop")]
    private void DebugForceDrop() => OnPieceClicked();
#endif
}