using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Scriptable Objects/DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField] private List<Dialog> _dialogs;

    public List<Dialog> Dialogs => _dialogs;
}
