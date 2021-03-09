//#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

public class BallKinectController : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    #region SINGLETON
    public static BallKinectController Instance { get; set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    #endregion

    [Header("Player Hands")]
    public GameObject handLeftObj;
    public GameObject handRightObj;
    Dictionary<KinectInterop.JointType, GameObject> playerTouchPoints;

    // Kinect User ID
    long? m_userId = null;

    Vector3 leftHandPos, rightHandPos;
    Vector3[] targetPos = new Vector3[2];
    Vector3[] velocity = new Vector3[2];
    float smoothing = 0.02f;

    public float FactorX { get { return TrinaxGlobal.Instance.hand_factorX; } }
    public float FactorY { get { return TrinaxGlobal.Instance.hand_factorY; } }
    public float FactorZ { get { return TrinaxGlobal.Instance.hand_factorZ; } }
    public float OffsetY { get { return TrinaxGlobal.Instance.hand_offsetY; } }

    bool detectingUserAcquired = false;
    bool detectingUserLost = false;
    float durationToDetect = 0.5f;

    bool startCalibrating = false;
    public bool doneCalibrating = false;

    public TextMeshProUGUI calibrationText;
    public Image fillImageCalibration;
    public RawImage camDisplay = null;

    KinectManager kinectMan;

    void Start()
    {
        kinectMan = GetComponent<KinectManager>();
        playerTouchPoints = new Dictionary<KinectInterop.JointType, GameObject>
        {
            {KinectInterop.JointType.HandLeft,  handLeftObj},
            {KinectInterop.JointType.HandRight, handRightObj },
        };

        fillImageCalibration.fillAmount = 0;
        kinectMan.maxUserDistance = TrinaxGlobal.Instance.maxUserDistance;

        if (!kinectMan.IsUserDetected())
        {
            calibrationText.text = "Waiting for users";
        }
    }

    float nexTime = 0.0f;
    float period = 1.0f;
    void Update()
    {
        if (m_userId != null && Determine.Instance.IsPageActive(DETERMINE_PAGES.GAME))
        {
            DetectUserHands((long)m_userId);
        }

        // Calibration
        if (m_userId.HasValue && Determine.Instance.IsPageActive(DETERMINE_PAGES.CALIBRATION) && !doneCalibrating)
        {
            // Get user distance from the kinect to the user
            DetectUserDistance(m_userId);
            if (startCalibrating)
            {
                StartCoroutine(StartCalibration(m_userId));
            }
        }

        if(Determine.Instance.IsPageActive(DETERMINE_PAGES.CALIBRATION))
        {
            camDisplay.texture = kinectMan.GetUsersClrTex();
        }
        else
        {
            camDisplay.texture = null;
        }
#if DEBUG_MODE
        if (Input.GetKeyDown(KeyCode.A))
        {
            Determine.Instance.ToGame();
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
        doneCalibrating = true;
        startCalibrating = false;

        yield return new WaitForSeconds(2.0f);

        Determine.Instance.ToGame();
    }

    void DetectUserHands(long userId)
    {
        int i = 0;
        foreach (KeyValuePair<KinectInterop.JointType, GameObject> kvp in playerTouchPoints)
        {
            KinectInterop.JointType joint = kvp.Key;
            GameObject handObj = kvp.Value;

            Vector3 jointPos = kinectMan.GetJointPosition(userId, (int)joint);
            Vector3 jointPosAdjusted = new Vector3(jointPos.x * FactorX, (jointPos.y * FactorY - OffsetY), /*jointPos.z * */FactorZ);
            targetPos[i] = jointPosAdjusted;

            // Move handObj's positions smoothly over time according to kinect hand positions
            handObj.transform.position = Vector3.SmoothDamp(handObj.transform.position, targetPos[i], ref velocity[i], smoothing);

            ++i;
        }
    }

    IEnumerator DetectingUserAcquired(long userId)
    {
        Debug.Log("Already detected user of " + userId + ", confirming user is trying to play game...");
        detectingUserAcquired = true;

        StopCoroutine("DetectingUserLost");

        detectingUserAcquired = false;

        // Delay to confirm that user is trying to play
        yield return new WaitForSeconds(durationToDetect);

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

#region KINECT IMPLEMENTATIONS
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

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        return;
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        return true;
    }
#endregion



}
