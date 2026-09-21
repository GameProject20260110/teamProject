using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapIntroController
{
    private readonly MapCharacterMover characterMover;

    public MapIntroController(MapCharacterMover characterMover)
    {
        this.characterMover = characterMover;
    }

    public async UniTask PlayIntroAsync(IEnumerable<NodeView> nodeViews, Vector2 characterSpawnPosition, CancellationToken token = default, bool playSpawnEffect = true)
    {
        var revealTasks = new List<UniTask>();
        foreach (var view in nodeViews)
            revealTasks.Add(view.WaitForRevealAsync().AttachExternalCancellation(token));

        await UniTask.WhenAll(revealTasks).AttachExternalCancellation(token);

        if (token.IsCancellationRequested) return;

        characterMover.Spawn(characterSpawnPosition, playSpawnEffect);
    }
}