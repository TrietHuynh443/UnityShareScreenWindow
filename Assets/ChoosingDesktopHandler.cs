using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosingDesktopHandler : MonoBehaviour, IPointerClickHandler
{
    //click controls still selecting this
    [SerializeField] private OnScreenSelectedSO _onScreenSelectedSO;

    private Button _button;
    private Image _chosenBackGround;

    public void OnPointerClick(PointerEventData eventData)
    {
        _onScreenSelectedSO.RaiseEvent(gameObject);
    }


    // Start is called before the first frame update
    private void Start()
    {
        _button = GetComponent<Button>();
        _chosenBackGround = GetComponent<Image>();
        _onScreenSelectedSO.AddListener(CheckClicking);
    }

    private void OnDestroy()
    {
        _onScreenSelectedSO.RemoveListener(CheckClicking);
    }

    private void CheckClicking(GameObject @object)
    {
        _chosenBackGround.enabled = (@object == gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
