using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// IDs of all inputFields
/// </summary>
/// 
public enum FIELD_ID
{
    // Adjust as needed
    SERVER_IP,
    PHOTO_PATH,
    K_FACTORX,
    K_FACTORY,
    K_FACTORZ,
    K_OFFSETY,
    IDLE_INTERVAL,
    BALL_GAME_DURATION,
    RUNNING_GAME_DURATION,
    MEMORY_GAME_DURATION,
    BALL_SPAWN_INTERVAL,
    RUNNING_POWER,

    MAX_USER_DISTANCE,
    MIN_DETECT_DISTANCE,
    DONATION_IDLE_INTERVAL,
}

/// <summary>
/// IDs of all toggles
/// </summary>
public enum TOGGLE_ID
{
    // Adjust as needed
    USE_SERVER,
    USE_MOCKY,
    MUTE_SOUND,
    DEV_MODE,
}

/// <summary>
/// IDs of all sliders
/// </summary>
public enum SLIDER_ID
{
    // Adjust as needed
    MASTER,
    MUSIC,
    SFX,
    UI_SFX,
    SFX2,
}

/// <summary>
/// Admin Panel
/// </summary>
public class TrinaxAdminPanel : MonoBehaviour
{
    [Header("InputFields")]
    public TMP_InputField[] inputFields;

    [Header("Toggles")]
    public Toggle[] toggles;

    [Header("Sliders")]
    public Slider[] sliders;
    public TextMeshProUGUI[] sliderValue;

    [Header("Panel Buttons")]
    public Button closeBtn;
    public Button submitBtn;
    //public Button nextPageBtn;
    //public Button prevPageBtn;
    public TextMeshProUGUI result;

    Color red = Color.red;
    Color green = Color.green;

    int selected = 0;
    int maxInputFieldCount;

    float hideResultText = 0f;
    const float DURATION_RESULT = 5f;

    IEnumerator Start ()
    {
        maxInputFieldCount = inputFields.Length;

        closeBtn.onClick.AddListener(Close);
        submitBtn.onClick.AddListener(Submit);

        toggles[(int)TOGGLE_ID.MUTE_SOUND].onValueChanged.AddListener(delegate { OnMuteAllSounds(toggles[(int)TOGGLE_ID.MUTE_SOUND]); });

        PopulateCurrentValues();
        CycleThroughInputFields(selected);

        yield return null;
	}

    private void OnEnable()
    {
        //if (GameManager.Instance != null && GameManager.Instance.IsReady) {
        //    PopulateCurrentValues();
        //}
        result.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleInputs();
        UpdateSliderValueText();

        if (hideResultText > 0)
        {
            hideResultText -= Time.deltaTime;
        }
 
        else
        {
            result.gameObject.SetActive(false);
            hideResultText = 0f;
        }
    }

    void OnMuteAllSounds(Toggle toggle)
    {
        TrinaxAudioManager.Instance.MuteAllSounds(toggle.isOn);
    }

    /// <summary>
    /// Handles all inputs.
    /// </summary>
    void HandleInputs() {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Submit();

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();

        if (Input.GetKeyDown(KeyCode.Tab))
            CycleThroughInputFields(++selected);
    }

    /// <summary>
    /// Updates slider text values.
    /// </summary>
    void UpdateSliderValueText()
    {
        sliderValue[(int)SLIDER_ID.MASTER].text = sliders[(int)SLIDER_ID.MASTER].value.ToString("0.0");
        sliderValue[(int)SLIDER_ID.MUSIC].text = sliders[(int)SLIDER_ID.MUSIC].value.ToString("0.0");
        sliderValue[(int)SLIDER_ID.SFX].text = sliders[(int)SLIDER_ID.SFX].value.ToString("0.0");
        sliderValue[(int)SLIDER_ID.UI_SFX].text = sliders[(int)SLIDER_ID.UI_SFX].value.ToString("0.0");
        sliderValue[(int)SLIDER_ID.SFX2].text = sliders[(int)SLIDER_ID.SFX2].value.ToString("0.0");
    }

    /// <summary>
    /// Cycles through all inputfields in the admin panel.
    /// </summary>
    /// <param name="index"></param>
    void CycleThroughInputFields(int index) {
        if (index >= maxInputFieldCount) {
            index       = 0;
            selected    = 0;
        }

        inputFields[index].Select();
        inputFields[index].ActivateInputField();
    }

    /// <summary>
    /// Sets current values to fields.
    /// </summary>
    void PopulateCurrentValues() {

        inputFields[(int)FIELD_ID.SERVER_IP].text = TrinaxAsyncServerManager.Instance.ip;
        inputFields[(int)FIELD_ID.PHOTO_PATH].text = TrinaxGlobal.Instance.photoPath;
        inputFields[(int)FIELD_ID.K_FACTORX].text = TrinaxGlobal.Instance.hand_factorX.ToString();
        inputFields[(int)FIELD_ID.K_FACTORY].text = TrinaxGlobal.Instance.hand_factorY.ToString();
        inputFields[(int)FIELD_ID.K_FACTORZ].text = TrinaxGlobal.Instance.hand_factorZ.ToString();
        inputFields[(int)FIELD_ID.K_OFFSETY].text = TrinaxGlobal.Instance.hand_offsetY.ToString();
        inputFields[(int)FIELD_ID.IDLE_INTERVAL].text = TrinaxGlobal.Instance.idleInterval.ToString();

        inputFields[(int)FIELD_ID.BALL_GAME_DURATION].text = TrinaxGlobal.Instance.ballGameDuration.ToString();
        inputFields[(int)FIELD_ID.RUNNING_GAME_DURATION].text = TrinaxGlobal.Instance.runningGameDuration.ToString();
        inputFields[(int)FIELD_ID.MEMORY_GAME_DURATION].text = TrinaxGlobal.Instance.memoryGameDuration.ToString();

        inputFields[(int)FIELD_ID.BALL_SPAWN_INTERVAL].text = TrinaxGlobal.Instance.ballSpawnInterval.ToString();
        inputFields[(int)FIELD_ID.RUNNING_POWER].text = TrinaxGlobal.Instance.runningPower.ToString();

        inputFields[(int)FIELD_ID.MAX_USER_DISTANCE].text = TrinaxGlobal.Instance.maxUserDistance.ToString();
        inputFields[(int)FIELD_ID.MIN_DETECT_DISTANCE].text = TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT.ToString();

        inputFields[(int)FIELD_ID.DONATION_IDLE_INTERVAL].text = TrinaxGlobal.Instance.donationIdleInterval.ToString();

        //inputFields[(int)FIELD_ID.ARDUINO1].text = TrinaxGlobal.Instance.adnComPort1;
        //inputFields[(int)FIELD_ID.ARDUINO2].text = TrinaxGlobal.Instance.adnComPort2;

        toggles[(int)TOGGLE_ID.USE_SERVER].isOn = TrinaxAsyncServerManager.Instance.useServer;
        toggles[(int)TOGGLE_ID.USE_MOCKY].isOn = TrinaxAsyncServerManager.Instance.useMocky;
        toggles[(int)TOGGLE_ID.MUTE_SOUND].isOn = TrinaxAudioManager.Instance.muteAllSounds;
        toggles[(int)TOGGLE_ID.DEV_MODE].isOn = TrinaxGlobal.Instance.IsDevMode;
    }

    /// <summary>
    /// Saves the value to respective fields.
    /// </summary>
    void Submit() {
        string resultText = "Empty";
        if (string.IsNullOrEmpty(inputFields[(int)FIELD_ID.SERVER_IP].text.Trim())
            || string.IsNullOrEmpty(inputFields[(int)FIELD_ID.PHOTO_PATH].text.Trim()))
        {
            Debug.Log("Mandatory fields in admin panel is empty!");
            result.color = red;
            resultText = "Need to fill mandatory fields!";
        }
        else
        {
            result.color = green;
            resultText = "Success!";

            TrinaxAsyncServerManager.Instance.ip = inputFields[(int)FIELD_ID.SERVER_IP].text.Trim();
            TrinaxGlobal.Instance.photoPath = inputFields[(int)FIELD_ID.PHOTO_PATH].text.Trim();
            TrinaxGlobal.Instance.hand_factorX = float.Parse(inputFields[(int)FIELD_ID.K_FACTORX].text);
            TrinaxGlobal.Instance.hand_factorY = float.Parse(inputFields[(int)FIELD_ID.K_FACTORY].text);
            TrinaxGlobal.Instance.hand_factorZ = float.Parse(inputFields[(int)FIELD_ID.K_FACTORZ].text);
            TrinaxGlobal.Instance.hand_offsetY = float.Parse(inputFields[(int)FIELD_ID.K_OFFSETY].text);
            TrinaxGlobal.Instance.idleInterval = float.Parse(inputFields[(int)FIELD_ID.IDLE_INTERVAL].text);

            TrinaxGlobal.Instance.ballGameDuration = int.Parse(inputFields[(int)FIELD_ID.BALL_GAME_DURATION].text);
            TrinaxGlobal.Instance.runningGameDuration = int.Parse(inputFields[(int)FIELD_ID.RUNNING_GAME_DURATION].text);
            TrinaxGlobal.Instance.memoryGameDuration = int.Parse(inputFields[(int)FIELD_ID.MEMORY_GAME_DURATION].text);

            TrinaxGlobal.Instance.ballSpawnInterval = float.Parse(inputFields[(int)FIELD_ID.BALL_SPAWN_INTERVAL].text);
            TrinaxGlobal.Instance.runningPower = float.Parse(inputFields[(int)FIELD_ID.RUNNING_POWER].text);

            TrinaxGlobal.Instance.maxUserDistance = float.Parse(inputFields[(int)FIELD_ID.MAX_USER_DISTANCE].text);
            TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT = float.Parse(inputFields[(int)FIELD_ID.MIN_DETECT_DISTANCE].text);

            TrinaxGlobal.Instance.donationIdleInterval = float.Parse(inputFields[(int)FIELD_ID.DONATION_IDLE_INTERVAL].text);

            //TrinaxGlobal.Instance.adnComPort1 = inputFields[(int)FIELD_ID.ARDUINO1].text.Trim();
            //TrinaxGlobal.Instance.adnComPort2 = inputFields[(int)FIELD_ID.ARDUINO2].text.Trim();

            TrinaxAsyncServerManager.Instance.useServer = toggles[(int)TOGGLE_ID.USE_SERVER].isOn;
            TrinaxAsyncServerManager.Instance.useMocky = toggles[(int)TOGGLE_ID.USE_MOCKY].isOn;
            TrinaxAudioManager.Instance.muteAllSounds = toggles[(int)TOGGLE_ID.MUTE_SOUND].isOn;
            TrinaxGlobal.Instance.IsDevMode = toggles[(int)TOGGLE_ID.DEV_MODE].isOn;

            TrinaxSaveManager.SaveJson();
        }

        result.text = resultText;
        result.gameObject.SetActive(true);
        hideResultText = DURATION_RESULT;
    }

    /// <summary>
    /// Closes admin panel.
    /// </summary>
    void Close() {
        gameObject.SetActive(false);
    }
}
