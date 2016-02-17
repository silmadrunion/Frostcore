using UnityEngine;
using System.Collections;

public class CollisionBasedDamage : MonoBehaviour 
{
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_KnockbackForce;

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Enemy")
        {
            theCollider.gameObject.SendMessage("ApplyDamage", m_Damage, SendMessageOptions.DontRequireReceiver);

            theCollider.GetComponent<Rigidbody2D>().AddForce((theCollider.transform.position - transform.position).normalized * m_KnockbackForce);
        }
    }
}
