using UnityEngine;
using System.Collections;

public enum InventoryEquipSlot
{
    MeleeWeapon, RangedWeapon, Head, Torso, Legs,
    NonEquippable
};

public enum MeleeTypes
{
    Pickaxe
};

public enum RangedTypes
{
    Pistol, Rifle
};

public class Item : MonoBehaviour
{
    public Sprite itemIcon; //The Icon.
    public bool canGet = true; //If we can pick up the Item.
    public bool isUsable;
    public float Weight = 0; // Mass of this (1 item, multiple stacks have this multiplied by stacks). (in kg damn 'muricans)
    public bool stackable = false; //Is it stackable? If yes then items with the same itemType will be stacked.
    public int maxStack = 0; //How many Items each stack can have before creating a new one. Remember that the Items that should be stacked should have the same itemType.
    public int stack = 0; //This is how many stack counts this Item will take up.

    public InventoryEquipSlot EquipSlot;

    public MeleeTypes MeleeType;

    public RangedTypes RangedType;

    //This is the object we will instantiate in the Players hand.
    //We use this so we can have two versions of the weapon. One for picking up and one for using.
    public GameObject equippedWeaponVersion;
    public GameObject itemPlacement;

    //These will store information about usefull components.
    public EquipmentEffect equipmentEffect;

    private bool FPPickUpFound = false;

    [AddComponentMenu("Inventory/Items/Item")]

    //Here we find the components we need.
    void Awake()
    {
        if (GetComponent<EquipmentEffect>())
        {
            equipmentEffect = GetComponent<EquipmentEffect>();
        }
    }

    void Update()
    {
        if (GameMaster.gm.HasGameStarted)
            return;
        else
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    //Picking up the Item.
    public void PickUpItem()
    {
        bool getit = true;
        if (canGet)
        {//if its getable or hasnt been gotten.
            Item locatedit = null;
            Inventory.Instance.gameObject.SendMessage("PlayPickUpSound", SendMessageOptions.DontRequireReceiver); //Play sound
            if (stackable)
            {
                foreach (Transform t in Inventory.Instance.Contents)
                {
                    if (t != null)
                    {
                        if (t.name == this.transform.name)
                        {//if the item we wanna stack this on has the same name
                            Item i = t.GetComponent<Item>();
                            if (i.stack < i.maxStack)
                            {
                                locatedit = i;
                            }
                        }
                    }
                }
                if (locatedit != null)
                {//if we have a stack to stack it to!
                    getit = false;
                    if (locatedit.stack + stack <= locatedit.maxStack && Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        locatedit.stack += stack;
                        Player.Instance.pStats.CarryWeight += Weight * stack;
                        InventoryDisplay.Instance.UpdateInventoryList();
                        Destroy(this.gameObject);
                        MoveToPlayer();
                    }
                    else if (Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        stack -= locatedit.maxStack - locatedit.stack;
                        locatedit.stack = locatedit.maxStack;
                        Inventory.Instance.AddItem(this.transform);
                        MoveMeToThePlayer(Inventory.Instance.itemHolderObject);
                        Player.Instance.pStats.CarryWeight += Weight * stack;
                        MoveToPlayer();
                    }
                    else if (Player.Instance.pStats.CarryWeight + Weight <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        if (locatedit.stack + (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight) <= locatedit.maxStack)
                        {
                            locatedit.stack += (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                            stack -= (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                        }
                        else
                        {
                            stack -= locatedit.maxStack - locatedit.stack;
                            locatedit.stack = locatedit.maxStack;
                            MoveMeToThePlayer(Inventory.Instance.itemHolderObject);
                            Inventory.Instance.AddItem(this.transform);
                        }
                        Player.Instance.pStats.CarryWeight += Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                        MoveToPlayer();
                    }
                    else
                    {
                        Debug.Log("Inventory is full");
                    }
                }
                else
                {
                    getit = true;
                }
            }
            //If we can get it and the inventory isn't full.
            if (getit && Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
            {
                // Chuck it in the inventory
                Inventory.Instance.AddItem(this.transform);
                MoveMeToThePlayer(Inventory.Instance.itemHolderObject);//moves the object, to the player
                MoveToPlayer();
            }
            else if (getit && Player.Instance.pStats.CarryWeight + Weight <= Player.Instance.pStats.MaxCarryWeight)
            {
                Transform clone = Instantiate(gameObject, transform.position, transform.rotation) as Transform;
                clone.GetComponent<Item>().stack -= (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                Player.Instance.pStats.CarryWeight += Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                stack -= (int)Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);

                Inventory.Instance.AddItem(clone);

                MoveToPlayer();
            }
            else if (getit)
            {
                Debug.Log("Inventory is full");
            }
        }
    }

    public void MoveToPlayer()
    {
        if (GetComponent<Rigidbody2D>() == null)
            return;

        GetComponent<Rigidbody2D>().velocity = (Inventory.Instance.transform.parent.position - transform.position).normalized * 10;
    }

    //Moves the item to the Players 'itemHolderObject' and disables it. In most cases this will just be the Inventory object.
    public void MoveMeToThePlayer(Transform itemHolderObject)
    {
        canGet = false;
        transform.gameObject.SetActive(false);
        transform.parent = itemHolderObject;
        transform.localPosition = Vector3.zero;
    }

    //Drops the Item from the Inventory.
    public void DropMeFromThePlayer(bool makeDuplicate)
    {
        GameObject clone;
        if (makeDuplicate == false) //We use this if the object is not stacked and so we can just drop it.
        {
            canGet = true;
            gameObject.SetActive(true);
            transform.parent = null;
            DelayPhysics();

            StartCoroutine(MakeItPickable());
        }
        else //If the object is stacked we need to make a clone of it and drop the clone instead.
        {
            canGet = true;
            clone = Instantiate(gameObject, transform.position, transform.rotation) as GameObject;
            canGet = false;
            clone.SetActive(true);
            clone.transform.parent = null;
            clone.name = gameObject.name;

            clone.GetComponent<Item>().stack = 1;

            clone.SendMessage("MakeItPickable", SendMessageOptions.DontRequireReceiver);
        }
    }

    public IEnumerator MakeItPickable()
    {
        yield return new WaitForSeconds(2f);

        transform.FindChild("ItemPickUp").gameObject.SetActive(true);
    }

    public IEnumerator DelayPickUp()
    {
        if (transform.FindChild("ItemPickUp").GetComponent<Collider>() != null && Inventory.Instance.transform.parent.GetComponent<Collider>() != null)
        {
            Physics.IgnoreCollision(Inventory.Instance.transform.parent.GetComponent<Collider>(), transform.FindChild("ItemPickUp").GetComponent<Collider>(), true);
            yield return new WaitForSeconds(1);
            Physics.IgnoreCollision(Inventory.Instance.transform.parent.GetComponent<Collider>(), transform.FindChild("ItemPickUp").GetComponent<Collider>(), false);
        }
    }

    IEnumerator DelayPhysics()
    {
        if (Inventory.Instance.transform.parent.GetComponent<Collider>() != null && GetComponent<Collider>() != null)
        {
            Physics.IgnoreCollision(Inventory.Instance.transform.parent.GetComponent<Collider>(), GetComponent<Collider>(), true);
            yield return new WaitForSeconds(1);
            Physics.IgnoreCollision(Inventory.Instance.transform.parent.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
    }
}