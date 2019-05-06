using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplicitTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Fahrenheit fahr = new Fahrenheit(100.0f);
        Debug.Log($"{fahr.Degrees} Fahrenheit");
        Celsius c = (Celsius)fahr;

        Debug.Log($" = {c.Degrees} Celsius");
        Fahrenheit fahr2 = (Fahrenheit)c;
        Debug.Log($" = {fahr2} Fahrenheit");
        fahr2.Degrees += fahr2++;
        Debug.Log(fahr2++.list.Count);
    }
}

class Celsius
{
    public Celsius(float temp)
    {
        Degrees = temp;
    }

    public float Degrees { get; }

    public static explicit operator Fahrenheit(Celsius c)
    {
        return new Fahrenheit((9.0f / 5.0f) * c.Degrees + 32);
    }
}

class Fahrenheit
{
    public Fahrenheit(float temp)
    {
        Degrees = temp;
    }

    public float Degrees { get; set; }
    public List<int> list = new List<int>();

    public static explicit operator Celsius(Fahrenheit fahr)
    {
        return new Celsius((5.0f / 9.0f) * (fahr.Degrees - 32));
    }

    public static implicit operator float(Fahrenheit fahr)
    {
        return fahr.Degrees;
    }

    public static implicit operator int(Fahrenheit fahr)
    {
        return (int)fahr.Degrees;
    }

    public static float operator +(Fahrenheit fahr, float f) {
        return fahr.Degrees + f;
    }

    public static int operator *(Fahrenheit fahr, int a)
    {
        return fahr;
    }

    public static implicit operator string(Fahrenheit fahr)
    {
        return fahr.list.ToString();
    }

    public static implicit operator List<int>(Fahrenheit fahr)
    {
        fahr.list.Add(fahr);
        return fahr.list;
    }

    public static Fahrenheit operator ++(Fahrenheit fahr)
    {
        fahr.list.Add(fahr);
        return fahr;
    }
}

