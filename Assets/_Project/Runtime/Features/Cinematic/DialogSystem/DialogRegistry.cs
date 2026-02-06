using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogRegistry", menuName = "Scriptable Objects/DialogRegistry")]
public class DialogRegistry : ScriptableObject
{
    [SerializeField] private List<DialogData> dialogDatas;

    public DialogData GetDialogData(string name)
    {
        var dialogData = dialogDatas.Find(dialogDatas => dialogDatas.name == name);
        
        if(dialogData == null)
            throw new KeyNotFoundException($"DialogData not found: {name}");

        return dialogData;
    }
}
