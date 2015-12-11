using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class AI_Motor : MonoBehaviour 
{
    [SerializeField] private float m_MaxSpeed = 10f;    // Player max move speed on X axis.
    [SerializeField] private LayerMask m_WhatIsGround;  // A mask determining what is ground to the character.

    private Transform GroundCheck;      // A position marking where to check if the player is grounded.
    const float GroundedRadius = .2f;   // Radius of the overlap circle to determine if grounded.
    private bool Grounded;              // Whether or not the player is grounded.
    private Transform CeilingCheck;     // A position marking where to check for ceilings.
    const float CeilingRadius = .01f;   // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D k_Rigidbody2D;  // The Rigidbody2D component attached to the player GameObject.
    bool FacingRight = true;            // For determining which way the player is curently facing.

    public float Damage = 10f;
    public float attackRepeatTime = 1f;
    public float attackTime;

	public void ImposedAwake() 
    {
        // Setting up references
        GroundCheck = transform.Find("GroundCheck");
        CeilingCheck = transform.Find("CeilingCheck");
        k_Rigidbody2D = GetComponent<Rigidbody2D>();
	}
	
	public void ImposedFixedUpdate() 
    {
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                Grounded = true;
        }
	}

    void Start()
    {
        attackTime = Time.time;
    }

    public void LookAt(Vector2 target)
    {
        if(transform.position.x < target.x && !FacingRight)
        {
            Flip();
        }
        else if(transform.position.x > target.x && FacingRight)
        {
            Flip();
        }
    }

    public void Move(Vector2 move)
    {
        if (Grounded)
        {
            k_Rigidbody2D.AddForce(move);
        }
    }

    public void Attack(Transform target)
    {
        if (Time.time > attackTime)
        {
            target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
            attackTime = Time.time + attackRepeatTime;
        }
    }

    void Flip()
    {
        FacingRight = !FacingRight;

        Vector2 tempLocalScale = transform.localScale;
        tempLocalScale.x *= -1;
        transform.localScale = tempLocalScale;
    }
}
