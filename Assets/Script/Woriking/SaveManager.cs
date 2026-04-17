using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string savePath => Application.persistentDataPath;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save<T>(T data, string fileName)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(savePath, fileName);
            File.WriteAllText(path, json);
            
        }
        catch(System.Exception e)
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
                T data = JsonUtility.FromJson<T>(json);
                Debug.Log($"로드 완료");
                return data;
            }
            else
            {
                Debug.LogError($"파일 없음");
                return new T();
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
            return new T();
        }
    }

    public bool HasSaveFile(string fileName)
    {
        string path = Path.Combine(savePath,fileName);
        return File.Exists(path);
    }

    public void Delete(string fileName)
    {
        try
        {
            string path = Path.Combine(savePath, fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"삭제 완료: {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"삭제 실패: {fileName}\n{e.Message}");
        }
    }

}
