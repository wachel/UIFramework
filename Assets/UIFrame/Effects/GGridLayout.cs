using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
public class GGridLayout : MonoBehaviour
{
    public GridLayoutGroup grid;
    public bool autoCellSize;

#if UNITY_EDITOR    
    public void Update()
    {
        if(autoCellSize) {
            Vector2 cellSize = grid.cellSize;
            if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
                cellSize.x = (transform as RectTransform).rect.width / grid.constraintCount;
            }
            else if(grid.constraint == GridLayoutGroup.Constraint.FixedRowCount) {
                cellSize.y = (transform as RectTransform).rect.height / grid.constraintCount;
            }
            grid.cellSize = cellSize;
        }
    }
#endif
}
