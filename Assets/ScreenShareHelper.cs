using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using ATOM = System.UInt16;
using BITMAP = System.IntPtr;
using DWORD = System.UInt32;
using HDC = System.IntPtr;
using UINT = System.UInt32;
using WORD = System.UInt16;
public class ScreenShareHelper
{
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lParams);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWINFO
    {
        public DWORD cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public DWORD dwStyle;
        public DWORD dwExStyle;
        public DWORD dwWindowStatus;
        public UINT cxWindowBorders;
        public UINT cyWindowBorders;
        public ATOM atomWindowType;
        public WORD wCreatorVersion;
        public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO lpInfo);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern HDC GetDC(IntPtr hWnd);

    [DllImport("Gdi32.dll")]
    private static extern bool BitBlt(
        HDC hdcDest, // handle to destination DC
        int nXDest,     // x-coordinate of destination upper-left corner
        int nYDest,     // y-coordinate of destination upper-left corner
        int nWidth,     // width of the source rectangle
        int nHeight,    // height of the source rectangle
        HDC hdcSrc,  // handle to source DC
        int nXSrc,      // x-coordinate of source upper-left corner
        int nYSrc,      // y-coordinate of source upper-left corner
        DWORD dwRop // raster-operation code
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    private static extern HDC CreateCompatibleDC(HDC hdc);

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern BITMAP CreateCompatibleBitmap(HDC hdc, int width, int height); //return bitmap

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern IntPtr SelectObject(HDC hdc, IntPtr obj); //select obj to hdc device context

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr obj);
    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern bool DeleteDC(HDC obj);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(IntPtr hwnd, HDC hdc);


    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool IsIconic(IntPtr hwnd);

    [DllImport("user32.dll")]
    static extern bool PrintWindow(IntPtr hwnd, HDC hDC, UINT nFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetObject(IntPtr handle, int numBytes, out BitMap @object);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private static event EnumWindowsProc callback;


    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public static IntPtr EntireHwnd => IntPtr.Zero;

    private static Dictionary<string, IntPtr> windows = new();
    public static string EntireScreen = "EntireScreen";

    public static Dictionary<string, IntPtr> GetAllTopWindows()
    {
        callback += OnWindowFound;
        windows.Clear();
        EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
        callback -= OnWindowFound;
        return windows;
    }

    public static Bitmap CaptureWindow(string screenName)
    {
        IntPtr hwnd = IntPtr.Zero;
        int width = GetSystemMetrics(0); // SM_CXSCREEN
        int height = GetSystemMetrics(1); // SM_CYSCREEN


        if (screenName == EntireScreen)
        {
            hwnd = EntireHwnd;
        }
        else if (!windows.ContainsKey(screenName))
        {
            return null;
        }
        else
        {
            hwnd = windows[screenName];
            RECT windowRect = new RECT();
            GetWindowRect(hwnd, ref windowRect);

            width = windowRect.right - windowRect.left;
            height = windowRect.bottom - windowRect.top;
        }
        IntPtr hdcSrc = GetWindowDC(hwnd);
        IntPtr hdcDest = CreateCompatibleDC(hdcSrc);


        IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
        IntPtr hOld = SelectObject(hdcDest, hBitmap);

        // Use the SRCCOPY ROP code defined earlier
        // Using PrintWindow instead of BitBlt
        if (screenName == EntireScreen)
        {
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, ROPCodes.SRCCOPY);
        }
        else
        {
            PrintWindow(hwnd, hdcDest, 2);
        }

        Bitmap bmp = Bitmap.FromHbitmap(hBitmap);


        SelectObject(hdcDest, hOld);
        DeleteObject(hBitmap);
        DeleteDC(hdcDest);
        ReleaseDC(hwnd, hdcSrc);
        return bmp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitMap
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    private static bool OnWindowFound(IntPtr hWnd, IntPtr lParam)
    {
        if (IsWindowVisible(hWnd))
        {
            StringBuilder title = new StringBuilder(500);
            GetWindowText(hWnd, title, title.Capacity);
            if (title.Length > 0)
            {
                windows.TryAdd(title.ToString(), hWnd);
            }
        }
        return true;
    }

    public static bool IsIconicWindow(IntPtr hWnd)
    {
        return IsIconic(hWnd);
    }

    public static IntPtr GetScreen(string name)
    {
        if (name == EntireScreen)
        {
            return EntireHwnd;
        }
        if (windows.ContainsKey(name))
        {
            return windows[name];
        }
        return IntPtr.Zero;
    }

    public static Texture2D GetTextureFromBitmap(Bitmap bitmap)
    {
        Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.BGRA32, false);
        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        BitmapData bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        texture.LoadRawTextureData(bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
        texture.Apply();

        bitmap.UnlockBits(bitmapData);

        return texture;
    }



    public static void PopupWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) // handle full display
        {
            return;
        }
        SetForegroundWindow(hwnd);
    }

}


