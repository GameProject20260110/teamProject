using UnityEngine;
using System.IO;
using System;

public class SaveManager
{
    private string savePath => Application.persistentDataPath;

    public bool Save<T>(T data, string fileName)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            WriteFileAtomic(Path.Combine(savePath, fileName), json);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Save 실패({fileName}) : {e.Message}");
            return false;
        }
    }

    public bool Load<T>(string fileName, out T data) where T : new()
    {
        string path = Path.Combine(savePath, fileName);
        if(!File.Exists(path))
        {
            data = new T();
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<T>(json);
            if(data == null)
            {
                data = new T();
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load 실패({fileName}) : {e.Message}");
            data = new T();
            return false;
        }
    }

    public bool HasSaveFile(string fileName)
    {
        return File.Exists(Path.Combine(savePath, fileName));
    }

    public bool Delete(string fileName)
    {
        try
        {
            string path = Path.Combine(savePath, fileName);
            if (File.Exists(path)) File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Delete 실패({fileName}) : {e.Message}");
            return false;
        }
    }

    // 임시 파일에 먼저 쓰고 나중에 교체
    private static void WriteFileAtomic(string path, string content) 
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, content);
        if (File.Exists(path)) File.Delete(path);
        File.Move(tempPath, path);
    }
}
