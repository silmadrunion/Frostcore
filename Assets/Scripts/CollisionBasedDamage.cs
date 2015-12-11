using UnityEngine;
using System.Collections;

public class CollisionBasedDamage : MonoBehaviour 
{
    [SerializeField] private float m_Damage;

	void Start() 
    {
	
	}
	
	void Update() 
    {
	
	}

    void OnCollisionEnter2D(Collision2D theCollider)
    {
        if (theCollider.gameObject.tag == "Enemy")
        {
            theCollider.gameObject.SendMessage("ApplyDamage", m_Damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
