using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionView : MonoBehaviour
{
    [SerializeField] private SelectionSlotView selectionSlotViewPrefab;
    [SerializeField] private Vector2 slotViewOffset;
    [SerializeField] private float slotViewInterval;

    public void Initialize(Selection[] selections, Action<Selection> onSelected = null)
    {
        for(int index = 0; index < selections.Length; index++)
        {
            var selection = selections[index];

            var selectionSlotView = Instantiate(selectionSlotViewPrefab, transform);
            
            selectionSlotView.GetComponent<RectTransform>().anchoredPosition = slotViewOffset + Vector2.down * slotViewInterval * index;
            selectionSlotView.Initialize(selection);

            selectionSlotView.OnSelected += onSelected;
        }
    }
}
