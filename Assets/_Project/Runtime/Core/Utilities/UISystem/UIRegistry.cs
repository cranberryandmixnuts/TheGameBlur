using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UIRegistry", menuName = "Scriptable Objects/UIRegistry")]
public class UIRegistry : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public UIView Prefab;

        // expand after
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<Type, Entry> entryDictionary = new();

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        if (entryDictionary.Count > 0)
            return;

        foreach (var entry in entries)
        {
            entryDictionary.Add(entry.Prefab.GetType(), entry);
        }
    }

    public Entry GetEntry<T>() where T : UIView
    {
        if (entries.Length != entryDictionary.Count)
            InitializeDictionary();

        entryDictionary.TryGetValue(typeof(T), out var entry);

        return entry;
    }
}