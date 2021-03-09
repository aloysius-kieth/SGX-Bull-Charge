//#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class RunningKinectController : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    #region SINGLETON
    public static RunningKinectController Instance { get; set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
        }
    }
    #endregion

    // Kinect User ID
    long? m_userId = null;

    bool detectingUserAcquired = false;
    bool detectingUserLost = false;
    float durationToDetect = 0.5f;

    bool startCalibrating = false;
    public bool doneCalibrating = false;

    public TextMeshProUGUI calibrationText;
    public Image fillImageCalibration;
    public RawImage camDisplay;

    KinectManager kinectMan;

    void Start()
    {
        kinectMan = GetComponent<KinectManager>();
        IsRunning = false;
        fillImageCalibration.fillAmount = 0;
        kinectMan.maxUserDistance = TrinaxGlobal.Instance.maxUserDistance;

        if (!kinectMan.IsUserDetected())
        {
            calibrationText.text = "Waiting for users";
        }
    }

    void Update()
    {
        //if (m_userId.HasValue)
        //{
        //    if (Time.time > nexTime)
        //    {
        //        nexTime += period;
        //        DetectPlayerZ((long)m_userId);
        //    }
        //    if (posZ.z >= 1.7f)
        //    {
        //        Debug.Log("OKKKKKKKKKKKK");
        //    }
        //}

        //if (Energetic.Instance.IsPageActive(ENERGETIC_PAGES.CALIBRATION)
        //    || Energetic.Instance.IsPageActive(ENERGETIC_PAGES.GAME))
        //{
            camDisplay.texture = kinectMan.GetUsersClrTex();
        //}
        //else
        //{
        //    camDisplay.texture = null;
        //}

        // Calibration
        if (m_userId.HasValue && Energetic.Instance.IsPageActive(ENERGETIC_PAGES.CALIBRATION) && !doneCalibrating)
        {
            // Get user distance from the kinect to the user
            DetectUserDistance(m_userId);
            if (startCalibrating)
            {
                StartCoroutine(StartCalibration(m_userId));
            }
        }
#if DEBUG_MODE
        if (Input.GetKeyDown(KeyCode.A))
        {
            Energetic.Instance.ToGame();
        }
#endif
    }

    Vector3 userPos;
    void DetectUserDistance(long? id)
    {
        userPos = kinectMan.GetJointPosition((long)id, (int)KinectInterop.JointType.SpineBase);
        if (userPos.z >= TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT)
        {
            startCalibrating = true;
        }
        //Debug.Log(userPos.z);
    }

    Tweener tweenPhoto = null;
    IEnumerator StartCalibration(long? userId)
    {
        int durationToDetect = 3;

        if (tweenPhoto == null)
        {
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.BEEP);
            tweenPhoto = fillImageCalibration.DOFillAmount(1.0f, durationToDetect).SetEase(Ease.Linear);
        }
        else
        {
            startCalibrating = false;
            yield break;
        }

        for (int i = 0; i < (durationToDetect * 2); i++)
        {
            yield return new WaitForSeconds(0.5f);

            Vector3 posZ = kinectMan.GetJointPosition((long)userId, (int)KinectInterop.JointType.SpineBase);

            if (posZ.z < TrinaxGlobal.Instance.MIN_DISTANCE_FROM_KINECT)
            {
                Debug.Log("User too close to screen!");
                if (tweenPhoto != null)
                {
                    tweenPhoto.Kill();
                }
                tweenPhoto = null;

                calibrationText.text = "Align your position until you hear a beep!";
                fillImageCalibration.DOFillAmount(0.0f, fillImageCalibration.fillAmount > 0.5f ? 0.2f : 0.1f);
                startCalibrating = false;
                yield break;
            }
            else
            {
                calibrationText.text = "Hold it there!";
                Debug.Log("User within range!");
            }
        }

        // Mark calibration as done!
        StartCoroutine(OnCalibrationDone());

        yield return new WaitForSeconds(1.0f);
        startCalibrating = false;
        tweenPhoto = null;
    }

    IEnumerator OnCalibrationDone()
    {
        TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
        Debug.Log("Done calibration!");
        calibrationText.text = "Calibration done!";
        fillImageCalibration.fillAmount = 0;
        //isCalibrating = false;
        doneCalibrating = true;
        startCalibrating = false;

        yield return new WaitForSeconds(2.0f);

        Energetic.Instance.ToGame();
    }

    IEnumerator DetectingUserAcquired(long userId)
    {
        Debug.Log("Already detected user of " + userId + ", confirming user is trying to play game...");
        detectingUserAcquired = true;

        StopCoroutine("DetectingUserLost");

        detectingUserAcquired = false;

        // Delay to confirm that user is trying to play
        //yield return new WaitForSeconds(durationToDetect);
        yield return null;

        if (m_userId.HasValue)
        {
            // TODO: Maybe transit/do something when user has been detected? if not just leave it.
            Debug.Log("User " + userId + " can now interact with kinect");
            calibrationText.text = "Align your position until you hear a beep!";
        }

        detectingUserAcquired = false;
    }

    IEnumerator DetectingUserLost()
    {
        detectingUserLost = true;

        yield return new WaitForSeconds(durationToDetect);

        if (!detectingUserAcquired && !m_userId.HasValue)
        {
            Debug.Log("Clearing all kinect users...");
            calibrationText.text = "You are too far away from the screen!";
            kinectMan.ClearKinectUsers();
        }

        detectingUserLost = false;
    }

    public bool IsRunning { get; set; }
#region KINECT IMPLEMENTATIONS
    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        if (gesture == KinectGestures.Gestures.Run
            && !TrinaxGlobal.Instance.IsGameOver)
        {
            IsRunning = false;
            Debug.Log("<color=green> STOP RUN </color>");
        }
        return true;
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (gesture == KinectGestures.Gestures.Run
            && progress >= 0.8f && !TrinaxGlobal.Instance.IsGameOver)
        {
            IsRunning = true;
            Debug.Log("<color=red> RUN </color>");
        }
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        return true;
    }

    public void UserDetected(long userId, int userIndex)
    {
        Debug.Log("Detected User " + userId);
        m_userId = userId;
        if (m_userId.HasValue)
            kinectMan.DetectGesture((long)m_userId, KinectGestures.Gestures.Run);

        StartCoroutine(DetectingUserAcquired(userId));
    }

    public void UserLost(long userId, int userIndex)
    {
        m_userId = null;

        if (!m_userId.HasValue && !detectingUserLost)
        {
            StartCoroutine("DetectingUserLost");
        }

        Debug.Log("User Lost!");
    }
#endregion
}
