using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AI_Motor))]
public class AI_Controller : MonoBehaviour 
{

    public Transform Target;
    float Distance;
    [SerializeField] float a_LookAtRange = 25f;
    [SerializeField] float a_ChaseRange = 15f;
    [SerializeField] float a_AttackRange = 1.5f;

    private AI_Motor _Motor;

    void Awake()
    {
        _Motor = GetComponent<AI_Motor>();
        _Motor.ImposedAwake();
    }

	void Start() 
    {
        Target = GameObject.FindGameObjectWithTag("Player").transform;
	}

    void FixedUpdate()
    {
        Distance = Vector2.Distance(transform.position, Target.position);
        
        float moveHorizontal = 0f;

        if(Distance < a_LookAtRange)
        {
            _Motor.LookAt(Target.position);
        }
        if(Distance < a_AttackRange)
        {
            _Motor.Attack(Target);
        }
        else if(Distance < a_ChaseRange)
        {
            if (Target.position.x > transform.position.x)
                moveHorizontal = 1f;
            else if (Target.position.x < transform.position.x)
                moveHorizontal = -1f;

            _Motor.Move(moveHorizontal);
        }

        _Motor.ImposedFixedUpdate();
    }
}
