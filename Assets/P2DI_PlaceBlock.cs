using UnityEngine;
using System.Collections;

public class P2DI_PlaceBlock : MonoBehaviour {

	private Camera _camera;
	public Vector2 blockDimension;

	void Start () {
		_camera = Camera.main;
	}

	public void Place(GameObject block)
	{
		Vector3 pos2 = _camera.ScreenToWorldPoint (Input.mousePosition);
		Vector2 positionClick = new Vector2 (pos2.x, pos2.y);
		Vector2 placePosition;
		Debug.Log (positionClick);
		if (canPlace () == true)
		{
			placePosition.x = ((int)((int)positionClick.x/(int)blockDimension.x)*blockDimension.x);
			placePosition.y = ((int)((int)positionClick.y/(int)blockDimension.y)*blockDimension.y);
			Instantiate(block, placePosition, Quaternion.identity);
		}
	}

	private bool canPlace()
	{
		GameObject objectHit=null;
		Vector3 pos2 = _camera.ScreenToWorldPoint (Input.mousePosition);
		Vector2 pos = new Vector2 (pos2.x, pos2.y);

		try{
			objectHit = Physics2D.OverlapPoint(pos).gameObject;
			if (objectHit==null)
				return false;
		}catch{};

		return true;
	}
}
