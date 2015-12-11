using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(AI_Motor))]
[RequireComponent(typeof(Seeker))]
public class AI_Controller : MonoBehaviour 
{
    public Transform Target;
    float Distance;
    [SerializeField] float m_LookAtRange = 25f;
    [SerializeField] float m_ChaseRange = 15f;
    [SerializeField] float m_AttackRange = 1.5f;

    [SerializeField] private float updateRate = 2f;
    private Seeker seeker;
    public Path path;

    [HideInInspector]
    public bool pathIsEnded = false;

    // The max distance from the AI to a waypoint for it to continue to the next waypoint	
    public float nextWaypointDistance = 3;

    // The waypoint we are currently moving towards
    private int currentWaypoint = 0;

    private bool searchingForPlayer = false;

    private AI_Motor _Motor;

    void Awake()
    {
        _Motor = GetComponent<AI_Motor>();
        _Motor.ImposedAwake();
    }

	void Start() 
    {
        seeker = GetComponent<Seeker>();

        if (Target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            return;
        }

        // Start a new path to the target position, return the results on the OnPathComplete method
        seeker.StartPath(transform.position, Target.position, OnPathComplete);

        StartCoroutine(UpdatePath());
	}

    IEnumerator SearchForPlayer()
    {
        GameObject sResult = GameObject.FindGameObjectWithTag("Player");
        if(sResult == null)
        {
            yield return new WaitForSeconds(1f/updateRate);
            StartCoroutine(SearchForPlayer());
        }
        else
        {
            Target = sResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());

            return false;
        }
    }

    IEnumerator UpdatePath()
    {
        if (Target == null)
        {
            if(!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            return false;
        }

        // Start a new path to the target position, return the results on the OnPathComplete method
        seeker.StartPath(transform.position, Target.position, OnPathComplete);

        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    void FixedUpdate()
    {
        if (Target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            if (pathIsEnded)
                return;

            pathIsEnded = true;
            return;
        }

        Distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if(Distance < m_LookAtRange)
        {
            _Motor.LookAt(Target.position);
        }
        if(Distance < m_AttackRange)
        {
            _Motor.Attack(Target);
        }
        else if(Distance < m_ChaseRange)
        {
            Vector2 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;

            _Motor.Move(dir);
        }

        if (Distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        _Motor.ImposedFixedUpdate();
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
