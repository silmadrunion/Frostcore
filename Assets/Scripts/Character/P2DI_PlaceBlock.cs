using UnityEngine;
using System.Collections;

public class P2DI_PlaceBlock : MonoBehaviour
{

    private Camera _camera;
    public Vector2 blockDimension;
    public LayerMask layerMask;

    public float Range = 10f;

    void Start()
    {
        _camera = Camera.main; 
    }

    public void Place(GameObject block)
    {
        Vector3 pos2 = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 positionClick = new Vector2(pos2.x, pos2.y);
        Vector2 placePosition;
        if (canPlace() == true)
        {
            placePosition.x = (int)(positionClick.x * 2);
            placePosition.y = (int)(positionClick.y * 2);

            if (placePosition.y >= 0)
                placePosition.y = GameMaster.gm.mapGen.height - placePosition.y;
            else
            {
                placePosition.y *= -1;
                placePosition.y += GameMaster.gm.mapGen.height;
            }

            if (block.GetComponent<BlockBreak>() == null)
                return;

            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id = block.GetComponent<BlockBreak>().id;
            Destroy(GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].clone);

            GameObject clone;

            if (placePosition.y > GameMaster.gm.mapGen.height)
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, (float)-((int)placePosition.y - GameMaster.gm.mapGen.height) / 2), Quaternion.identity) as GameObject;
            else
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, GameMaster.gm.mapGen.height / 2 - (float)placePosition.y / 2), Quaternion.identity) as GameObject;

            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].clone = clone;
            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].blockingAmount = 0.10f;
            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].spriteRenderer = clone.GetComponent<SpriteRenderer>();
        }
    }

    private bool canPlace()
    {
        GameObject objectHit = null;
        Vector3 pos2 = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new Vector2(pos2.x, pos2.y);

        try
        {
            objectHit = Physics2D.OverlapPoint(pos, layerMask).gameObject;
            if (objectHit != null)
                return false;
        }
        catch { };

        return true;
    }
}
