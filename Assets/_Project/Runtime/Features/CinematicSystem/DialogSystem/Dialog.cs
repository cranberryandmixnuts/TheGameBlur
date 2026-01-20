using System;
using UnityEngine;

[Serializable]
public class Dialog
{
    [SerializeField] private string name;
    [SerializeField] private string line;

    [SerializeField] private bool isSelectable;

    [SerializeField] private Selection[] selections;

    public string Name => name;
    public string Line => line;

    public bool IsSelectable => isSelectable;

    public Selection[] Selections => selections;
}
