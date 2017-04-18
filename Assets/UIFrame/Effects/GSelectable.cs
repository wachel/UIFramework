using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GSelectable : MonoBehaviour, IPointerClickHandler
{
    public RectTransform selectedNode;
    public RectTransform unselectedNode;
    public Text text;
    public Color selectedTextColor;
    public Color unselectedTextColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = true;
        GSelectionGroup group = transform.parent.GetComponent<GSelectionGroup>();
        if (group) {
            group.OnChildSelected(this);
        }
    }

    public bool _isSelected;
    public bool isSelected {
        get {
            return _isSelected;
        }
        set {
            _isSelected = value;
            OnValidate();
        }
    }

    public void OnValidate()
    {
        if (selectedNode) { selectedNode.gameObject.SetActive(_isSelected); }
        if (unselectedNode) { unselectedNode.gameObject.SetActive(!_isSelected); }
        if (text) { text.color = _isSelected ? selectedTextColor : unselectedTextColor; }
    }
}
