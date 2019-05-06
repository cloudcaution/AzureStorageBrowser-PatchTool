using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatchLogException : Exception
{
    public PatchLogException() : base("Default error.")
    {

    }

    public PatchLogException(string message) : base(string.Format("Error Message: {0}", message))
    {
        Debug.Log(base.Message);
    }

    public PatchLogException(string message, Action<string> method = null) : base(string.Format("Error Message: {0}", message))
    {
        method?.Invoke(base.Message);
    }
}
