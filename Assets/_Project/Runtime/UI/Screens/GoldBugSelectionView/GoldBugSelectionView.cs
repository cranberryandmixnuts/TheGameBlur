using System;
using UnityEngine;

public class GoldBugSelectionView : MonoBehaviour
{
    [SerializeField] private GoldBugSelectionSlotView selectionSlotViewPrefab;
    [SerializeField] private Vector2 slotViewOffset;
    [SerializeField] private float slotViewInterval;

    public void Initialize(Selection[] selections, Action<Selection> onSelected = null)
    {
        for (int index = 0; index < selections.Length; index++)
        {
            var selection = selections[index];

            var selectionSlotView = Instantiate(selectionSlotViewPrefab, transform);

            selectionSlotView.GetComponent<RectTransform>().anchoredPosition = slotViewOffset + Vector2.down * slotViewInterval * index;
            selectionSlotView.Initialize(selection);

            selectionSlotView.OnSelected += onSelected;
        }
    }
}
