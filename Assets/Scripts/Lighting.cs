using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    List<Transform> blocksInCollider;
    public LayerMask discrimination;

    public float UpdateRate = 0.1f;
    float passedTime;

    void Start()
    {
        blocksInCollider = new List<Transform>();

        passedTime = Time.time;
    }

    void Update()
    {
        if (passedTime > Time.time)
            return;

        passedTime = Time.time + UpdateRate;

        foreach (Transform block in blocksInCollider)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, block.position - transform.position, Vector3.Distance(transform.position, block.position), discrimination);

            if (hit[0].transform == block)
            {
                if (block.GetComponent<SpriteRenderer>().color != Color.white)
                    block.GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                if (block.GetComponent<SpriteRenderer>().color != Color.black)
                    block.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if(theCollider.transform.GetComponent<SpriteRenderer>() != null)
            blocksInCollider.Add(theCollider.transform);
    }

    void OnTriggerExit2D(Collider2D theCollider)
    {
        foreach (Transform block in blocksInCollider)
            if (block == theCollider.transform)
            {
                blocksInCollider.Remove(block);
                break;
            }
    }
}
