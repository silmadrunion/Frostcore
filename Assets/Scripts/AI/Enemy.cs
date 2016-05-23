using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{
    [System.Serializable]
    public class EnemyStats
    {
        public float maxHP;
        private float _curHP;
        public float curHP
        {
            get { return _curHP; }
            set { _curHP = Mathf.Clamp(value, 0, maxHP); }
        }

        public void ResetHP()
        {
            curHP = maxHP;
        }
    }

    public EnemyStats eStats;

	void Start () 
    {
        eStats.ResetHP();
	}
	
	void Update () 
    {

	}

    public void ApplyDamage(float damage)
    {
        eStats.curHP -= damage;

        if (eStats.curHP == 0)
        {
            GameMaster.KillEnemy(this);
        }
    }
}
