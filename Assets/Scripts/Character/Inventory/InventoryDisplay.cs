using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the UI coresponding with Inventory
/// </summary>
public class InventoryDisplay : MonoBehaviour
{
    /// <summary>
    /// Instance for quick access
    /// </summary>
    public static InventoryDisplay Instance;

    void Start()
    {
        Instance = this;

        ItemIcons = new List<RectTransform>();

        UpdateInventoryList();
    }

    public RectTransform InventoryUIReference;

    /// <summary>
    /// Required input to activate/deactivate the inventory
    /// </summary>
    public KeyCode onOffButton;
    /// <summary>
    /// Is the inventory activated?
    /// </summary>
    public bool displayInventory;

    /// <summary>
    /// Where we will create/destroy/manage icons
    /// </summary>
    public RectTransform InventoryItemList;
    /// <summary>
    /// The position at which the next instantiated icon will be placed
    /// </summary>
    [SerializeField]
    Vector3 lastPositionInList;
    public float PositionGap;
    public float SmallShadowIconPositionGap;
    /// <summary>
    /// The scale the icon should have. Must be manually set.
    /// </summary>
    [SerializeField]
    Vector3 targetLocalScale;

    /// <summary>
    /// The icons coresponding with each Item in Inventory
    /// </summary>
    public List<RectTransform> ItemIcons;
    /// <summary>
    /// For Instantiation purposes
    /// </summary>
    public GameObject IconPrefab;
    /// <summary>
    /// What will be shown over an icon on the GUI when holding an dragged item over another item
    /// </summary>
    public GameObject ShadowIconNormalPrefab;
    /// <summary>
    /// What will be shown between two icons on the GUI when holding an dragged item over the space between two items 
    /// </summary>
    public GameObject ShadowIconSmallPrefab;

    public RectTransform CarryWeightBar;

    /// <summary>
    /// Shows items in UI properly
    /// </summary>
    public void UpdateInventoryList()
    {
        // Resets the Carry Weight, to update it
        Player.Instance.pStats.CarryWeight = 0;

        // Checks if there are more icons than items
        if (Inventory.Instance.Contents.Count < ItemIcons.Count)
        {
            // If true, then remove surplus icons
            for (int index = ItemIcons.Count - 1; index >= Inventory.Instance.Contents.Count; index--)
            {
                RemoveItemIcon(ItemIcons[index]);
            }
        }
        // Checks if there are more items than icons
        else if (Inventory.Instance.Contents.Count > ItemIcons.Count)
        {
            // If true, then add more icons;
            for (int index = ItemIcons.Count; index < Inventory.Instance.Contents.Count; index++)
            {
                CreateItemIcon(Inventory.Instance.Contents[index]);
            }
        }

        if (Inventory.Instance.Contents.Count == ItemIcons.Count)
        {
            for (int index = 0; index < ItemIcons.Count; index++)
            {
                // Checks if the item displayed is the same as the item in Contents
                if (int.Parse(ItemIcons[index].name) != Inventory.Instance.Contents[index].ID)
                {
                    // If not, assign the item to the icon
                    AssignItemToIcon(ItemIcons[index], Inventory.Instance.Contents[index]);
                }
                // We check to see if the Count of this item is displayed as it should
                else if (int.Parse(ItemIcons[index].GetChild(3).GetComponent<Text>().text) != Inventory.Instance.Contents[index].Count)
                {
                    // We assign the item again, just to be sure
                    AssignItemToIcon(ItemIcons[index], Inventory.Instance.Contents[index]);
                }

                int indexHB = Array.IndexOf(Inventory.Instance.HotbarContents, index);
                
                if (indexHB != -1)
                {
                    ItemIcons[index].GetChild(4).GetComponent<Text>().text = "In Hotbar -> " + (indexHB + 1).ToString();
                    ItemIcons[index].GetChild(4).GetComponent<Text>().color = Color.red;
                    Inventory.Instance.HotbarButtons[indexHB].GetChild(0).GetComponent<Image>().sprite = Inventory.Instance.Contents[index].Icon;
                    Inventory.Instance.HotbarButtons[indexHB].GetChild(0).GetComponent<Image>().color = Color.white;
                }
                else
                    ItemIcons[index].GetChild(4).GetComponent<Text>().color = Color.clear;

                // Adds this item's weight to the CarryWeight
                Player.Instance.pStats.CarryWeight += Inventory.Instance.Contents[index].Weight * Inventory.Instance.Contents[index].Count;
            }

            for (int index = 0; index < Inventory.Instance.HotbarContents.Length; index++)
            {
                if(Inventory.Instance.HotbarContents[index] == -1)
                    Inventory.Instance.HotbarButtons[index].GetChild(0).GetComponent<Image>().color = Color.clear;
            }
        }
        // In case some error occured, rearrange the inventory UI
        else
            UpdateInventoryList();
    }

    void CreateItemIcon(Item item)
    {
        RectTransform clone = (Instantiate(IconPrefab) as GameObject).GetComponent<RectTransform>();
        clone.transform.parent = InventoryItemList.transform;
        clone.transform.localPosition = lastPositionInList;
        clone.transform.localScale = targetLocalScale;
        lastPositionInList = new Vector3(lastPositionInList.x, lastPositionInList.y - PositionGap, lastPositionInList.z);
        InventoryItemList.sizeDelta = new Vector2(InventoryItemList.sizeDelta.x, InventoryItemList.sizeDelta.y + PositionGap);

        clone.gameObject.name = item.ID.ToString();

        clone.GetChild(1).GetComponent<Image>().sprite = item.Icon;
        clone.GetChild(2).GetComponent<Text>().text = item.Name;
        clone.GetChild(3).GetComponent<Text>().text = item.Count.ToString();

        ItemIcons.Add(clone);
    }

    void RemoveItemIcon(RectTransform icon)
    {
        Destroy(icon.gameObject);
        ItemIcons.Remove(icon);
        InventoryItemList.sizeDelta = new Vector2(InventoryItemList.sizeDelta.x, InventoryItemList.sizeDelta.y - PositionGap);
        lastPositionInList = new Vector3(lastPositionInList.x, lastPositionInList.y + PositionGap, lastPositionInList.z);
    }

    void AssignItemToIcon(RectTransform icon, Item item)
    {
        icon.gameObject.name = item.ID.ToString();

        icon.GetChild(1).GetComponent<Image>().sprite = item.Icon;
        icon.GetChild(2).GetComponent<Text>().text = item.Name;
        icon.GetChild(3).GetComponent<Text>().text = item.Count.ToString();
    }

    private GameObject lastSelectedGameObject;
    private GameObject shadowItemIcon = null;

    void Update()
    {
        if (GameMaster.gm.isMenuActive)
            return;

        if (Input.GetKeyDown(onOffButton))
        {
            Inventory.Instance.draggedItem.ClearDraggedItem();
            InventoryUIReference.gameObject.SetActive(!displayInventory);

            //TODO: Play sound on open or close.

            displayInventory = !displayInventory;
        }

        if (!displayInventory)
            return;

        if (lastSelectedGameObject != null && lastSelectedGameObject != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelectedGameObject.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            lastSelectedGameObject = null;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!Inventory.Instance.draggedItem.isAnItemDragged)
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    int nameToInt = 10;
                    int.TryParse(EventSystem.current.currentSelectedGameObject.name, out nameToInt);
                    if (ItemIcons.Find(x => x.gameObject == EventSystem.current.currentSelectedGameObject) != null)
                    {
                        int itemIndex = ItemIcons.FindIndex(x => x.gameObject == EventSystem.current.currentSelectedGameObject);

                        EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>().color = new Color32(101, 57, 57, 255);
                        lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        Inventory.Instance.draggedItem.DragItem(itemIndex);
                    }
                    else if (nameToInt <= 8)
                    {
                        Inventory.Instance.HotbarContents[nameToInt] = -1;
                        UpdateInventoryList();
                    }
                }
            }
            else
            {
                RectTransform RectTransformHit = null; 

                if(Physics2D.OverlapPoint(Input.mousePosition) != null)
                    RectTransformHit = Physics2D.OverlapPoint(Input.mousePosition).gameObject.GetComponent<RectTransform>();

                int indexHBdrgd = Array.IndexOf(Inventory.Instance.HotbarContents, Inventory.Instance.draggedItem.indexInContents);

                if (RectTransformHit != null && ItemIcons.Contains(RectTransformHit))
                {
                    int itemIndex = ItemIcons.FindIndex(x => x == RectTransformHit);

                    if (itemIndex != Inventory.Instance.draggedItem.indexInContents)
                    {
                        Helper.Swap4List<Item>(Inventory.Instance.Contents, itemIndex, Inventory.Instance.draggedItem.indexInContents);
                        if (indexHBdrgd != -1)
                            Inventory.Instance.HotbarContents[indexHBdrgd] = -1;

                        int indexHBtrgt = Array.IndexOf(Inventory.Instance.HotbarContents, itemIndex);
                        if(indexHBtrgt != -1)
                            Inventory.Instance.HotbarContents[indexHBtrgt] = -1;
                    }

                    Inventory.Instance.draggedItem.ClearDraggedItem();
                }
                else if(RectTransformHit == null)
                {
                    RectTransform RectTransformHitUp = null;

                    if( Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y + PositionGap / 2)) != null)
                        RectTransformHitUp = Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y + PositionGap / 2)).gameObject.GetComponent<RectTransform>();

                    RectTransform RectTransformHitDown = null;

                    if (Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y - PositionGap / 2)) != null)
                        RectTransformHitDown = Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y - PositionGap / 2)).gameObject.GetComponent<RectTransform>();

                    if (RectTransformHitUp != null && ItemIcons.Contains(RectTransformHitUp) && RectTransformHitDown != null && ItemIcons.Contains(RectTransformHitDown))
                    {
                        int itemIndex = ItemIcons.FindIndex(x => x == RectTransformHitUp);

                        Helper.Swap4List<Item>(Inventory.Instance.Contents, itemIndex, Inventory.Instance.draggedItem.indexInContents);
                        if (indexHBdrgd != -1)
                            Inventory.Instance.HotbarContents[indexHBdrgd] = -1;
                        int indexHBtrgt = Array.IndexOf(Inventory.Instance.HotbarContents, itemIndex);
                        if (indexHBtrgt != -1)
                            Inventory.Instance.HotbarContents[indexHBtrgt] = -1;
                        Inventory.Instance.draggedItem.ClearDraggedItem();
                    }
                    else if (RectTransformHitUp != null && ItemIcons.Contains(RectTransformHitUp))
                    {
                       Helper.Swap4List<Item>(Inventory.Instance.Contents, Inventory.Instance.Contents.Count - 1, Inventory.Instance.draggedItem.indexInContents);
                       if (indexHBdrgd != -1)
                           Inventory.Instance.HotbarContents[indexHBdrgd] = -1;
                       int indexHBtrgt = Array.IndexOf(Inventory.Instance.HotbarContents, Inventory.Instance.Contents.Count - 1);
                       if (indexHBtrgt != -1)
                           Inventory.Instance.HotbarContents[indexHBtrgt] = -1;
                       Inventory.Instance.draggedItem.ClearDraggedItem();
                    }
                    else if (RectTransformHitDown != null && ItemIcons.Contains(RectTransformHitDown))
                    {
                        Helper.Swap4List<Item>(Inventory.Instance.Contents, 0, Inventory.Instance.draggedItem.indexInContents);
                        if (indexHBdrgd != -1)
                            Inventory.Instance.HotbarContents[indexHBdrgd] = -1;
                        int indexHBtrgt = Array.IndexOf(Inventory.Instance.HotbarContents, 0);
                        if (indexHBtrgt != -1)
                            Inventory.Instance.HotbarContents[indexHBtrgt] = -1;
                        Inventory.Instance.draggedItem.ClearDraggedItem();                        
                    }
                    else if (!IsPointerOverUIObject())
                    {
                        Inventory.Instance.DropItem(Inventory.Instance.Contents[Inventory.Instance.draggedItem.indexInContents]);
                        if (indexHBdrgd != -1)
                            Inventory.Instance.HotbarContents[indexHBdrgd] = -1;
                        Inventory.Instance.draggedItem.ClearDraggedItem();
                    }
                }

                if(EventSystem.current.currentSelectedGameObject != null)
                {
                    int nameToInt = 10;
                    int.TryParse(EventSystem.current.currentSelectedGameObject.name, out nameToInt);
                    if (nameToInt <= 8)
                    {
                        if (Character.Instance.IsRightType(Inventory.Instance.Contents[Inventory.Instance.draggedItem.indexInContents], Item.ItemType.Itemblock))
                        {
                            Inventory.Instance.HotbarContents[nameToInt] = Inventory.Instance.draggedItem.indexInContents;
                            Inventory.Instance.draggedItem.ClearDraggedItem();
                        }
                    }
                }

                UpdateInventoryList();
            }
        }
        else
        {
            if(Inventory.Instance.draggedItem.isAnItemDragged)
            {
                RectTransform RectTransformHit = null;

                if (Physics2D.OverlapPoint(Input.mousePosition) != null)
                    RectTransformHit = Physics2D.OverlapPoint(Input.mousePosition).gameObject.GetComponent<RectTransform>();

                if (RectTransformHit != null && ItemIcons.Contains(RectTransformHit) && RectTransformHit != ItemIcons[Inventory.Instance.draggedItem.indexInContents])
                {
                    if (shadowItemIcon != null)
                    {
                        if (Physics2D.OverlapPoint(shadowItemIcon.transform.position) != RectTransformHit.GetComponent<Collider2D>())
                            Destroy(shadowItemIcon);
                    }
                    if (shadowItemIcon == null && RectTransformHit != ItemIcons[Inventory.Instance.draggedItem.indexInContents])
                    {
                        shadowItemIcon = Instantiate(ShadowIconNormalPrefab) as GameObject;
                        shadowItemIcon.transform.parent = InventoryItemList;
                        shadowItemIcon.transform.position = RectTransformHit.position;
                        shadowItemIcon.transform.localScale = Vector3.one;
                    }
                }
                else if (RectTransformHit == null)
                {
                    RectTransform RectTransformHitUp = null;

                    if (Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y + PositionGap / 2)) != null)
                        RectTransformHitUp = Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y + PositionGap / 2)).gameObject.GetComponent<RectTransform>();

                    RectTransform RectTransformHitDown = null;

                    if (Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y - PositionGap / 2)) != null)
                        RectTransformHitDown = Physics2D.OverlapPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y - PositionGap / 2)).gameObject.GetComponent<RectTransform>();

                    if (RectTransformHitUp != null && ItemIcons.Contains(RectTransformHitUp) && RectTransformHitDown != null && ItemIcons.Contains(RectTransformHitDown))
                    {
                        if (shadowItemIcon != null)
                        {
                            if (Physics2D.OverlapPoint(new Vector2(shadowItemIcon.transform.position.x, shadowItemIcon.transform.position.y + SmallShadowIconPositionGap)) != RectTransformHitUp.GetComponent<Collider2D>())
                                Destroy(shadowItemIcon);
                        }
                        else
                        {
                            shadowItemIcon = Instantiate(ShadowIconSmallPrefab) as GameObject;
                            shadowItemIcon.transform.parent = InventoryItemList;
                            shadowItemIcon.transform.position = new Vector3(RectTransformHitUp.transform.position.x, RectTransformHitUp.transform.position.y - SmallShadowIconPositionGap, RectTransformHitUp.transform.position.z);
                            shadowItemIcon.transform.localScale = Vector3.one;
                        }
                    }
                    else if (RectTransformHitUp != null && ItemIcons.Contains(RectTransformHitUp))
                    {
                        if (shadowItemIcon != null)
                        {
                            if (Physics2D.OverlapPoint(new Vector2(shadowItemIcon.transform.position.x, shadowItemIcon.transform.position.y + SmallShadowIconPositionGap)) != RectTransformHitUp.GetComponent<Collider2D>())
                                Destroy(shadowItemIcon);
                        }
                        else
                        {
                            shadowItemIcon = Instantiate(ShadowIconSmallPrefab) as GameObject;
                            shadowItemIcon.transform.parent = InventoryItemList;
                            shadowItemIcon.transform.position = new Vector3(RectTransformHitUp.transform.position.x, RectTransformHitUp.transform.position.y - SmallShadowIconPositionGap, RectTransformHitUp.transform.position.z);
                            shadowItemIcon.transform.localScale = Vector3.one;
                        }
                    }
                    else if (RectTransformHitDown != null && ItemIcons.Contains(RectTransformHitDown))
                    {
                        if (shadowItemIcon != null)
                        {
                            if (Physics2D.OverlapPoint(new Vector2(shadowItemIcon.transform.position.x, shadowItemIcon.transform.position.y - SmallShadowIconPositionGap)) != RectTransformHitDown.GetComponent<Collider2D>())
                                Destroy(shadowItemIcon);
                        }
                        else
                        {
                            shadowItemIcon = Instantiate(ShadowIconSmallPrefab) as GameObject;
                            shadowItemIcon.transform.parent = InventoryItemList;
                            shadowItemIcon.transform.position = new Vector3(RectTransformHitDown.transform.position.x, RectTransformHitDown.transform.position.y + SmallShadowIconPositionGap, RectTransformHitDown.transform.position.z);
                            shadowItemIcon.transform.localScale = Vector3.one;
                        }
                    }
                    else
                    {
                        if (shadowItemIcon != null)
                            Destroy(shadowItemIcon);
                    }
                }
                else if (RectTransformHit == ItemIcons[Inventory.Instance.draggedItem.indexInContents])
                    Destroy(shadowItemIcon);
            }
            else
            {
                if (shadowItemIcon != null)
                    Destroy(shadowItemIcon);
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
                Inventory.Instance.draggedItem.ClearDraggedItem();
        }
    }

    /// <summary>
    /// Checks if the mouse is over an UI object
    /// </summary>
    /// <returns> true if yes </returns>
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    /// It's better to do this separately from the Update
    /// </summary>
    /// <returns>null</returns>
    IEnumerator UpdateCarryWeightBar()
    {
        for (; ; )
        {
            if (displayInventory)
                yield return null;

            CarryWeightBar.GetComponent<Image>().fillAmount = Player.Instance.pStats.CarryWeight / Player.Instance.pStats.MaxCarryWeight;
            yield return null;
        }
    }

    /// <summary>
    /// To make sure the UpdateCarryWeightBar() coroutine is always running
    /// </summary>
    void OnEnable()
    {
        StartCoroutine(UpdateCarryWeightBar());
    }
}