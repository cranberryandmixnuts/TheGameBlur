using UnityEngine;

public static class DiceChanceTable
{
    public static float GetChance01(int diceValue)
    {
        diceValue = Mathf.Clamp(diceValue, 2, 6);
        return diceValue switch
        {
            2 => 0.50f,
            3 => 0.25f,
            4 => 0.20f,
            5 => 0.10f,
            6 => 0.05f,
            _ => 0f
        };
    }
}
