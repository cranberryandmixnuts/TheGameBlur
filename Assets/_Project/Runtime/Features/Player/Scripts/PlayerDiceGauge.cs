using System;
using UnityEngine;

public sealed class PlayerDiceGauge : MonoBehaviour
{
    [SerializeField] private PlayerSettings settings;

    public int Current { get; private set; }
    public int Max => settings.diceGaugeMax;

    public event Action<int, int> OnChanged;

    private void Awake()
    {
        Current = 0;
    }

    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        int before = Current;
        Current += amount;

        if (Current > Max)
            Current = Max;

        if (before != Current)
            OnChanged?.Invoke(Current, Max);
    }
}
