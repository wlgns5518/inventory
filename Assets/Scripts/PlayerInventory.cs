using DevionGames.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    // Test Data
    public List<GameItemData> gameItems;

    // Inventory Logics
    public enum InventoryMode
    {
        Default,
        Picked,
    }
    private InventoryMode inventoryMode;
    private int height = 5;
    private int width = 12;
    private RectInt inventoryRect;

    // Inventory UI Elements
    private VisualElement root;
    private GridElement inventoryGrid;
    private Dictionary<Vector2Int, GridSlotElement> inventorySlots = new Dictionary<Vector2Int, GridSlotElement>();
    private Dictionary<GameItem, GridGameItemElement> inventoryGameItems = new Dictionary<GameItem, GridGameItemElement>();
    private VisualElement pointerFollowingObject;
    private GridGameItemElement pickedInventoryItem;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        inventoryRect = new RectInt(new Vector2Int(0, 0), new Vector2Int(width, height));
        inventoryGrid = root.Q<GridElement>("InventoryGrid");
        var inventorySlots = inventoryGrid.Query<GridSlotElement>().Build();
        foreach(GridSlotElement slot in inventorySlots)
        {
            this.inventorySlots.Add(slot.Position, slot);
            slot.OnPointerDown += Slot_OnPointerDown;
            slot.OnPointerOver += Slot_OnPointerOver;
        }

        // Test with test data
        for (int i = 0; i < gameItems.Count; i++)
        {
            if (GetEmptySpace(gameItems[i].inventorySize, out var rect))
            {
                GameItem item = new GameItem();
                item.id = (ulong)i;
                item.data = gameItems[i];
                CreateGameItemVisualElement(item, rect);
            }
            else
            {
                Debug.LogWarning("false");
            }
        }
    }

    private void Update()
    {
        if (pointerFollowingObject != null)
        {
            var mousePosition = Input.mousePosition;
            Vector2 mousePositionCorrected = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            mousePositionCorrected = RuntimePanelUtils.ScreenToPanel(root.panel, mousePositionCorrected);
            mousePositionCorrected.y -= pointerFollowingObject.style.height.value.value/2f;
            mousePositionCorrected.x -= pointerFollowingObject.style.width.value.value / 2f;

            pointerFollowingObject.transform.position = mousePositionCorrected;
        }
    }

    //
    // Rect Utilities
    //
    private bool IsInsideInventory(RectInt rect)
    {
        // Rect.Contains로도 가능하지만, rect가 포함되는 것이 아니라 맞닿을수도 있기 때문에 직접 비교한다.
        if (rect.xMax > width || rect.yMax > height) return false;
        if (rect.xMin < 0 || rect.yMin < 0) return false;
        return true;
    }
    private bool IsFitInInventory(RectInt rect)
    {
        if (!IsInsideInventory(rect))
        {
            return false;
        }
        foreach (var item in inventoryGameItems)
        {
            if (item.Value.rect.Overlaps(rect))
            {
                return false;
            }
        }
        return true;
    }
    
    private void ResetAllSlotsColor()
    {
        foreach (var slot in inventorySlots)
        {
            slot.Value.style.backgroundColor = new StyleColor(UnityEngine.Color.black);
        }
    }


    private void CreateGameItemVisualElement(GameItem gameItem, RectInt rect)
    {
        GridGameItemElement gameItemElement = new GridGameItemElement(rect, gameItem, inventoryGrid);
        inventoryGrid.Add(gameItemElement);
        inventoryGameItems.Add(gameItem, gameItemElement);
    }

    private VisualElement CreatePointerFollowingGameItem(GridGameItemElement gridGameItemElement)
    {
        VisualElement visualElement = new VisualElement();
        visualElement.style.flexGrow = 1;
        visualElement.style.position = Position.Absolute;
        int CellSize = inventoryGrid.CellSize;
        Image image = new()
        {
            image = gridGameItemElement.GameItem.data.icon.texture,
            pickingMode = PickingMode.Ignore
        };
        image.style.flexGrow = 1;

        visualElement.style.width = gridGameItemElement.GameItem.data.inventorySize.x * CellSize;
        visualElement.style.height = gridGameItemElement.GameItem.data.inventorySize.y * CellSize;
        visualElement.Add(image);
        visualElement.pickingMode = PickingMode.Ignore;
        root.Add(visualElement);
        return visualElement;
    }
    private bool GetEmptySpace(Vector2Int size, out RectInt emptySpace)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                RectInt newSpace = new RectInt(pos, size);

                if (!IsFitInInventory(newSpace)) 
                {
                    continue;
                }

                emptySpace = newSpace;
                return true;
            }
        }
        emptySpace = new RectInt();
        return false;
    }
    private GridGameItemElement GetGameItemAt(Vector2Int position)
    {
        foreach (var item in inventoryGameItems)
        {
            if (item.Value.rect.Contains(position))
            {
                return item.Value;
            }
        }
        return default;
    }

    private void Slot_OnPointerDown(GridSlotElement slot)
    {
        if (inventoryMode == InventoryMode.Default)
        {
            PickGameItem(slot.Position);
            inventoryMode = InventoryMode.Picked;
        }
        else if(inventoryMode == InventoryMode.Picked)
        {
            RectInt rect = pickedInventoryItem.rect;
            rect.x = slot.Position.x - rect.width / 2;
            rect.y = slot.Position.y - rect.height/ 2;
            int overlapCount = 0;
            bool overlaps = false;
            GameItem overlappedGameItem = null;
            foreach (var item in inventoryGameItems)
            {
                if (item.Value.rect.Overlaps(rect))
                {
                    overlaps = true;
                    overlappedGameItem = item.Key;
                    overlapCount++;
                }
            }
            if (overlapCount >= 2) return;
            if (overlaps)
            {
                inventoryGameItems.Add(pickedInventoryItem.GameItem, pickedInventoryItem);
                pickedInventoryItem.Deselect();
                pickedInventoryItem.SetPosition(rect.position);
                pickedInventoryItem = default;
                pointerFollowingObject.Clear();
                pointerFollowingObject = null;
                PickGameItem(overlappedGameItem);
            }
            else
            {
                inventoryGameItems.Add(pickedInventoryItem.GameItem, pickedInventoryItem);
                pickedInventoryItem.Deselect();
                pickedInventoryItem.SetPosition(rect.position);
                pickedInventoryItem = default;
                pointerFollowingObject.Clear();
                pointerFollowingObject = null;
                inventoryMode = InventoryMode.Default;
            }
        }
    }

    private void Slot_OnPointerOver(GridSlotElement slot)
    {
        ResetAllSlotsColor();
        if (pointerFollowingObject != null)
        {
            RectInt rect = pickedInventoryItem.rect;
            rect.x = slot.Position.x - rect.width/2;
            rect.y = slot.Position.y - rect.height/2;
            if (IsFitInInventory(rect))
            {
                foreach (var pos in rect.allPositionsWithin)
                {
                    if (!inventorySlots.TryGetValue(pos, out var inventorySlot))
                    {
                        continue;
                    }
                    inventorySlot.style.backgroundColor = new StyleColor(UnityEngine.Color.green);
                }
            }
            else
            {
                foreach (var pos in rect.allPositionsWithin)
                {
                    if (!inventorySlots.TryGetValue(pos, out var inventorySlot))
                    {
                        continue;
                    }
                    inventorySlot.style.backgroundColor = new StyleColor(UnityEngine.Color.red);
                }
            }
            
        }
    }

    private void PickGameItem(GameItem gameItem)
    {
        inventoryGameItems.TryGetValue(gameItem, out var item);
        pickedInventoryItem = item;
        if (pickedInventoryItem == null)
        {
            return;
        }
        pickedInventoryItem.Select();
        inventoryGameItems.Remove(pickedInventoryItem.GameItem);
        pointerFollowingObject = CreatePointerFollowingGameItem(pickedInventoryItem);
    }
    private void PickGameItem(Vector2Int position)
    {
        pickedInventoryItem = GetGameItemAt(position);
        if (pickedInventoryItem == null)
        {
            return; 
        }
        pickedInventoryItem.Select();
        inventoryGameItems.Remove(pickedInventoryItem.GameItem);
        pointerFollowingObject = CreatePointerFollowingGameItem(pickedInventoryItem);
    }
}
