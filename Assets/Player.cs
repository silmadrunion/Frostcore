using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHP;
        private float _curHP;
        public float curHP
        {
            get { return _curHP; }
            set { _curHP = Mathf.Clamp(value, 0, maxHP); }
        }
        private float _regenHP = 1f;
        public float regenHP
        {
            get { return _regenHP; }
            set { _regenHP = value; }
        }
        
        public void ResetHP()
        {
            curHP = maxHP;
        }
    }
    public PlayerStats pStats;

    private float curTime;

    public bool IsDead = false;

    void Awake()
    {
        if(pStats == null)
            pStats = new PlayerStats();
    }

    void Start() 
    {
        pStats.ResetHP();

        curTime = Time.time;
	}
	
	void Update() 
    {
        if (curTime < Time.time)
        {
            pStats.curHP += pStats.regenHP;
            curTime = Time.time + 1f;
        }

        if (pStats.curHP == 0)
        {
            Dead();
        }
	}

    void ApplyDamage(float damage)
    {
        pStats.curHP -= damage;
    }

    public void Dead()
    {
        IsDead = true;
        transform.gameObject.SetActive(false);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Box(pStats.curHP + "HP");

        GUILayout.EndVertical();

        if(IsDead)
        {
            GUI.Box(new Rect(Screen.height / 2 - 25, Screen.width / 2 - 75, 150, 75), "You died");
        }
    }
}
