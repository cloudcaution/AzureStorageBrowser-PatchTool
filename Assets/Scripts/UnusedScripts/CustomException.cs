using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomException : Exception
{
    public CustomException()
    {
        Debug.Log("dwadwa");
    }

    public CustomException(string message)
        : base(message)
    {
        Debug.Log(base.Message);
        Debug.Log(message);
    }

    public CustomException(string message, Exception inner)
        : base(message, inner)
    {
        Debug.Log(message + "inner");
    }
}
