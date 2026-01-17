using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Scriptable Objects/DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private List<Dialog> _dialogs;

    public string Name => _name;
    public List<Dialog> Dialogs => _dialogs;
}
