using UnityEngine;
using System.IO;

public class SaveManager
{
    private string savePath => Application.persistentDataPath;

    public void Save<T>(T data, string fileName)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(savePath, fileName);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public T Load<T>(string filename) where T : new()
    {
        try
        {
            string path = Path.Combine(savePath, filename);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            return new T();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return new T();
        }
    }

    public bool HasSaveFile(string fileName)
    {
        return File.Exists(Path.Combine(savePath, fileName));
    }

    public void Delete(string fileName)
    {
        try
        {
            string path = Path.Combine(savePath, fileName);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
