using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrinaxSaveManager
{
    public static TrinaxSaves saveObj;

    static TrinaxSaves CreateAdminSave()
    {
        KinectVars saveKinect = new KinectVars
        {
            hand_factorX = TrinaxGlobal.Instance.hand_factorX,
            hand_factorY = TrinaxGlobal.Instance.hand_factorY,
            hand_factorZ = TrinaxGlobal.Instance.hand_factorZ,
            hand_offsetY = TrinaxGlobal.Instance.hand_offsetY,

            max_userDistance = TrinaxGlobal.Instance.maxUserDistance,
            min_DetectDistance = TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT,
        };

        GameDurationVars saveGameDuration = new GameDurationVars
        {
            ballGameDuration = TrinaxGlobal.Instance.ballGameDuration,
            memoryGameDuration = TrinaxGlobal.Instance.memoryGameDuration,
            runningGameDuration = TrinaxGlobal.Instance.runningGameDuration,

            ballSpawnInterval = TrinaxGlobal.Instance.ballSpawnInterval,
            runningPower = TrinaxGlobal.Instance.runningPower,
        };

        TrinaxSaves save = new TrinaxSaves
        {
            ip = TrinaxAsyncServerManager.Instance.ip,
            photoPath = TrinaxGlobal.Instance.photoPath,
            idleInterval = TrinaxGlobal.Instance.idleInterval,
            donationIdleInterval = TrinaxGlobal.Instance.donationIdleInterval,

            kinect = saveKinect,
            gameDuration = saveGameDuration,

            useServer = TrinaxAsyncServerManager.Instance.useServer,
            useMocky = TrinaxAsyncServerManager.Instance.useMocky,
            muteAllSounds = TrinaxAudioManager.Instance.muteAllSounds,
            isDevMode = TrinaxGlobal.Instance.IsDevMode,

            //adnComPort1 = TrinaxGlobal.Instance.adnComPort1,
            //adnComPort2 = TrinaxGlobal.Instance.adnComPort2,

            masterVolume = TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.MASTER].slider.value,
            musicVolume = TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.MUSIC].slider.value,
            SFXVolume = TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.SFX].slider.value,
            UI_SFXVolume = TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.UI_SFX].slider.value,
            SFX2Volume = TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.SFX2].slider.value,
        };
        return save;
    }

    const string ADMINSAVEFILE = "adminsave.json";
    public static void SaveJson()
    {
        saveObj = CreateAdminSave();

        string saveJsonString = JsonUtility.ToJson(saveObj, true);

        JsonFileUtility.WriteJsonToFile(ADMINSAVEFILE, saveJsonString, JSONSTATE.PERSISTENT_DATA_PATH);
        Debug.Log("Saving as JSON " + saveJsonString);
    }

    public static void LoadJson()
    {
        string loadJsonString = JsonFileUtility.LoadJsonFromFile(ADMINSAVEFILE, JSONSTATE.PERSISTENT_DATA_PATH);
        saveObj = JsonUtility.FromJson<TrinaxSaves>(loadJsonString);

        if (saveObj != null)
        {
            // Assign our values back
            TrinaxAsyncServerManager.Instance.ip = saveObj.ip;
            TrinaxGlobal.Instance.photoPath = saveObj.photoPath;
            TrinaxGlobal.Instance.idleInterval = saveObj.idleInterval;
            TrinaxGlobal.Instance.donationIdleInterval = saveObj.donationIdleInterval;

            TrinaxGlobal.Instance.hand_factorX = saveObj.kinect.hand_factorX;
            TrinaxGlobal.Instance.hand_factorY = saveObj.kinect.hand_factorY;
            TrinaxGlobal.Instance.hand_factorZ = saveObj.kinect.hand_factorZ;
            TrinaxGlobal.Instance.hand_offsetY = saveObj.kinect.hand_offsetY;

            TrinaxGlobal.Instance.maxUserDistance = saveObj.kinect.max_userDistance;
            TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT = saveObj.kinect.min_DetectDistance;

            TrinaxGlobal.Instance.ballGameDuration = saveObj.gameDuration.ballGameDuration;
            TrinaxGlobal.Instance.memoryGameDuration = saveObj.gameDuration.memoryGameDuration;
            TrinaxGlobal.Instance.runningGameDuration = saveObj.gameDuration.runningGameDuration;

            TrinaxGlobal.Instance.ballSpawnInterval = saveObj.gameDuration.ballSpawnInterval;
            TrinaxGlobal.Instance.runningPower = saveObj.gameDuration.runningPower;

            TrinaxAsyncServerManager.Instance.useServer = saveObj.useServer;
            TrinaxAsyncServerManager.Instance.useMocky = saveObj.useMocky;
            TrinaxAudioManager.Instance.muteAllSounds = saveObj.muteAllSounds;

            TrinaxGlobal.Instance.IsDevMode = saveObj.isDevMode;

            TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.MASTER].slider.value = saveObj.masterVolume;
            TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.MUSIC].slider.value = saveObj.musicVolume;
            TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.SFX].slider.value = saveObj.SFXVolume;
            TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.UI_SFX].slider.value = saveObj.UI_SFXVolume;
            TrinaxAudioManager.Instance.audioSettings[(int)TrinaxAudioManager.AUDIOPLAYER.SFX2].slider.value = saveObj.SFX2Volume;
        }
        else
        {
            Debug.Log("Json file is empty! Creating a new save file...");
            saveObj = CreateAdminSave();
        }
    }

}
