using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class SaveFilesManager
{
    static string saveDirectoryPath = Path.Combine(Application.persistentDataPath,"Saves");
    static string startingSaveDirectoryPath = Path.Combine(Application.streamingAssetsPath,"Saves");

    public static void Save(string saveName, SaveData saveData)
    {
        string savePath = Path.Combine(saveDirectoryPath, saveName);
        Save(saveData, savePath);
    }

    public static void CreateStartingSave(string saveName, SaveData saveData)
    {
        string savePath = Path.Combine(startingSaveDirectoryPath, saveName);
        Debug.Log(savePath);
        Save(saveData, savePath);
    }
    public static SaveData LoadStartingSave(string saveName)
    {
        string path = Path.Combine(startingSaveDirectoryPath, saveName);
        return LoadFromFile(path);
    }

    static void Save(SaveData saveData, string savePath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            string jsonData = JsonUtility.ToJson(saveData);
            Debug.Log(jsonData);
            Debug.Log(savePath);

            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(jsonData);
                }
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error while saving data to file: " + savePath + "\n" + e);
        }
    }

    public static SaveData Load(string saveName)
    {
        string path = Path.Combine(saveDirectoryPath, saveName);
        return LoadFromFile(path);
    }

    static SaveData LoadFromFile(string path)
    {
        Debug.Log("Loading " + Path.GetFileName(path));
        SaveData loadedData = null;

        if (File.Exists(path))
        {
            try
            {
                string jsonToLoad = "";

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonToLoad = reader.ReadToEnd();
                    }
                }
                loadedData = JsonUtility.FromJson<SaveData>(jsonToLoad);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error while load data from file: " + path + "\n" + e);
            }
        }
        else Debug.LogError("Nie znaleziono pliku o nazwie: " + Path.GetFileName(path));
        Debug.Log(path);
        return loadedData;
    }

    public static List<string> GetSaveFileNames()
    {
        if(!Directory.Exists(saveDirectoryPath)) return new List<string>();

        DirectoryInfo directoryInfo = new DirectoryInfo(saveDirectoryPath);
        FileInfo[] files = directoryInfo.GetFiles().OrderBy(p => p.CreationTime).ToArray();
        List<string> fileNames = new List<string>();
        for (int i = 0; i < files.Length; i++) fileNames.Add(files[i].Name);
        return fileNames;
    } 
}
