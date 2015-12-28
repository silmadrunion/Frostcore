using UnityEngine;
using System.Collections;

[RequireComponent(typeof(P2D_Motor))]
[RequireComponent(typeof(P2D_Animator))]
public class P2D_Controller : MonoBehaviour
{
    public static P2D_Controller Instance;

    private P2DI_DestroyBlock _DestroyBlock;
	private P2DI_PlaceBlock _PlaceBlock;

    private bool isJumping;
    private bool isSprinting;

    private float timeFromLastShiftPress;
    private bool shiftWasPressed;
    private bool isDashing;

    public bool canPlace = false;
    public bool canBreak = false;

	public GameObject block;

    private float timeToFire;

	void Awake() 
    {
        Instance = this;

        _DestroyBlock = GetComponent<P2DI_DestroyBlock>();
		_PlaceBlock = GetComponent<P2DI_PlaceBlock>();
	}
	
	void Update() 
    {
	    if(!isJumping)
        {
            isJumping = Input.GetButtonDown("Jump");
        }

        if (Input.GetButtonDown("Fire1"))
        {
            P2D_Animator.Instance.Attack();

            if (canPlace)
                _DestroyBlock.MiningStart();
            else if (canBreak)
                _PlaceBlock.Place(block);

        }
        else if (Input.GetButtonUp("Fire1"))
        {
            P2D_Animator.Instance.Attack(false);

            _DestroyBlock.MiningStop();
        }

        if (!isSprinting)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        if (!isDashing && !P2D_Motor.Instance.IsDashing)
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
        P2D_Motor.Instance.ImposedUpdate();
	}

    void FixedUpdate()
    {
        var deadZone = 0.1f;
        bool crouch = Input.GetKey(KeyCode.C);

        float moveHorizontal = 0f;

        if(Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            moveHorizontal = Input.GetAxis("Horizontal");

        P2D_Motor.Instance.Move(moveHorizontal, isJumping, crouch, isSprinting);

        P2D_Motor.Instance.Dash(moveHorizontal, isDashing);

        P2D_Motor.Instance.ImposedFixedUpdate();
        P2D_Animator.Instance.ImposedFixedUpdate();

        isJumping = false;
        isSprinting = false;
        isDashing = false;
    }
}
