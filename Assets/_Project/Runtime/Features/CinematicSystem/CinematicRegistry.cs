using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CinematicRegistry", menuName = "Scriptable Objects/CinematicRegistry")]
public class CinematicRegistry : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public Cinematic Cinematic;

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
            entryDictionary.Add(entry.Cinematic.GetType(), entry);
        }
    }

    public Entry GetEntry<T>() where T : Cinematic
    {
        if (entries.Length != entryDictionary.Count)
            InitializeDictionary();

        entryDictionary.TryGetValue(typeof(T), out var entry);

        return entry;
    }
}