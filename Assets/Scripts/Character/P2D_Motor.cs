using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class P2D_Motor : MonoBehaviour 
{
    [SerializeField] private float m_MaxSpeed = 10f;                    // Player max move speed on X axis.
    [Range(1, 10)][SerializeField] private float m_SprintSpeed = 1.5f;  // Player speed multiplier applied when sprinting 
    [SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
    [SerializeField] private float m_DashSpeed = 40f;                   // How fast will the player move while dashing.
    [SerializeField] private float m_DashLenght = 0.02f;                 // For how long will the player dash.
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;   // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping.
    [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character.

    private Transform GroundCheck;      // A position marking where to check if the player is grounded.
    const float GroundedRadius = .2f;   // Radius of the overlap circle to determine if grounded.
    private bool Grounded;              // Whether or not the player is grounded.
    private Transform CeilingCheck;     // A position marking where to check for ceilings.
    const float CeilingRadius = .01f;   // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D k_Rigidbody2D;  // The Rigidbody2D component attached to the player GameObject.
    bool FacingRight = true;            // For determining which way the player is curently facing.
    public bool IsDashing = false;             // For determining wheather or not the player is dashing.
    float dashTime;                     // To check for how long should the player dash.

    private Transform Graphics;         // Stores the Transform component of the Arm attached to the player.
    private ArmRotation armRotation;    // Used to store the rotation of the arm. 

    float _move;

	public void ImposedAwake() 
    {
	    // Setting up references
        GroundCheck = transform.Find("GroundCheck");
        CeilingCheck = transform.Find("CeilingCheck");
        k_Rigidbody2D = GetComponent<Rigidbody2D>();
	}

    void Start()
    {
        Graphics = transform.FindChild("Graphics");
        armRotation = transform.FindChild("Arm").GetComponent<ArmRotation>();
    }

    public void ImposedUpdate()
    {
        if (armRotation.rotZ > 90 || armRotation.rotZ < -90)
        {
            if (FacingRight)
                Flip();
        }
        else
        {
            if (!FacingRight)
                Flip();
        }
    }
	
	public void ImposedFixedUpdate() 
    {
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject != gameObject)
            {
                Grounded = true;
            }
        }

        if (Time.time > dashTime && IsDashing)
        {
            k_Rigidbody2D.velocity = Vector2.zero;
            IsDashing = false;
            gameObject.layer = 8;
            return; 
        }
        else if(IsDashing)
            k_Rigidbody2D.velocity = new Vector2(Mathf.Clamp(k_Rigidbody2D.velocity.x, -1f, 1f) * m_DashSpeed, k_Rigidbody2D.velocity.y);
    }

    public void Move(float move, bool jump, bool crouch, bool sprint = false)
    {
        // If dashing, don't do anything
        if (IsDashing)
            return;

        // If crouching, check to see if the player can stand up.
        if (!crouch) 
        { 
            if(Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        // Only control the player if grounded or air control is true.
        if (Grounded || m_AirControl)
        {
            // Reduce the speed if crouching by the crouchSpeed multiplier
            move = (crouch ? move * m_CrouchSpeed : move);

            // Multiply the speed if sprinting by the sprintSpeed multiplier
            move = (sprint ? move * m_SprintSpeed : move);

            // Move the character
            k_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, k_Rigidbody2D.velocity.y);

            if (move > 0 && !FacingRight)
            {
                Flip();
            }
            else if (move < 0 && FacingRight)
            {
                Flip();
            }
        }

        // If the player should jump...
        if(Grounded && jump)
        {
            // add a vertical force to the player.
            Grounded = false;
            k_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }

    public void Dash(float move, bool dash)
    {
        if (dash == false)
            return;

        IsDashing = true;
        _move = move;
        gameObject.layer = 11;
        dashTime = Time.time + m_DashLenght;
    }

    void Flip()
    {
        // Switch the way the player is labeled as facing
        FacingRight = !FacingRight;

        Vector3 tempGraphicsLocalScale = Graphics.localScale;
        tempGraphicsLocalScale.x *= -1;
        Graphics.localScale = tempGraphicsLocalScale;

        Vector3 tempArmLocalScale = armRotation.transform.localScale;
        tempArmLocalScale.y *= -1;
        armRotation.transform.localScale = tempArmLocalScale;
    }

    public void Reset()
    {
        if (transform.localScale.x < 0)
        {
            Vector3 tempPlayerLocalScale = transform.localScale;
            tempPlayerLocalScale.x *= -1;
            transform.localScale = tempPlayerLocalScale;
        }

        if (Graphics.localScale.x < 0)
        {
            Vector3 tempGraphicsLocalScale = Graphics.localScale;
            tempGraphicsLocalScale.x *= -1;
            Graphics.localScale = tempGraphicsLocalScale;
        }

        if (armRotation.transform.localScale.x < 0 || armRotation.transform.localScale.y < 0)
        {
            Vector3 tempArmLocalScale = armRotation.transform.localScale;
            if (armRotation.transform.localScale.x < 0)
                tempArmLocalScale.x *= -1;
            if (armRotation.transform.localScale.y < 0)
                tempArmLocalScale.y *= -1;
            armRotation.transform.localScale = tempArmLocalScale;
        }

        FacingRight = true;
    }
}
