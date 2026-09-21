using System;
using System.Collections.Generic;

public class MapPathfinder
{
    public List<int> FindPath(Dictionary<int, List<int>> adjacency, int startId, int targetId)
    {
        if (startId == targetId) return new List<int> { startId };

        var visited = new HashSet<int> { startId };
        var parent = new Dictionary<int, int>();
        var queue = new Queue<int>();
        queue.Enqueue(startId);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (!adjacency.TryGetValue(current, out var neighbors)) continue;

            foreach (var next in neighbors)
            {
                if (!visited.Add(next)) continue;

                parent[next] = current;

                if (next == targetId)
                    return ReconstructPath(startId, targetId, parent);

                queue.Enqueue(next);
            }
        }

        return null; // 경로 없음
    }

    private List<int> ReconstructPath(int startId, int targetId, Dictionary<int, int> parent)
    {
        var path = new List<int> { targetId };
        int cursor = targetId;

        while (cursor != startId)
        {
            cursor = parent[cursor];
            path.Add(cursor);
        }

        path.Reverse();
        return path;
    }

    public List<int> TruncateAtFirstBlocking(List<int> path, Func<int, bool> isBlocking)
    {
        for (int i = 1; i < path.Count; i++)
        {
            if (isBlocking(path[i]))
                return path.GetRange(0, i + 1);
        }

        return path;
    }
}