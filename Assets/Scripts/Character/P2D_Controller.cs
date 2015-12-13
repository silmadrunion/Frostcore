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

    private bool isSprinting;
    private float timeFromLastShiftPress;
    private bool shiftWasPressed;
    private bool isDashing;

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

        if (!isSprinting)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        if (!isDashing && !_Motor.IsDashing)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                shiftWasPressed = true;
                timeFromLastShiftPress = Time.time;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (shiftWasPressed == true && Time.time < timeFromLastShiftPress + 0.2f)
                {
                    isDashing = true;
                }
                else
                    isDashing = false;
            }
        }

        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftWasPressed = false;
        }
        _Motor.ImposedUpdate();
	}

    void FixedUpdate()
    {
        var deadZone = 0.1f;
        bool crouch = Input.GetKey(KeyCode.LeftControl);

        float moveHorizontal = 0f;

        if(Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            moveHorizontal = Input.GetAxis("Horizontal");

        _Motor.Move(moveHorizontal, isJumping, crouch, isSprinting);

        _Motor.Dash(moveHorizontal, isDashing);

        _Motor.ImposedFixedUpdate();

        isJumping = false;
        isSprinting = false;
        isDashing = false;
    }
}
