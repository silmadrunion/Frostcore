using UnityEngine;
using System.Collections;

public class ItemManeuver : MonoBehaviour
{
    public enum ManeuverType
    {
        Pull, PickUp
    }

    public ManeuverType type;

    public float MagneticPower;

    /// <summary>
    /// If an item touches this object's collider, make some manoeuvers
    /// </summary>
    /// <param name="theCollider"> The item's attached collider </param>
    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag != "Item")
            return;

        // If not even 1 piece of this item can fit in the Inventory, it should not move
        if (!OnePieceCanFit(theCollider))
            return;

        // Move the item towards the player
        if (type == ManeuverType.Pull)
        {
            StartCoroutine(FollowPlayer(theCollider));
            theCollider.enabled = false;
            StartCoroutine(ResetCollision(theCollider));
        }
        // Add it to the Inventory
        else if (type == ManeuverType.PickUp)
        {
            Inventory.Instance.PickItem(theCollider.transform.parent.GetComponent<Item>());
        }
    }

    /// <summary>
    /// Checks if 1 piece of the item can fit in the inventory, because it should not move if false
    /// </summary>
    /// <param name="theCollider">The item's attached collider. We are sending this instead of the item component for ease of access</param>
    /// <returns> false if the inventory can't hold one more piece of the sent item</returns>
    protected bool OnePieceCanFit(Collider2D theCollider)
    {
        if (theCollider.GetComponent<Item>() != null)
        {
            if (Player.Instance.pStats.CarryWeight + theCollider.GetComponent<Item>().Weight > Player.Instance.pStats.MaxCarryWeight)
                return false;
        }
        else if (theCollider.transform.parent.GetComponent<Item>() != null)
        {
            if (Player.Instance.pStats.CarryWeight + theCollider.transform.parent.GetComponent<Item>().Weight > Player.Instance.pStats.MaxCarryWeight)
                return false;
        }
        else return false;

        return true;
    }

    IEnumerator FollowPlayer(Collider2D theCollider)
    {
        for (; ; )
        {
            if (theCollider == null || theCollider.gameObject.active == false)
                yield break;

            // If not even 1 piece of this item can fit in the Inventory, it should not move
            if (!OnePieceCanFit(theCollider))
                yield break;

            if (Vector2.Distance(theCollider.transform.position, transform.position) > GetComponent<CircleCollider2D>().radius)
                yield break;

            theCollider.GetComponent<Rigidbody2D>().velocity = (transform.position - theCollider.transform.position).normalized * MagneticPower;
            yield return null;
        }
    }

    IEnumerator ResetCollision(Collider2D theCollider)
    {
        for (; ; )
        {
            if (theCollider == null || theCollider.gameObject.active == false)
            {
                theCollider.enabled = true;
                yield break;
            }

            if(!OnePieceCanFit(theCollider))
            {
                theCollider.enabled = true;
                yield break;
            }

            // If the item is outside the area of action of this magnet, the collider should be re-enabled so the item wont fall into the abyss
            if (Vector2.Distance(theCollider.transform.position, transform.position) > GetComponent<CircleCollider2D>().radius)
                theCollider.enabled = true;

            yield return null;
        }
    }

    public void IgnoreCollision(GameObject target)
    {
        StartCoroutine(_IgnonreCollision(target.GetComponent<Collider2D>()));
    }

    protected IEnumerator _IgnonreCollision(Collider2D theCollider)
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), theCollider, true);

        yield return new WaitForSeconds(2f);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), theCollider, false);
    }
}
