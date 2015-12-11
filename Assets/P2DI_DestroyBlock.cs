using UnityEngine;
using System.Collections;

public class P2DI_DestroyBlock : MonoBehaviour {

    private bool _inMining;
    private Camera _camera;
    private float _m_Resistance;
    private float _speed;
    private float _time;
    private Transform _block;

	void Start () {
        _inMining = false;
        _camera = Camera.main;
	}

	public void MiningStart()
    {
        _time = 0;
        _inMining = true;
    }

    void Update()
    {
        Transform _previousBlock = null;
        if (_inMining == true)
        {
            if (isBreakabel() == true)
            {
                if (_previousBlock != _block)
                    _time = 0;
                _previousBlock = _block;
                _time += Time.deltaTime * _speed;
            }
            else
                _time = 0;
        }
        if (_time>=_m_Resistance)
        {
            Destroy(_block.gameObject);
        }
    }
    
    //verifica daca jucatorul a dat click pe un obiect care are tagul "Breakabel"
    private bool isBreakabel()
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            if (objectHit.tag == "Breakabel")
            {
                _block = objectHit;
                _m_Resistance = objectHit.GetComponent<BlockBreak>().m_Resistance;
                return true;
            }
        }
        return false;
    }

    public void MiningStop()
    {
        _time = 0;
        _inMining = false;
    }
}
