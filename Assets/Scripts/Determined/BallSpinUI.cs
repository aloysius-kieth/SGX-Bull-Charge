using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpinUI : MonoBehaviour
{
    public float rotatePow = 9f;
    void Update()
    {
        transform.Rotate(new Vector3(1, -1 * rotatePow, 1));
    }

}
