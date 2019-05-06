using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Register event listener when enabled, and call event listener whenever is called.
//////////////////////////////////////
[CreateAssetMenu]
public class FileEvent : ScriptableObject
{
    private List<FileEventListener> Eventlistener = new List<FileEventListener>();
    private int index;

    public void Raise()
    {
        for (int i = Eventlistener.Count - 1; i >= 0; i--)
            Eventlistener[i].OnEventRaised(index);
    }

    public void RegisterListener(FileEventListener listener, int index)
    {
        if (!Eventlistener.Contains(listener))
        {
            Eventlistener.Add(listener);
            this.index = index;
        }
    }

    public void UnregisterListener(FileEventListener listener)
    {
        if (Eventlistener.Contains(listener))
        {
            Eventlistener.Remove(listener);
            index = -1;
        }
    }
}
