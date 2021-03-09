using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public GameObject cardBack;
    public GameObject ps;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(MemoryGameManager.durationToShowCards);
        transform.DORotate(new Vector3(0, 180, 0), 0.25f);
    }

    public void OnMouseDown()
    {
        if (TrinaxGlobal.Instance.IsGameOver)
            return;

        if (cardBack.activeSelf && MemoryGameManager.Instance.canReveal)
        {
            this.transform.DORotate(new Vector3(0, 0, 0), 0.25f);
            cardBack.SetActive(false);
            MemoryGameManager.Instance.CardRevealed(this);
            ps.SetActive(true);
        }
    }

    int id;
    public int ID { get { return id; } }

    public void ChangeSprite(int _id, Sprite image)
    {
        id = _id;
        // For changing sprite
        GetComponent<SpriteRenderer>().sprite = image;
    }

    public void OnReveal()
    {
        this.transform.DORotate(new Vector3(0, 180, 0), 0.25f);
        cardBack.SetActive(true);
        ps.SetActive(false);
    }
}
