using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerPrefs wrapper
/// </summary>
public static class TrinaxPlayerPrefs
{
    const int STORAGEFALSE = 0;
    const int STORAGETRUE = 1;

    #region Booleans
    public static void SetBool(string _key, bool _value, bool _saveImmediate)
    {
        int value = _value ? STORAGETRUE : STORAGEFALSE;
        PlayerPrefs.SetInt(_key, value);
        if (_saveImmediate) Save();
    }

    public static bool GetBool(string _key)
    {
        int value = PlayerPrefs.GetInt(_key);
        return value == STORAGETRUE;
    }
    #endregion

    #region Integers
    public static void SetInt(string _key, int _value, bool _saveImmediate)
    {
        PlayerPrefs.SetInt(_key, _value);
        if (_saveImmediate) Save();
    }

    public static int GetInt(string _key)
    {
        return PlayerPrefs.GetInt(_key);
    }

    public static int GetInt(string _key, int _defaultValue)
    {
        return PlayerPrefs.GetInt(_key, _defaultValue);
    }
    #endregion

    #region FLOATS
    public static void SetFloat(string _key, int _value, bool _saveImmediate)
    {
        PlayerPrefs.SetFloat(_key, _value);
        if (_saveImmediate) Save();
    }

    public static float GetFloat(string _key)
    {
        return PlayerPrefs.GetFloat(_key);
    }

    public static float GetFloat(string _key, float _defaultValue)
    {
        return PlayerPrefs.GetFloat(_key, _defaultValue);
    }
    #endregion

    #region STRINGS
    public static void SetString(string _key, string _value, bool _saveImmediate)
    {
        PlayerPrefs.SetString(_key, _value);
        if (_saveImmediate) Save();
    }

    public static string GetString(string _key)
    {
        return PlayerPrefs.GetString(_key);
    }

    public static string GetString(string _key, string _defaultValue)
    {
        return PlayerPrefs.GetString(_key, _defaultValue);
    }
    #endregion

    #region HELPER FUNCS
    public static bool HasKey(string _key)
    {
        return PlayerPrefs.HasKey(_key);
    }

    public static void DeleteKey(string _key)
    {
        PlayerPrefs.DeleteKey(_key);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    static void Save()
    {
        PlayerPrefs.Save();
    }
    #endregion
}
