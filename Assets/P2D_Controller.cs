using UnityEngine;
using System.Collections;

[RequireComponent(typeof(P2D_Motor))]
public class P2D_Controller : MonoBehaviour
{

    private P2D_Motor _Motor;
    private bool isJumping;

	void Awake() 
    {
        _Motor = GetComponent<P2D_Motor>();
        _Motor.ImposedAwake();
	}
	
	void Update() 
    {
	    if(!isJumping)
        {
            isJumping = Input.GetButtonDown("Jump");
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
