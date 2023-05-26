using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridElement : VisualElement
{
    public GridElement()
    {
        AddToClassList("grid");
    }

    public int Rows { get; set; }
    public int Columns { get; set; }
    public int CellSize { get; set; }

    private void Init(int rows, int columns, int cellSize)
    {
        Rows = rows;
        Columns = columns;
        CellSize = cellSize;
        Clear();
        for (int r = 0; r < Rows; r++)
        {
            Add(new GridRowElement(r, columns, cellSize));
        }
    }

    public new class UxmlFactory : UxmlFactory<GridElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription _columns = new() { name = "columns", defaultValue = 10 };
        UxmlIntAttributeDescription _rows = new() { name = "rows", defaultValue = 4 };
        UxmlIntAttributeDescription _cellSize = new() { name = "cell-size", defaultValue = 72 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var grid = ve as GridElement;
            int AsInt(UxmlIntAttributeDescription e) => e.GetValueFromBag(bag, cc);
            grid.Init(AsInt(_rows), AsInt(_columns), AsInt(_cellSize));
        }
    }
}
