using UnityEngine;
using System.Collections;

[RequireComponent(typeof(P2D_Motor))]
[RequireComponent(typeof(P2D_Animator))]
public class P2D_Controller : MonoBehaviour
{
    private P2D_Motor _Motor;
    private P2D_Animator _Animator;
    private P2DI_DestroyBlock _DestroyBlock;
    private bool isJumping;

    private float timeToFire;

	void Awake() 
    {
        _Motor = GetComponent<P2D_Motor>();
        _Motor.ImposedAwake();

        _Animator = GetComponent<P2D_Animator>();

        _DestroyBlock = GetComponent<P2DI_DestroyBlock>();
	}
	
	void Update() 
    {
	    if(!isJumping)
        {
            isJumping = Input.GetButtonDown("Jump");
        }

        if (Input.GetButton("Fire1"))
        {
            _Animator.Attack();

            _DestroyBlock.MiningStart();
        }
        else
        {
            _DestroyBlock.MiningStop();
        }
	}

    void FixedUpdate()
    {
        var deadZone = 0.1f;
        bool crouch = Input.GetKey(KeyCode.LeftControl);

        float moveHorizontal = 0f;

        if(Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            moveHorizontal = Input.GetAxis("Horizontal");

        _Motor.ImposedFixedUpdate();
        _Motor.Move(moveHorizontal, isJumping, crouch);

        isJumping = false;
    }
}
