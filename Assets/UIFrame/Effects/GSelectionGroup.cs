using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSelectionGroup : MonoBehaviour
{
    public int _currentIndex;
    public System.Action<int> onSelectedChange;//params:(int newSelectedIndex)

    public void OnEnable()
    {
        OnValidate();
    }

    public void OnTransformChildrenChanged()
    {
        OnValidate();
    }

    //call from GSelectable
    public void OnChildSelected(GSelectable selected)
    {
        for (int i = 0; i < transform.childCount; i++) {
            GSelectable child = transform.GetChild(i).GetComponent<GSelectable>();
            if (child) {
                if (child == selected) {
                    currentIndex = i;
                } else {
                    child.isSelected = false;
                }
            }
        }
        if (onSelectedChange != null) {
            onSelectedChange(currentIndex);
        }
    }

    public int currentIndex {
        get { return _currentIndex; }
        set {
            _currentIndex = value;
            OnValidate();
        }
    }

    public void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++) {
            GSelectable child = transform.GetChild(i).GetComponent<GSelectable>();
            if (child) {
                child.isSelected = i == _currentIndex;
            }
        }
    }
}
