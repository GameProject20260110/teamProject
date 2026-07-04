using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

// 턴 사이클 흐름만 담당
public class TurnController
{
    private readonly BattleManager _bm;

    public TurnController(BattleManager battleManager)
    {
        _bm = battleManager;
    }

    public async UniTask RunOneTurnCycle()
    {
        var ct = _bm.BattleToken;

        try
        {
            // 1. 적 방어
            _bm.isPlayerTurn = false;
            await ExecuteEnemyDefense(ct);
            await UniTask.Delay(500, cancellationToken: ct);

            // 2. 내 방어
            await ExecutePlayerDefense(ct);
            await UniTask.Delay(500, cancellationToken: ct);

            // 3. 내 공격
            _bm.isPlayerTurn = true;
            await ExecutePlayerAttack(ct);
            if (_bm.EnemyData.IsDead())
            {
                Debug.Log("Dead");
                await _bm.HandleBattleEnd(isSuccess: true);
                return;
            }
            await UniTask.Delay(500, cancellationToken: ct);

            // 4. 적 공격
            _bm.isPlayerTurn = false;
            await ExecuteEnemyAttack(ct);
            if (_bm.PlayerData.IsDead())
            {
                await _bm.HandleBattleEnd(isSuccess: false);
                return;
            }
            await UniTask.Delay(500, cancellationToken: ct);

            // 5. 턴 종료
            await ExecuteNewTurn(ct);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("전투가 취소되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"전투 오류: {e.Message}\n{e.StackTrace}");
        }
    }

    #region 페이즈 정보

    private async UniTask ExecuteEnemyDefense(CancellationToken ct)
    {
        foreach (var dice in _bm.DefenseEnemyDices)
        {
            if (dice == null) continue;
            await dice.Glow.ShowGlowAsync(ct);
            var ctx = _bm.CreateDiceCtx(false, dice, _bm.AttackEnemyDices, _bm.DefenseEnemyDices);
            await dice.Effect.OnDefense(ctx);
            dice.Glow.HideGlow();
        }
    }

    private async UniTask ExecutePlayerAttack(CancellationToken ct)
    {
        foreach (var dice in _bm.AttackDices)
        {
            if (dice == null) continue;
            _bm.EventBus.TriggerPlayerAttackStart(_bm.CreateDiceCtx(false, dice, _bm.AttackDices, _bm.DefenseDices));
            await UniTask.Delay(300, cancellationToken: ct);

            await dice.Glow.ShowGlowAsync(ct);
            var ctx = _bm.CreateDiceCtx(true, dice, _bm.AttackDices, _bm.DefenseDices);
            await dice.Effect.OnAttack(ctx);
            dice.Glow.HideGlow();
        }
    }

    private async UniTask ExecutePlayerDefense(CancellationToken ct)
    {
        foreach (var dice in _bm.DefenseDices)
        {
            await dice.Glow.ShowGlowAsync(ct);
            var ctx = _bm.CreateDiceCtx(true, dice, _bm.AttackDices, _bm.DefenseDices);
            await dice.Effect.OnDefense(ctx);
            dice.Glow.HideGlow();
        }
        _bm.SaveBattleData();
    }

    private async UniTask ExecuteEnemyAttack(CancellationToken ct)
    {
        foreach (var dice in _bm.AttackEnemyDices)
        {
            if (dice == null) continue;
            _bm.EventBus.TriggerEnemyAttackStart(_bm.CreateDiceCtx(false, dice, _bm.AttackEnemyDices, _bm.DefenseEnemyDices));
            await UniTask.Delay(300, cancellationToken: ct);

            await dice.Glow.ShowGlowAsync(ct);
            var ctx = _bm.CreateDiceCtx(false, dice, _bm.AttackEnemyDices, _bm.DefenseEnemyDices);
            await dice.Effect.OnAttack(ctx);
            dice.Glow.HideGlow();
        }
    }

    private async UniTask ExecuteNewTurn(CancellationToken ct)
    {
        var playerCtx = _bm.CreateCtx(isPlayer: true);
        var enemyCtx = _bm.CreateCtx(isPlayer: false);

        var playerDiceCtx = new DiceContext { battle = playerCtx };
        var enemyDiceCtx = new DiceContext { battle = enemyCtx };

        await _bm.EnemyData.ProcessTurnStart(enemyDiceCtx);
        await _bm.PlayerData.ProcessTurnStart(playerDiceCtx);

        if (_bm.EnemyData.IsDead()) { await _bm.HandleBattleEnd(isSuccess: true); return; }
        if (_bm.PlayerData.IsDead()) { await _bm.HandleBattleEnd(isSuccess: false); return; }

        // 실드 리셋
        _bm.PlayerData.ResetShield();
        _bm.EnemyData.ResetShield();
        _bm.UpdateShieldUI();

        // VFX 리셋
        _bm.ResetAllDiceVFX();
        _bm.ClearEnemyDices();

        DicePanelManager.instance?.ResetAllDice(DiceManager.instance.GetAllDice());

        // 주사위 세팅
        DeckManager.instance.DrawDice();
        EnemyDeckHandler.instance.SetupEnemyDice();

       
        // 턴 종료
        _bm.EventBus.TriggerTurnEnd(playerCtx);
        _bm.currentTurn++;

        // 턴 시작
        _bm.EventBus.TriggerTurnStart(_bm.CreateCtx());

        await _bm.UpdateTurnUI();

        await GameManager.instance.EnemyRoll();

        _bm.isPlayerTurn = true;
    }

    #endregion
}
