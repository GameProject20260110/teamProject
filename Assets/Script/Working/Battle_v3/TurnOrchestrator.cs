using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using System.Runtime.InteropServices;

// 턴 전체 페이즈 순서를 지휘(하향식 직접 호출)
// turnController와는 다른 코드 추후에 turnController 삭제 예정
public class TurnOrchestrator : MonoBehaviour
{
    private readonly DrawOrchestrator _playerDrawOrchestrator;
    private readonly DrawOrchestrator _enemyDrawOrchestrator;
    private readonly MulliganOrchestrator _playerMulliganOrchestrator;
    private readonly DiceRollOrchestrator _playerDiceRollOrchestrator;
    private readonly DiceRollController _enemyDiceRollController;
    private readonly CardPlacementOrchestrator _playerPlacementOrchestrator;
    private readonly bool _isPlayerFirst;

    private int _turnNumber;

    public TurnOrchestrator(bool isPlayerFirst, DrawOrchestrator playerDrawOrchestrator, DrawOrchestrator enemyDrawOrchestrator, MulliganOrchestrator playerMulliganOrchestrator, DiceRollOrchestrator playerDiceRollOrchestrator, DiceRollController enemyDiceRollController, CardPlacementOrchestrator playerPlacementOrchestrator)
    {

        _isPlayerFirst = isPlayerFirst;
        _playerDrawOrchestrator = playerDrawOrchestrator;
        _enemyDrawOrchestrator = enemyDrawOrchestrator;
        _playerMulliganOrchestrator = playerMulliganOrchestrator;
        _playerDiceRollOrchestrator = playerDiceRollOrchestrator;
        _enemyDiceRollController = enemyDiceRollController;
        _playerPlacementOrchestrator = playerPlacementOrchestrator;
    }


    public async UniTask RunTurn(CancellationToken ct)
    {
        try
        {
            _turnNumber++;
            bool isFirstTurn = _turnNumber == 1;

            if(_isPlayerFirst)
            {
                await RunActorAction(isPlayer: true, isFirstTurn, ct);
                await RunActorAction(isPlayer: false, isFirstTurn, ct);
            }
            else
            {
                await RunActorAction(isPlayer: false, isFirstTurn, ct);
                await RunActorAction(isPlayer: true, isFirstTurn, ct);
            }
            
            // TODO: 턴 종료(성공/후공 둘 다 행동 끝난 후)
            // TODO : 배틀
            // TODO : 배틀 후 플레이어 또는 적 HP가 0인지 확인해서 다음 턴 진행 여부 판단(HP 연결 필요)
        }
        catch(OperationCanceledException)
        {
            Debug.Log("[TurnOrchestrator] 턴 진행이 취소되었습니다.");
        }
    }

    // 행동 1회 - 드로우 -> 멀리건(플레이어만) -> 주사위 굴림 -> 패널 배치
    private async UniTask RunActorAction(bool isPlayer, bool isFirstTurn, CancellationToken ct)
    {
        DrawOrchestrator drawOrchestrator = isPlayer ? _playerDrawOrchestrator : _enemyDrawOrchestrator;
        if (drawOrchestrator == null) return;

        await drawOrchestrator.RunDrawSequence(isFirstTurn, ct);
        if(isPlayer && _playerMulliganOrchestrator != null)
        {
            await _playerMulliganOrchestrator.RunMulliganAsync(ct);
        }

        if(isPlayer)
        {
            if(_playerDiceRollOrchestrator != null)
            {
                await _playerDiceRollOrchestrator.RunAsync(ct);
            }
            else
            {
                _enemyDiceRollController?.RollHand();
            }
        }
        // TODO : 카드 패널 배치(플레이어/적 공통) 
        if(isPlayer)
        {
            if (_playerPlacementOrchestrator != null) await _playerPlacementOrchestrator.RunAsync(ct);
        }
        else
        {
             // TODO(적 배치)
        }
    }
}
