using System;
using UnityEngine;

[Serializable]
public class Selection
{
    [SerializeField] private string name;
    [SerializeField] private DialogData dialogData;

    public string Name => name;
    public DialogData DialogData => dialogData;
}
