using UnityEngine;
using System.Collections;

public class HeatSource : MonoBehaviour 
{
    [SerializeField] private float m_MinHeat = 0.02f;
    [SerializeField] private float m_MaxHeat = 0.20f;
    [SerializeField] private float m_HeatPerDistance = 0.01f;
    [SerializeField] private float m_DistanceChange = 0.1f;

    private Transform target;

	void Update() 
    {
        if (target == null)
            return;

        float Distance = Vector2.Distance(transform.position, target.position);

        float heat = Mathf.Clamp(Distance / m_DistanceChange * m_HeatPerDistance, m_MinHeat, m_MaxHeat);

        target.GetComponent<Player>().RecieveHeat(heat);

        Debug.Log(heat);
	}

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Player")
        {
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
