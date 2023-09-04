using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class TimeActionUpdate
{
    public float timeCounter;
    public Action updateAction;
    public Action endAction;
    public bool isTrigered = false;
    public object obj;

    public TimeActionUpdate(Action updateAction = null, Action endAction = null)
    {
        this.updateAction = updateAction;
        this.endAction = endAction;
    }
    public TimeActionUpdate() { }

    public void Update(float dt)
    {
        if (isTrigered)
        {
            if (timeCounter > 0)
            {
                timeCounter -= dt;
                updateAction?.Invoke();
            }
            else
            {
                isTrigered = false;
                endAction?.Invoke();
            }
        }
    }

    public void Trigger(float invokeTime = 0.05f)
    {
        timeCounter = invokeTime;
        isTrigered = true;
    }  
}