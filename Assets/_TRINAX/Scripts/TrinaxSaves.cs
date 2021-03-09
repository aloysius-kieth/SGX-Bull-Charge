using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrinaxSaves
{
    public string ip;
    public string photoPath;
    public float idleInterval;
    public float donationIdleInterval;

    public bool useServer;
    public bool useMocky;
    public bool muteAllSounds;
    public bool isDevMode;

    //public string adnComPort1;
    //public string adnComPort2;

    public float masterVolume;
    public float musicVolume;
    public float SFXVolume;
    public float UI_SFXVolume;
    public float SFX2Volume;

    public KinectVars kinect;
    public GameDurationVars gameDuration;
}

[System.Serializable]
public class KinectVars
{
    public float hand_factorX;
    public float hand_factorY;
    public float hand_factorZ;
    public float hand_offsetY;

    public float max_userDistance;
    public float min_DetectDistance;
}

[System.Serializable]
public class GameDurationVars
{
    public int ballGameDuration;
    public int memoryGameDuration;
    public int runningGameDuration;

    public float ballSpawnInterval;
    public float runningPower;
}

#region REGISTER SEND
[System.Serializable]
public class RegisterSendJsonData
{
    //public string name;
    public string email;
    //public string phone;
}

[System.Serializable]
public class RegisterReceiveJsonData
{
    public requestClass request;
    public errorClass error;
    public RegisterData data;
}

[System.Serializable]
public class RegisterData
{
    public string userID;
    public bool dispense;
}
#endregion

#region OTP
[System.Serializable]
public class OTPRecieveJsonData
{
    public requestClass request;
    public errorClass error;
    public bool data;
}
#endregion

#region CHECK TRANSACTION
//[System.Serializable]
//public class CheckTransactionSendJsonData
//{
//    public string userID;
//    public string gameType;
//}

//[System.Serializable]
//public class CheckTransactionReceiveJsonData
//{
//    public requestClass request;
//    public errorClass error;
//    public bool data;
//}
#endregion

#region ADD RESULT
[System.Serializable]
public class AddResultSendJsonData
{
    public bool result;
    public string score;
    public bool chooseEmailSent;
    //public bool chooseFacebookSent;
    //public int amount;
    public string gameType;
    public string userID;
}

[System.Serializable]
public class AddResultReceiveJsonData
{
    public requestClass request;
    public errorClass error;
    public bool data;
}
#endregion

#region DISPENSE
[System.Serializable]
public class DispenseSendJsonData
{
    public string userID;
}

[System.Serializable]
public class DispenseReceiveJsonData
{
    public requestClass request;
    public errorClass error;
    public bool data;
}
#endregion

#region UPLOAD
[System.Serializable]
public class UploadSendJsonData
{
    public string imageFile;
    public string directory;
}

[System.Serializable]
public class UploadReceiveJsonData
{
    public requestClass request;
    public errorClass error;
    public bool data;
}
#endregion

//#region ENABLE DONATE
//[System.Serializable]
//public class EnableReceiveJsonData
//{
//    public requestClass request;
//    public errorClass error;
//    public EnableDonateData data;
//}

//[System.Serializable]
//public class EnableDonateData
//{
//    public bool result;
//    public int amount;
//}
//#endregion

//#region DISABLE DONATE
//[System.Serializable]
//public class DisableReceiveJsonData
//{
//    public requestClass request;
//    public errorClass error;
//    public DisableDonateData data;
//}

//[System.Serializable]
//public class DisableDonateData
//{
//    public bool result;
//    public int amount;
//}
//#endregion

//#region DONATE
//[System.Serializable]
//public class DonateReceiveJsonData
//{
//    public requestClass request;
//    public errorClass error;
//    public DonateData data;
//}

//[System.Serializable]
//public class DonateData
//{
//    public bool result;
//    public int amount;
//}
//#endregion

#region STOCK CHECK
[System.Serializable]
public class StockCheckReceiveJsonData
{
    public requestClass request;
    public errorClass error;
    public List<StockCheckData> data;
    public int total;
}

[System.Serializable]
public class StockCheckData
{
    public string vendingID;
    public string rack;
    public int quantity;
    public string type;
    public int point;
    public string application;
    public string rfid;
    public string createdDateTime;
    public string modifiedDateTime;
}
#endregion

[System.Serializable]
public class requestClass
{
    public string api;
    public string result;
}

[System.Serializable]
public class errorClass
{
    public string error_code;
    public string error_message;
}

/// <summary>
/// For using playerPrefs with.
/// </summary>
public interface ITrinaxPersistantVars
{
    string ip { get; set; }
    string photoPath { get; set; }
    bool useServer { get; set; }
}

