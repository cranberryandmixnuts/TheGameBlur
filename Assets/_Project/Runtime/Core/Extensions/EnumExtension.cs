using System;
using System.Collections.Generic;
using UnityEngine;

public static class EnumExtension
{
    public static T GetRandomEnum<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    public static T GetRandomEnum<T>(T exceptionType) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));

        int count = 0;
        for (int index = 0; index < values.Length; index++)
            if (!EqualityComparer<T>.Default.Equals(values[index], exceptionType))
                count++;

        if (count == 0)
            throw new InvalidOperationException();

        int randomIndex = UnityEngine.Random.Range(0, count);

        for (int index = 0; index < values.Length; index++)
        {
            if (EqualityComparer<T>.Default.Equals(values[index], exceptionType))
                continue;

            if (randomIndex-- == 0)
                return values[index];
        }

        throw new InvalidOperationException();
    }
}
