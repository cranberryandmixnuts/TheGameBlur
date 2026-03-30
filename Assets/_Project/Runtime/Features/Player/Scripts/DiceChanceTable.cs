public static class DiceChanceTable
{
    public static float GetEnemyChance(int diceValue)
    {
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

    public static float GetPlayerChance(int diceValue)
    {
        return diceValue switch
        {
            8 => 0.05f,
            9 => 0.10f,
            10 => 0.20f,
            11 => 0.25f,
            12 => 0.50f,
            _ => 0f
        };
    }

    public static float GetPlayerSkillSize(int diceValue)
    {
        return diceValue switch
        {
            2 => 0.05f,
            3 => 0.2f,
            4 => 0.4f,
            5 => 0.6f,
            6 => 0.8f,
            7 => 1f,
            8 => 1.5f,
            9 => 2f,
            10 => 2.5f,
            11 => 3f,
            12 => 5f,
            _ => 1f
        };
    }
}
