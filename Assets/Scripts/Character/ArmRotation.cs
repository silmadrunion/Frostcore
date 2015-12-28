using UnityEngine;
using System.Collections;

public class ArmRotation : MonoBehaviour 
{
    [SerializeField] public float rotationOffset = 45;

    public float rotZ;

	void Update() 
    {
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();

        rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
	}
}
