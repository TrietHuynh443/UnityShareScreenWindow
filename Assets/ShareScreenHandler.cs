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
        _screen = GetComponent<RawImage>();
    }
    private void OnEnable()
    {
        if (_onScreenSelectedSO.SelectedGameObject == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _screenName = _onScreenSelectedSO.SelectedGameObject.name;
            StartShareScreen();
        }
    }

    private void StartShareScreen()
    {
        IntPtr hwnd = ScreenShareHelper.GetScreen(_screenName);
        if (hwnd == IntPtr.Zero)
        {
            gameObject.SetActive(false);
            return;
        }

        _onShareScreenEventSO?.RaiseOnStartShareEvent();

        _isSharing = true;
        _lastUpdate = Time.time;
    }


    private void StopShareScreen()
    {
        if (_isSharing)
        {
            _isSharing = false;
            _onShareScreenEventSO?.RaiseOnStopShareEvent();

        }
    }
    Texture2D texture;
    private bool _isBlocking;

    // Update is called once per frame
    private void Update()
    {
        if (!_isBlocking && _isSharing && Time.time - _lastUpdate >= 1 / _smooth)
        {
            _isBlocking = true;
            //_screen.texture = ScreenShareHelper.GetTexture(_screenName);
            Bitmap bitmap = null;
            texture = _screen.texture as Texture2D;
            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                cts.Token.ThrowIfCancellationRequested();
                bitmap = ScreenShareHelper.CaptureWindow(_screenName);
                return bitmap;
            }, cts.Token)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Texture2D oldtexture = _screen.texture as Texture2D;
                    _screen.texture = ScreenShareHelper.GetTextureFromBitmap(bitmap);
                    Destroy(oldtexture);
                    _onShareScreenEventSO.RaiseOnNewFrameArriveEvent(_screen.texture as Texture2D);
                    _lastUpdate = Time.time;
                    _isBlocking = false;
                }
                cts.Dispose();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}
