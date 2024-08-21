using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OnScreenSelectedSO", menuName = "SciptableObjects/OnScreenSelectedSO")]
public class OnScreenSelectedSO : ScriptableObject
{
    Action<GameObject> action;

    public GameObject SelectedGameObject { get => selectedGameObject; private set => selectedGameObject = value; }

    private GameObject selectedGameObject;
    public void RaiseEvent(GameObject gameObject)
    {
        action?.Invoke(gameObject);
        selectedGameObject = gameObject;
    }

    public void AddListener(Action<GameObject> action)
    {
        this.action += action;
    }

    public void RemoveListener(Action<GameObject> action)
    {
        this.action -= action;
    }

}
