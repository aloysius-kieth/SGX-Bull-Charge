using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnManager : MonoBehaviour
{
    float ballSpawnTimer;
    //float timeToSpawnBall = 1f;
    bool doShootBall = false;

    private void Start()
    { 
     
    }

    private void Update()
    {
        if (TrinaxGlobal.Instance.IsGameOver/* || MainManager.Instance.gameState != GAMESTATES.DETERMINED*/)
            return;

        ballSpawnTimer += Time.deltaTime;

        if (ballSpawnTimer > TrinaxGlobal.Instance.ballSpawnInterval)
        {
            SpawnBalls();
        }
    }

    void SpawnBalls()
    {
        ballSpawnTimer = 0;

        GameObject spawnedBall = ObjectPooler.Instance.GetPooledObject("Soccer Ball") as GameObject;
        //randomBallPosX = Random.Range(0, Determine.Instance.HorizontalBounds);

        if(spawnedBall != null)
        {
            spawnedBall.transform.position = new Vector3(0f, -1.50f, 19.08f);
            spawnedBall.SetActive(true);
            //Determine.Instance.fakeBall.SetActive(true);
        }
    }

}
