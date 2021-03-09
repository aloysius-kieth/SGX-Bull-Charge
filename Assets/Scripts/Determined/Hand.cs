using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    Vector3 lastPos;
    Vector3 currentPos;
    float velocity;

    public float GetVelocity()
    {
        return velocity * 5;
    }

    private void Update()
    {
        if (TrinaxGlobal.Instance.IsGameOver)
            return;

        currentPos = transform.position;

        velocity = ((currentPos - lastPos).magnitude) / Time.deltaTime;

        lastPos = transform.position;

    }

}
