using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: This is a File event listener that will trigger a specified event when some event triggers.
//////////////////////////////////////
public class FileEventListener : MonoBehaviour
{
    public FileEvent[] fileEvent;
    public UnityEvent[] response;

    private void OnEnable()
    {
        for (int i = 0; i < fileEvent.Length; i++)
            fileEvent[i].RegisterListener(this, i);
    }

    private void OnDisable()
    {
        for (int i = 0; i < fileEvent.Length; i++)
            fileEvent[i].UnregisterListener(this);
    }

    public void OnEventRaised(int index)
    {
        response[index].Invoke();
    }
}
