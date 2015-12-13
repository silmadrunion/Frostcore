using UnityEngine;
using System.Collections;

public class HeatSource : MonoBehaviour 
{
    [SerializeField] private float m_MinHeat = 0.02f;
    [SerializeField] private float m_MaxHeat = 0.20f;
    [SerializeField] private float m_HeatPower = 0.2f;

    private Transform target;
    private float timePassed;

    
	void Update() 
    {
        if (target == null)
            return;

        float Distance = Vector2.Distance(transform.position, target.position);

        if (Time.time > timePassed + 1f)
        {
            float heat = Mathf.Clamp(10 * Mathf.Pow(m_HeatPower, (1 / Distance)), m_MinHeat, m_MaxHeat);
            timePassed = Time.time;
            target.GetComponent<Player>().RecieveHeat(heat);
        }
	}
    
    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Player")
        {
            timePassed = Time.time;
            target = theCollider.transform;
        }
    }

    void OnTriggerExit2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Player")
        {
            target = null;
        }
    }
}
