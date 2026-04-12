using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager instance;

    public int bestRound;
    public int bestScore;
    public int totalGamePlayed;
    public int totalClears;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save()
    {
        PlayerStatusData data = new PlayerStatusData
        {
            bestRound = bestRound,
            bestScore = bestScore,
            totalGamePlayed = totalGamePlayed,
            totalClears = totalClears
        };

        string json = JsonUtility.ToJson(data, true);
        try
        {
            System.IO.File.WriteAllText(SavePath(), json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"통계 저장 실패: {ex.Message} (PlayerStatsData)");
        }
    }

    public void Load()
    {
        string path = SavePath();
        if (!System.IO.File.Exists(path))
        {
            InitDefault();
            return;
        }
        
        string json = System.IO.File.ReadAllText(path);
        PlayerStatusData data = JsonUtility.FromJson<PlayerStatusData>(json);

        bestRound = data.bestRound;
        bestScore = data.bestScore;
        totalGamePlayed = data.totalGamePlayed;
        totalClears = data.totalClears; 
    }

    public void RecordGameEnd(int finalRound, int finalScore, bool cleared)
    {
        totalGamePlayed++;

        if (cleared)
        {
            totalClears++;
        }
        if (finalRound > bestRound)
        {
            bestRound = finalRound;
        }
        if (finalScore > bestScore)
        {
            bestScore = finalScore;
        }

        Save();
    }

    public void ResetStatus()
    {
        InitDefault();
        Save();
    }

    private void InitDefault()
    {
        bestRound = 0;
        bestScore = 0;
        totalGamePlayed = 0;
        totalClears = 0;
    }

    private string SavePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Application.persistentDataPath + "/playerStats.json";
    }
}
