using System;
using TMPro;
using UnityEngine;

public class SelectionSlotView : MonoBehaviour
{
    public event Action<Selection> OnSelected;

    [SerializeField] private TMP_Text selectionName;

    private Selection selection;

    public void Initialize(Selection selection)
    {
        this.selection = selection;
        selectionName.text = selection.Name;
    }

    public void SelectSlot()
    {
        OnSelected?.Invoke(selection);
    }
}
