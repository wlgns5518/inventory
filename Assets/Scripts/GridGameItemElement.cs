using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class GridGameItemElement : VisualElement
{
    public bool IsSelected { get;private set; }
    public RectInt rect;
    public GameItem GameItem { get; private set; }
    private Image image;
    private Color color;
    private GridElement parent;
    public GridGameItemElement(RectInt rect, GameItem gameItem, GridElement parent)
    {
        AddToClassList("gameitem-container");
        this.rect = rect;
        this.GameItem = gameItem;
        this.parent = parent;

        image = new()
        {
            image = gameItem.data.icon.texture,
            pickingMode = PickingMode.Ignore
        };
        image.style.flexGrow = 1;
        Add(image);

        style.top = rect.y * parent.CellSize;
        style.left = rect.x * parent.CellSize;
        style.width = gameItem.data.inventorySize.x * parent.CellSize;
        style.height = gameItem.data.inventorySize.y * parent.CellSize;
        pickingMode = PickingMode.Ignore;
        color = style.backgroundColor.value;
    }
    public void Select()
    {
        IsSelected = true;
        style.display = DisplayStyle.None;
        RegisterCallback<MouseMoveEvent>(OnMouseMove);
        BringToFront();
    }

    public void Deselect()
    {
        IsSelected = false;
        style.display = DisplayStyle.Flex;
        UnregisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        transform.position = CalculateMousePosition(evt.mousePosition);
        Select();
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!IsSelected)
        {
            return;
        }
        transform.position = CalculateMousePosition(evt.mousePosition);
    }

    public void SetPosition(Vector2Int position)
    {
        rect.x = position.x;
        rect.y = position.y;
        rect.width = GameItem.data.inventorySize.x;
        rect.height = GameItem.data.inventorySize.y;
        style.top = rect.y * parent.CellSize;
        style.left = rect.x * parent.CellSize;
        style.width = GameItem.data.inventorySize.x * parent.CellSize;
        style.height = GameItem.data.inventorySize.y * parent.CellSize;
    }


    private Vector2 CalculateMousePosition(Vector2 mousePosition)
    {
        return new Vector2(
            mousePosition.x - (layout.width / 2) - parent.worldBound.position.x, 
            mousePosition.y - (layout.height / 2) - parent.worldBound.position.y
        );
    }
}
