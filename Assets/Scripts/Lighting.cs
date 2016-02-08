using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    public struct block
    {
        public Transform BlockTransform;
        public bool marked;
    }

    public float Power; // Number of blocks that you can see thru
    List<block> blocksInCollider;

    public LayerMask discrimination;

    public float UpdateRate = 0.1f;
    float passedTime;

    void Start()
    {
        blocksInCollider = new List<block>();

        passedTime = Time.time;
    }

    void Update()
    {
        float monotony = 255 / Power;
        if (passedTime > Time.time)
            return;
        
        passedTime = Time.time + UpdateRate;


        foreach (block b in blocksInCollider)
        {
            if (b.marked == true)
                continue;

            if (b.BlockTransform.tag == "Air")
                continue;

            try
            {
                RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, b.BlockTransform.position - transform.position, Vector3.Distance(transform.position, b.BlockTransform.position), discrimination);
                //Debug.DrawLine(transform.position, b.BlockTransform.position, Color.green, .1f);
                int modifier = 0;
                foreach (RaycastHit2D hitmark in hit)
                {
                    int index = blocksInCollider.FindIndex(x => x.BlockTransform == hitmark.transform);
                    block aux = new block();
                    aux.BlockTransform = hitmark.transform;

                    if (modifier <= Power)
                    {
                        if (aux.BlockTransform.tag == "Air")
                        {
                            aux.BlockTransform.GetComponent<SpriteRenderer>().color = Color.clear;
                        }
                        else
                        {
                            aux.BlockTransform.GetComponent<SpriteRenderer>().color = new Vector4(255 - monotony * modifier, 255 - monotony * modifier, 255 - monotony * modifier, 255);
                            modifier++;
                        }
                    }
                    else
                    {
                        aux.BlockTransform.GetComponent<SpriteRenderer>().color = Color.black;
                    }

                    aux.marked = true;
                    blocksInCollider[index] = aux;
                }

            }
            catch { };
        }

        for (int i = 0; i < blocksInCollider.Count; i++)
        {
            block aux = new block();
            aux.BlockTransform = blocksInCollider[i].BlockTransform;
            aux.marked = false;
            blocksInCollider[i] = aux;
        }
    }

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.transform.GetComponent<SpriteRenderer>() != null)
        {
            block aux = new block();
            aux.BlockTransform = theCollider.transform;
            aux.marked = false;
            blocksInCollider.Insert(0, aux);
        }
    }

    void OnTriggerExit2D(Collider2D theCollider)
    {
        theCollider.GetComponent<SpriteRenderer>().color = Color.black;
        block aux = new block();
        aux.BlockTransform = theCollider.transform;
        aux.marked = false;
        blocksInCollider.Remove(aux);
    }
}
