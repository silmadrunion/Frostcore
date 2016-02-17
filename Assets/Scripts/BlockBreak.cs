﻿using UnityEngine;
using System.Collections;

public class BlockBreak : MonoBehaviour 
{
    public float m_Resistance = 2f;
    public GameObject[] Drop;

	void Start () 
    {
        this.GetComponent<SpriteRenderer>().receiveShadows = true;
        this.GetComponent<SpriteRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}

    public void DropItems()
    {
        foreach (GameObject g in Drop)
        {
            GameObject clone = Instantiate(g, transform.position, transform.rotation) as GameObject;

            clone.name = g.name;
            if (g.GetComponent<Item>() != null)
                StartCoroutine(g.GetComponent<Item>().MakeItPickable());
        }
    }
}
