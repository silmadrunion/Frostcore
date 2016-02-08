using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    public float Power; // Number of blocks that you can see thru
    List<Transform> blocksInCollider;
    List<bool> marks;

    public LayerMask discrimination;

    public float UpdateRate = 0.1f;
    float passedTime;

    void Start()
    {
        blocksInCollider = new List<Transform>();
        marks = new List<bool>();

        passedTime = Time.time;
    }

    void Update()
    {
        float monotony = 255 / Power;
        if (passedTime > Time.time)
            return;
        
        passedTime = Time.time + UpdateRate;


        for (int i = 0; i < blocksInCollider.Count; i++ )
        {
            if (marks[i])
                continue;

            try
            {
                RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, blocksInCollider[i].position - transform.position, Vector3.Distance(transform.position, blocksInCollider[i].position), discrimination);
                //Debug.DrawLine(transform.position, b.BlockTransform.position, Color.green, 1f);
                int modifier = 0;
                foreach (RaycastHit2D hitmark in hit)
                {
                    int index;
                    for (index = i; index < blocksInCollider.Count; index++)
                    {
                        if (marks[index])
                            continue;
                        if (blocksInCollider[index] == hitmark)
                            break;
                    }
                    if (modifier <= Power)
                    {
                        if (blocksInCollider[index].tag == "Air")
                        {
                            if (blocksInCollider[index].GetComponent<SpriteRenderer>().color != Color.clear)
                                blocksInCollider[index].GetComponent<SpriteRenderer>().color = Color.clear;
                        }
                        else
                        {
                            if (blocksInCollider[index].GetComponent<SpriteRenderer>().color != new Color(255 - monotony * modifier, 255 - monotony * modifier, 255 - monotony * modifier, 255))
                                blocksInCollider[index].GetComponent<SpriteRenderer>().color = new Color(255 - monotony * modifier, 255 - monotony * modifier, 255 - monotony * modifier, 255);
                            modifier++;
                        }
                    }
                    else
                    {

                    }

                    marks[index] = true;
                }

            }
            catch { };
        }

        for (int i = 0; i < marks.Count; i++)
            marks[i] = false;
    }

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.transform.GetComponent<SpriteRenderer>() != null)
        {
            blocksInCollider.Insert(0, theCollider.transform);
            marks.Insert(0, false);
        }
    }

    void OnTriggerExit2D(Collider2D theCollider)
    {
        theCollider.GetComponent<SpriteRenderer>().color = Color.black;
        int index = blocksInCollider.FindIndex(x => x == theCollider.transform);
        blocksInCollider.RemoveAt(index);
        marks.RemoveAt(index);
    }
}
