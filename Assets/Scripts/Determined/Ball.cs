using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using Random = System.Random;

public enum KICKDIRECTION
{
    TOPLEFT = 1,
    TOP,
    TOPRIGHT,
    LEFT,
    MIDDLE,
    RIGHT,
}

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public static System.Action OnBallHit;

    Rigidbody rb;
    SphereCollider ballCol;
    int directionToRandom;
    int totalKickEnums;

    bool ballKicked;
    bool firstKicked;

    float power = 2f;

    Animator anim;

    KICKDIRECTION kickDirType;

    private void Awake()
    {
        totalKickEnums = Enum.GetNames(typeof(KICKDIRECTION)).Length;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void OnEnable()
    {
        anim = GameObject.Find("Bull").GetComponent<Animator>();

        ballCol = GetComponent<SphereCollider>();
        ballCol.enabled = true;
        directionToRandom = Determine.Instance.rand.Next(1, totalKickEnums + 1);
        // kick left
        if (directionToRandom == 1 || directionToRandom == 4 || directionToRandom == 2 || directionToRandom == 5)
        {
            anim.SetTrigger("KickLeft");
        }
        else if(directionToRandom == 3 || directionToRandom == 6)
        {
            anim.SetTrigger("KickRight");
        }
        //Debug.Log(directionToRandom);
        GetInitialRotation(directionToRandom);
        firstKicked = false;

    }

    private void OnDisable()
    {
        ballScored = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.rotation = Quaternion.identity;
        transform.eulerAngles = new Vector3(0, 0, 0);
        rb.isKinematic = false;
    }

    void Update()
    {
        #region KEYBOARD TEST
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    rb.AddRelativeForce(Vector3.back * power + Vector3.up * 0.005f, ForceMode.Impulse);
        //    ballKicked = true;
        //}

        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    //transform.position = currenPos;
        //    rb.velocity = Vector3.zero;
        //    rb.angularVelocity = Vector3.zero;
        //    //rb.rotation = Quaternion.identity;
        //    transform.eulerAngles = new Vector3(0.0f, 15.0f, 0.0f);
        //    ballKicked = false;
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    rb.angularVelocity = Vector3.zero;
        //    rb.rotation = Quaternion.identity;
        //}
        #endregion

        if (!firstKicked)
        {
            firstKicked = true;
            GetFirstKickDirection(directionToRandom);
            //rb.AddRelativeForce(Vector3.back * power + Vector3.up * 0.005f, ForceMode.Impulse);
            ballKicked = true;
        }
    }

    void GetInitialRotation(int _direction)
    {
        TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.KICK_BALL);
        switch (_direction)
        {
            case (int)KICKDIRECTION.TOPLEFT:
                transform.eulerAngles = new Vector3(-3.0f, 0.0f, 0.0f);
                break;
            case (int)KICKDIRECTION.TOP:
                transform.eulerAngles = new Vector3(5.0f, 0.0f, 0.0f);
                break;
            case (int)KICKDIRECTION.TOPRIGHT:
                transform.eulerAngles = new Vector3(-3.0f, 0.0f, 0.0f);
                break;
            case (int)KICKDIRECTION.LEFT:
                transform.eulerAngles = new Vector3(4.0f, 10.0f, 0.0f);
                break;
            case (int)KICKDIRECTION.MIDDLE:
                transform.eulerAngles = new Vector3(2.2f, 7.0f, 0.0f);
                break;
            case (int)KICKDIRECTION.RIGHT:
                transform.eulerAngles = new Vector3(4.0f, -10.0f, 0.0f);
                break;
        }
        //Determine.Instance.fakeBall.SetActive(false);

    }

    void GetFirstKickDirection(int _direction)
    {
        switch (_direction)
        {
            case (int)KICKDIRECTION.TOPLEFT:
                rb.AddRelativeForce(Vector3.back * power + Vector3.up * 0.3f + Vector3.right * 0.05f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.TOP:
                rb.AddRelativeForce(Vector3.back * power, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.TOPRIGHT:
                rb.AddRelativeForce(Vector3.back * power + Vector3.up * 0.3f + Vector3.left * 0.05f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.LEFT:
                rb.AddRelativeForce(Vector3.back * power + Vector3.right * 0.45f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.MIDDLE:
                rb.AddRelativeForce(Vector3.back * power + Vector3.up * 0.005f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.RIGHT:
                rb.AddRelativeForce(Vector3.back * power + Vector3.left * 0.45f, ForceMode.Impulse);
                break;
        }
    }

    void GetKickDirectionCurve(int _direction)
    {
        switch (_direction)
        {
            case (int)KICKDIRECTION.TOPLEFT:
                rb.AddRelativeForce(Vector3.left * 0.003f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.TOP:
                rb.AddRelativeForce(Vector3.up * 0.001f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.TOPRIGHT:
                rb.AddRelativeForce(Vector3.right * 0.005f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.LEFT:
                rb.AddRelativeForce(Vector3.left * 0.005f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.MIDDLE:
                rb.AddRelativeForce(Vector3.right * 0.0075f, ForceMode.Impulse);
                break;
            case (int)KICKDIRECTION.RIGHT:
                rb.AddRelativeForce(Vector3.right * 0.005f, ForceMode.Impulse);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (ballKicked)   // Curve force added each frame
        {
            //rb.AddRelativeForce(Vector3.left * 0.05f, ForceMode.Impulse);

            GetKickDirectionCurve(directionToRandom);
        }
    }

    IEnumerator DisableBall()
    {
        yield return new WaitForSeconds(3.0f);

        gameObject.SetActive(false);
    }

    public bool ballScored = false;
    private void OnCollisionEnter(Collision col)
    {
        // When collide with left hand
        if (col.gameObject.tag == "LeftHand")
        {
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.BALL_BLOCK);
            ballCol.enabled = false;
            ballKicked = false;

            float z = hitFactor(transform.position, col.transform.position, col.collider.bounds.size.z);
            Vector3 dir = new Vector3(1, 1, z).normalized;
            rb.velocity = dir * 20f;

            if (!ballScored)
            {
                ballScored = true;
                OnBallHit?.Invoke();
            }

            StartCoroutine("DisableBall");
        }

        // When collide with right hand
        if (col.gameObject.tag == "RightHand")
        {
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.BALL_BLOCK);
            ballCol.enabled = false;
            ballKicked = false;

            float z = hitFactor(transform.position, col.transform.position, col.collider.bounds.size.z);
            Vector3 dir = new Vector3(1, 1, z).normalized;
            rb.velocity = dir * 20f;

            if (!ballScored)
            {
                ballScored = true;
                OnBallHit?.Invoke();
            }

            StartCoroutine("DisableBall");
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "GoalPost")
        {
            gameObject.SetActive(false);
        }
    }

    float hitFactor(Vector2 ballPos, Vector2 handPos, float handWidth)
    {
        return (ballPos.x - handPos.x) / handWidth;
    }
}
