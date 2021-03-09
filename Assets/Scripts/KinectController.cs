using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectController : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    #region SINGLETON
    public static KinectController Instance { get; set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    [Header("Player Hands")]
    public GameObject handLeftObj;
    public GameObject handRightObj;
    Dictionary<KinectInterop.JointType, GameObject> playerTouchPoints;

    // Kinect User ID
    long? m_userId = null;

    public float FactorX { get { return TrinaxGlobal.Instance.hand_factorX; } }
    public float FactorY { get { return TrinaxGlobal.Instance.hand_factorY; } }
    public float OffsetY { get { return TrinaxGlobal.Instance.hand_offsetY; } }

    bool detectingUserAcquired = false;
    bool detectingUserLost = false;
    float durationToDetect = 4.0f;

    // Component caching
    KinectManager kinectMan;

    void Start()
    {
        kinectMan = GetComponent<KinectManager>();
        playerTouchPoints = new Dictionary<KinectInterop.JointType, GameObject>
        {
            {KinectInterop.JointType.HandLeft,  handLeftObj},
            {KinectInterop.JointType.HandRight, handRightObj },
        };
    }

    void Update()
    { 
        if(m_userId != null /*&& !MainManager.Instance.GameOver */
                            /*&& MainManager.Instance.gameState == GAMESTATES.DETERMINED*/)
        {
            DetectUserHands((long)m_userId);
        }
    }

    Vector3 leftHandPos, rightHandPos;
    Vector3[] targetPos = new Vector3[2];
    Vector3[] velocity = new Vector3[2];
    float smoothing = 0.02f;
    void DetectUserHands(long userId)
    {
        int i = 0;
        foreach (KeyValuePair<KinectInterop.JointType, GameObject> kvp in playerTouchPoints)
        {
            KinectInterop.JointType joint = kvp.Key;
            GameObject handObj = kvp.Value;

            Vector3 jointPos = kinectMan.GetJointPosition(userId, (int)joint);
            Vector3 jointPosAdjusted = new Vector3(jointPos.x * FactorX, (jointPos.y * FactorY - OffsetY), jointPos.z);
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
            kinectMan.ClearKinectUsers();
        }

        detectingUserLost = false;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        if (gesture == KinectGestures.Gestures.Run 
            && !TrinaxGlobal.Instance.IsGameOver 
            /*&& MainManager.Instance.gameState == GAMESTATES.ENERGETIC*/)
        {
            Debug.Log("<color=green> STOP RUN </color>"); 
            // TODO: set game over to true when user stops running for energetic game
        }
        return true;
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        return true;
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (gesture == KinectGestures.Gestures.Run 
            && progress >= 0.8f && !TrinaxGlobal.Instance.IsGameOver
            /*&& MainManager.Instance.gameState == GAMESTATES.ENERGETIC*/)
        {
            Debug.Log("<color=red> RUN </color>");
        }
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
}
