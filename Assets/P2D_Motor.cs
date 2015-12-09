﻿using UnityEngine;
using System.Collections;

public class P2D_Motor : MonoBehaviour 
{
    [SerializeField] private float p_MaxSpeed = 10f;                   // Player move speed on X axis.
    [SerializeField] private float p_JumpForce = 400f;                  // Amount of force added when the player jumps.
    [Range(0, 1)][SerializeField] private float p_CrouchSpeed = .36f;   // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [SerializeField] private bool p_AirControl = false;                 // Whether or not a player can steer while jumping.
    [SerializeField] private LayerMask p_WhatIsGround;                  // A mask determining what is ground to the character.

    private Transform GroundCheck;      // A position marking where to check if the player is grounded.
    const float GroundedRadius = .2f;   // Radius of the overlap circle to determine if grounded.
    private bool Grounded;              // Whether or not the player is grounded.
    private Transform CeilingCheck;     // A position marking where to check for ceilings.
    const float CeilingRadius = .01f;   // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D p_Rigidbody2D;  // The Rigidbody2D component attached to the player GameObject.
    bool FacingRight = true;            // For determining which way the player is curently facing.

	void Awake() 
    {
	    // Setting up references
        GroundCheck = transform.Find("GroundCheck");
        CeilingCheck = transform.Find("CeilingCheck");
        p_Rigidbody2D = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate() 
    {
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, p_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject != gameObject)
            {
                Grounded = true;
            }
        }
    }

    public void Move(float move, bool jump, bool crouch)
    {
        // If crouching, check to see if the player can stand up.
        if (!crouch) 
        { 
            if(Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, p_WhatIsGround))
            {
                crouch = true;
            }
        }

        // Only control the player if grounded or air control is true.
        if (Grounded || p_AirControl)
        {
            // Reduce the speed if crouching by the crouchSpeed multiplier
            move = (crouch ? move * p_CrouchSpeed : move);

            // Move the character
            p_Rigidbody2D.velocity = new Vector2(move * p_MaxSpeed, p_Rigidbody2D.velocity.y);

            if (move > 0 && FacingRight)
            {
                Flip();
            }
            else if (move < 0 && !FacingRight)
            {
                Flip();
            }
        }

        // If the player should jump...
        if(Grounded && jump)
        {
            // add a vertical force to the player.
            Grounded = false;
            p_Rigidbody2D.AddForce(new Vector2(0f, p_JumpForce));
        }
    }

    void Flip()
    {
        // Switch the way the player is labeled as facing
        FacingRight = !FacingRight;

        Vector3 tempPlayerLocalScale = transform.localScale;
        tempPlayerLocalScale.x *= -1;
        transform.localScale = tempPlayerLocalScale;
    }
}