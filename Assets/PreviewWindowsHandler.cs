using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Universal.Common.Extensions;
using ATOM = System.UInt16;
using BITMAP = System.IntPtr;
using DWORD = System.UInt32;
using HDC = System.IntPtr;
using UINT = System.UInt32;
using WORD = System.UInt16;
public class PreviewWindowsHandler : MonoBehaviour
{


    private Dictionary<string, IntPtr> windows = new();

    private Dictionary<string, GameObject> _desktops = new();
    private bool isInit = false;
    [SerializeField] private float _refreshTime = 3f;
    [SerializeField] private int _previewFrame = 30;
    [SerializeField] private GameObject _screenPreviewView;
    [SerializeField] private GameObject _deskTopHolderPrefabs;
    private float _lastRefreshTime;
    private float _lastUpdateDesktopTime;
    private bool _isBlocking;

    public void ChooseSystem()
    {
        windows.Clear();
        windows = ScreenShareHelper.GetAllTopWindows();
        foreach (var win in windows.Keys)
        {
            Bitmap bitmap = null;
            Texture2D texture = null;

            bitmap = ScreenShareHelper.CaptureWindow(win);

            texture = ScreenShareHelper.GetTextureFromBitmap(bitmap);


            InitItem(win, texture);

        }
        _lastRefreshTime = Time.time;

        isInit = true;
        _isBlocking = false;

    }

    private void InitItem(string title, Texture2D texture)
    {
        if (_desktops.ContainsKey(title)) return;

        GameObject newDesktop = Instantiate(_deskTopHolderPrefabs, _screenPreviewView.transform);
        newDesktop.name = title;
        newDesktop.GetComponentInChildren<RawImage>().texture = texture;
        newDesktop.GetComponentInChildren<TextMeshProUGUI>().text = title;
        _desktops.TryAdd(title, newDesktop);
    }

    private List<Task> _tasks = new List<Task>();


    // Update is called once per frame
    void Update()
    {
        if (!isInit || _isBlocking) return;

        IntervalUpdateApp();
        if (Time.time - _lastUpdateDesktopTime >= 1 / _previewFrame)
        {
            _tasks.Clear();
            _isBlocking = true;
            foreach (var win in windows.Keys)
            {
                if (!ScreenShareHelper.IsIconicWindow(windows[win]))
                {
                    Bitmap bitmap = null;
                    Texture2D texture = _desktops[win].GetComponentInChildren<RawImage>().texture as Texture2D;
                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken token = cts.Token;
                    Task t = Task.Run(() =>
                    {
                        // Handle task cancellation
                        token.ThrowIfCancellationRequested();
                        bitmap = ScreenShareHelper.CaptureWindow(win);
                        return bitmap;
                    }, token).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"Error capturing window {win}: {task.Exception}");
                        }
                        else if (task.IsCanceled)
                        {
                            Debug.LogWarning($"Task for window {win} was canceled.");
                        }
                        else if (task.IsCompleted)
                        {
                            // Safe to update the texture on the main thread
                            Texture2D newTexture = ScreenShareHelper.GetTextureFromBitmap(task.Result);
                            Texture texture1 = _desktops[win].GetComponentInChildren<RawImage>().texture;
                            _desktops[win].GetComponentInChildren<RawImage>().texture = newTexture;
                            Destroy(texture1);
                        }

                        cts.Dispose();

                    }, TaskScheduler.FromCurrentSynchronizationContext());

                    _tasks.Add(t);
                    _desktops[win].SetActive(true);
                }
                else
                {
                    _desktops[win].SetActive(false);
                }
            }
            foreach (var title in _desktops.Keys)
            {
                if (!windows.ContainsKey(title))
                {
                    Texture2D oldTexture = _desktops[title].GetComponentInChildren<RawImage>().texture as Texture2D;
                    Destroy(oldTexture);
                    _desktops[title].SetActive(false);
                }
            }
            // Wait for all tasks to complete
            Task.WhenAll(_tasks).ContinueWith(t =>
            {
                _lastUpdateDesktopTime = Time.time;
                _isBlocking = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private void IntervalUpdateApp()
    {
        if (Time.time - _lastRefreshTime >= _refreshTime)
        {
            windows.Clear();
            windows = ScreenShareHelper.GetAllTopWindows();
            foreach (var win in windows.Keys)
            {
                Bitmap bitmap = null;
                Texture2D texture;
                bitmap = ScreenShareHelper.CaptureWindow(win);
                texture = ScreenShareHelper.GetTextureFromBitmap(bitmap);
                InitItem(win, texture);
            }
            _lastRefreshTime = Time.time;
        }
    }

}