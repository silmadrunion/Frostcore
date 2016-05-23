﻿using UnityEngine;
using System.Collections;

public class P2DI_DestroyBlock : MonoBehaviour
{
    public float Range = 10f;
    public LayerMask layerMask;

    private bool _isMining;
    private Camera _camera;
    private float _m_Resistance;
    public float _speed = 1;
    private float _time;
    private GameObject _block;
    private GameObject _previousBlock = null;

    void Start()
    {
        _isMining = false;
        _camera = Camera.main;
    }

    public void MiningStart()
    {
        _time = 0;
        _isMining = true;
    }

    void Update()
    {
        if (_isMining == true)
        {
            if (isBreakable() == true)
            {
                if (Vector3.Distance(transform.position, _block.transform.position) > Range)
                {
                    _time += Time.deltaTime * _speed;
                    return;
                }

                if (_previousBlock != _block)
                {
                    _time = Time.deltaTime * _speed;
                    _previousBlock = _block;
                }
                else
                    _time += Time.deltaTime * _speed;
            }
            else
                _time = 0;
        }
        if (_time >= _m_Resistance && _m_Resistance != 0)
        {
            Vector3 tempPos = _block.transform.position;
            _block.SendMessage("DropItems", SendMessageOptions.DontRequireReceiver);

            Vector2 destroyPos;
            destroyPos.x = (int)(_block.transform.position.x * 2);
            destroyPos.y = (int)(_block.transform.position.y * 2);

            if (destroyPos.y >= 0)
                destroyPos.y = GameMaster.gm.mapGen.height - destroyPos.y;
            else
            {
                destroyPos.y *= -1;
                destroyPos.y += GameMaster.gm.mapGen.height;
            }

            GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].id = 0;
            Destroy(GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].clone);

            GameObject clone;

            if(destroyPos.y > GameMaster.gm.mapGen.height)
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].id.ToString(), typeof(GameObject)), new Vector3((float)destroyPos.x / 2, (float)-((int)destroyPos.y - GameMaster.gm.mapGen.height) / 2), Quaternion.identity) as GameObject;
            else
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].id.ToString(), typeof(GameObject)), new Vector3((float)destroyPos.x / 2, GameMaster.gm.mapGen.height / 2 - (float)destroyPos.y / 2), Quaternion.identity) as GameObject;

            GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].clone = clone;
            GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].blockingAmount = 0;
            GameMaster.gm.Map[(int)destroyPos.x][(int)destroyPos.y].spriteRenderer = clone.GetComponent<SpriteRenderer>();
        }
    }

    // Check to see if the player clicked an object with the Breakable tag
    private bool isBreakable()
    {
        GameObject objectHit = null;
        Vector3 pos2 = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new Vector2(pos2.x, pos2.y);

        try
        {
            objectHit = Physics2D.OverlapPoint(pos, layerMask).gameObject;
            if (objectHit.tag == "Breakable")
            {
                _block = objectHit;
                _m_Resistance = objectHit.GetComponent<BlockBreak>().m_Resistance;
                return true;
            }
        }
        catch { };

        return false;
    }

    public void MiningStop()
    {
        _time = 0;
        _isMining = false;
    }
}
