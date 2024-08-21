using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OnShareScreenEventSO", menuName = "OnShareScreenEventSO")]
public class OnShareScreenEventSO : ScriptableObject
{
    private Action<Texture2D> _onNewFrameArriveAction;
    private Action _onStartShareAction;
    private Action _onStopShareAction;

    public void RaiseOnNewFrameArriveEvent(Texture2D texture)
    {
        _onNewFrameArriveAction?.Invoke(texture);
    }

    public void RaiseOnStartShareEvent()
    {
        _onStartShareAction?.Invoke();
    }

    public void RaiseOnStopShareEvent()
    {
        _onStopShareAction?.Invoke();
    }

    public void AddOnNewFrameArriveListener(Action<Texture2D> action)
    {
        _onNewFrameArriveAction += action;
    }


    public void RemoveOnNewFrameArriveListener(Action<Texture2D> action)
    {
        _onNewFrameArriveAction -= action;
    }


    public void AddOnStartShareListener(Action action)
    {
        _onStartShareAction += action;
    }


    public void RemoveOnStartShareListener(Action action)
    {
        _onStartShareAction -= action;
    }

    public void AddOnStopShareListener(Action action)
    {
        _onStopShareAction += action;
    }


    public void RemoveOnStopShareListener(Action action)
    {
        _onStopShareAction -= action;
    }
}
