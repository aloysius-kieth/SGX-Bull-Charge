using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum JSONSTATE
{
    RESOURCE,
    PERSISTENT_DATA_PATH,
    GAME_DATA_PATH,
}

public class JsonFileUtility
{
    const string SAVE_DIR = "/Saves/";

    public static string LoadJsonFromFile(string path, JSONSTATE state)
    {
        string loadJsonString = "";

        switch (state)
        {
            case JSONSTATE.RESOURCE:
                loadJsonString = LoadJsonFromResource(path);
                break;
          
            case JSONSTATE.PERSISTENT_DATA_PATH:
                loadJsonString = LoadJsonFromPersistentData(path);
                break;
          
            case JSONSTATE.GAME_DATA_PATH:
                loadJsonString = LoadJsonFromGameDataPath(path);
                break;
        }
        return loadJsonString;
    }

    public static void WriteJsonToFile(string path, string content, JSONSTATE state)
    {
        switch (state)
        {
            case JSONSTATE.RESOURCE:
                WriteJsonToResource(path, content);
                break;
            case JSONSTATE.PERSISTENT_DATA_PATH:
                WriteJsonToPersistentData(path, content);
                break;
            case JSONSTATE.GAME_DATA_PATH:
                WriteJsonToGameDataPath(path, content);
                break;
            default:
                break;
        }
    }

    #region Loading Json Funcs

    // Load from resource path
    static string LoadJsonFromResource(string path)
    {
        //Remove the file extension
        path = path.Replace(".json", "");

        //Load json file from resource folder as text asset
        TextAsset loadedJsonFile = Resources.Load<TextAsset>(path);
        if(loadedJsonFile != null)
        {
            Debug.Log("Loaded " + path + ".json" + " successfully");
            return loadedJsonFile.text;
        }
        else
        {
            Debug.Log("Could not find " + path + ".json!");
            return null;
        }
    }

    // Load from persistent data path
    static string LoadJsonFromPersistentData(string path)
    {
        path = Application.persistentDataPath + "/" + path;
        if (!File.Exists(path))
        {
            Debug.Log(path + " does not exist!");
            return null;
        }

        StreamReader reader = new StreamReader(path);
        string response = "";
        while (!reader.EndOfStream)
        {
            response += reader.ReadLine();
        }
        reader.Close();
        Debug.Log("Loaded " + path + " successfully");
        return response;
    }

    // Load from game data path
    static string LoadJsonFromGameDataPath(string path)
    {
        path = Application.dataPath + "/" + path;
        if (!File.Exists(path))
        {
            Debug.Log(path + " does not exist!");
            return null;
        }

        StreamReader reader = new StreamReader(path);
        string response = "";
        while (!reader.EndOfStream)
        {
            response += reader.ReadLine();
        }
        reader.Close();
        Debug.Log("Loaded " + path + " successfully");
        return response;
    }

    #endregion

    #region Writing Json Funcs

    // Writes Json to resource path
    static void WriteJsonToResource(string path, string content)
    {
        path = Application.dataPath + "/" + "Resources/" + path;
        
        FileStream stream = File.Create(path);
        byte[] contentBytes = new UTF8Encoding(true).GetBytes(content);
        stream.Write(contentBytes, 0, contentBytes.Length);
        stream.Close();
        Debug.Log(content + " has been written to " + path);
    }

    // Writes Json to persistent data path
    static void WriteJsonToPersistentData(string path, string content)
    {
        path = Application.persistentDataPath + "/" + path;

        FileStream stream = File.Create(path);
        byte[] contentBytes = new UTF8Encoding(true).GetBytes(content);
        stream.Write(contentBytes, 0, contentBytes.Length);
        stream.Close();
        Debug.Log(content + " has been written to " + path);
    }

    // Writes Json to game data path
    static void WriteJsonToGameDataPath(string path, string content)
    {
        path = Application.dataPath + "/" + path;

        FileStream stream = File.Create(path);
        byte[] contentBytes = new UTF8Encoding(true).GetBytes(content);
        stream.Write(contentBytes, 0, contentBytes.Length);
        stream.Close();
        Debug.Log(content + " has been written to " + path);
    }

    #endregion
}
