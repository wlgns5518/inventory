using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridRowElement : VisualElement
{
    public GridRowElement(int row, int columns, int cellSize)
    {
        Init(row, columns, cellSize);
    }

    public GridRowElement()
    {
        AddToClassList("grid-row");
    }
    public int Row { get; private set; }
    public int Columns { get; set; }
    public int CellSize { get; set; }

    private void Init(int row, int columns, int cellSize)
    {
        AddToClassList("grid-row");
        CellSize = cellSize;
        Columns = columns;
        Row = row;
        Clear();
        for (int i = 0; i < Columns; i++)
        {
            Add(new GridSlotElement(cellSize, new Vector2Int(i, row)));
        }
    }

    public new class UxmlFactory : UxmlFactory<GridRowElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        private UxmlIntAttributeDescription _columns = new() { name = "columns", defaultValue = 10 };
        private UxmlIntAttributeDescription _cellSize = new() { name = "cell-size", defaultValue = 72 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var row = ve as GridRowElement;
            row.Init(row.Row, _columns.GetValueFromBag(bag, cc), _cellSize.GetValueFromBag(bag, cc));
        }
    }
}