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

        public float initialPlayerTemperature = 37f;
        private float _PlayerTemperature;
        public float PlayerTemperature
        {
            get { return _PlayerTemperature; }
            set { _PlayerTemperature = Mathf.Clamp(value, 20, 38); }
        } 

        public void ResetHP()
        {
            curHP = maxHP;
        }

        public void ResetTemperature()
        {
            PlayerTemperature = initialPlayerTemperature;
        }
    }
    public PlayerStats pStats;

    private float curTime;

    public bool IsDead = false;

    public float temperatureLossPerTime = 0.02f;
    public float timeToLoseTemperature = 2f;
    private float timeTemperature;

    void Awake()
    {
        if(pStats == null)
            pStats = new PlayerStats();
    }

    void Start() 
    {
        pStats.ResetHP();
        pStats.ResetTemperature();

        curTime = Time.time;
        timeTemperature = Time.time;

        GameMaster.gm.m_Player = this.transform;
	}
	
	void Update() 
    {
        if (curTime < Time.time)
        {
            pStats.curHP += pStats.regenHP;
            curTime = Time.time + 1f;
        }

        if (Time.time > timeTemperature)
        {
            timeTemperature = Time.time + timeToLoseTemperature;
            pStats.PlayerTemperature -= temperatureLossPerTime;
        }
	}

    void ApplyDamage(float damage)
    {
        if (GetComponent<P2D_Motor>().IsDashing)
            return;
        pStats.curHP -= damage;

        if(pStats.curHP == 0)
        {
            IsDead = true;
            GameMaster.KillPlayer(this);
        }
    }

    public void RecieveHeat(float amount)
    {
        pStats.PlayerTemperature += amount;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Box(pStats.curHP + "HP");
        GUILayout.Box(pStats.PlayerTemperature + " C");

        GUILayout.EndVertical();

        if(IsDead)
        {
            GUI.Box(new Rect(Screen.height / 2 - 25, Screen.width / 2 - 75, 150, 75), "You died");
        }
    }
}
