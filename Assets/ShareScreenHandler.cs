using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShareScreen : MonoBehaviour
{
    [SerializeField] OnScreenSelectedSO _onScreenSelectedSO;
    [SerializeField] OnShareScreenEventSO _onShareScreenEventSO;
    [SerializeField][Range(1, 120)] private int _smooth = 30;

    // SO to notify event
    private string _screenName;
    private bool _isSharing;
    private RawImage _screen;
    private float _lastUpdate;

    private void Start()
    {
        _screen = GetComponentInChildren<RawImage>();
    }
    private void OnEnable()
    {
        if (_onScreenSelectedSO == null)
        {
            _screenName = ScreenShareHelper.EntireScreen;
        }
        else if (_onScreenSelectedSO.SelectedGameObject == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            _screenName = _onScreenSelectedSO.SelectedGameObject.name;
        }
        StartShareScreen();
    }

    private void StartShareScreen()
    {
        IntPtr hwnd = ScreenShareHelper.GetScreen(_screenName);
        if (hwnd == IntPtr.Zero && _screenName != ScreenShareHelper.EntireScreen)
        {
            gameObject.SetActive(false);
            return;
        }

        _onShareScreenEventSO?.RaiseOnStartShareEvent();
        _isBlocking = false;
        _isSharing = true;
        _lastUpdate = Time.time;
        ScreenShareHelper.PopupWindow(hwnd);
    }


    private void StopShareScreen()
    {
        if (_isSharing)
        {
            _isSharing = false;
            _onShareScreenEventSO?.RaiseOnStopShareEvent();

        }
    }
    private bool _isBlocking;

    // Update is called once per frame
    private void Update()
    {
        if (!_isBlocking && _isSharing && Time.time - _lastUpdate >= 1 / _smooth)
        {
            _isBlocking = true;
            //_screen.texture = ScreenShareHelper.GetTexture(_screenName);
            Bitmap bitmap = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            Debug.Log(_lastUpdate);

            Task.Run(() =>
            {
                cts.Token.ThrowIfCancellationRequested();
                bitmap = ScreenShareHelper.CaptureWindow(_screenName);
                return bitmap;
            }, cts.Token)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Task isFaulted");

                }
                if (task.IsCanceled)
                {
                    Debug.Log("Task cancled");
                }
                if (task.IsCompleted)
                {
                    Texture2D oldtexture = _screen.texture as Texture2D;
                    _screen.texture = ScreenShareHelper.GetTextureFromBitmap(bitmap);
                    Destroy(oldtexture);
                    _onShareScreenEventSO?.RaiseOnNewFrameArriveEvent(_screen.texture as Texture2D);
                    _lastUpdate = Time.time;
                    _isBlocking = false;
                }
                cts.Dispose();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}
