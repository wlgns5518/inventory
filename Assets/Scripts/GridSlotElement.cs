using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridSlotElement : VisualElement
{
    public bool IsOccupied { get; set; }
    public int CellSize { get; set; }
    public Vector2Int Position { get; set; }
    public event Action<GridSlotElement> OnPointerDown;
    public event Action<GridSlotElement> OnPointerOver;

    public GridSlotElement()
    {
        AddToClassList("grid-slot");
    }
    public GridSlotElement(int cellSize, Vector2Int position)
    {
        Init(cellSize, position);
    }
    private void Init(int cellSize, Vector2Int position)
    {
        AddToClassList("grid-slot");
        CellSize = cellSize;
        style.width = cellSize;
        style.height = cellSize;
        Position = position;
        IsOccupied = false;
        RegisterCallback<PointerDownEvent>((evt) => OnPointerDown?.Invoke(this));
        RegisterCallback<PointerOverEvent>((evt)=> OnPointerOver?.Invoke(this));
    }
    public new class UxmlFactory : UxmlFactory<GridSlotElement, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        private UxmlIntAttributeDescription cellSize = new() { name = "cell-size", defaultValue = 72 };
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var slot = ve as GridSlotElement;
            slot.Init(cellSize.GetValueFromBag(bag, cc), slot.Position);
        }
    }
}
