using UnityEngine;
using System.Collections;

public class P2D_Animator : MonoBehaviour 
{
    [SerializeField] private Animator m_Animator;

    private float attackDelay = 0.33f;
    private float passedTime;

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

	void Start() 
    {
        float passedTime = Time.time;
	}
	
	void FixedUpdate() 
    {
        m_Animator.SetBool("IsAttacking", false);
	}

    public void Attack()
    {
        if (m_Animator.GetBool("IsAttacking")) 
            return;

        m_Animator.SetBool("IsAttacking", true);
    }
}
