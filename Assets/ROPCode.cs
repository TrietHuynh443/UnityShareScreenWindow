using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ROPCodes
{
    public const uint SRCCOPY = 0x00CC0020;  // Copies the source directly to the destination
    public const uint SRCPAINT = 0x00EE0086; // Combines the colors of the source and the destination using the Boolean OR operation
    public const uint SRCAND = 0x008800C6;   // Combines the colors of the source and the destination using the Boolean AND operation
    public const uint SRCINVERT = 0x00660046; // Combines the colors of the source and the destination using the Boolean XOR operation
    public const uint SRCERASE = 0x00440328; // Combines the inverted colors of the source and the destination using the Boolean AND operation
    public const uint NOTSRCCOPY = 0x00330008; // Inverts the source and copies to the destination
    public const uint MERGECOPY = 0x00C000CA; // Combines the colors of the source and the destination using the Boolean AND operation with a pattern mask
    public const uint MERGEPAINT = 0x00BB0226; // Combines the inverted colors of the destination and the source using the Boolean OR operation
    public const uint PATCOPY = 0x00F00021;   // Copies the brush currently selected in the destination device context to the destination bitmap
    public const uint PATPAINT = 0x00FB0A09;  // Combines the colors of the brush and the destination using the Boolean OR operation
    public const uint PATINVERT = 0x005A0049; // Combines the colors of the brush and the destination using the Boolean XOR operation
    public const uint DSTINVERT = 0x00550009; // Inverts the destination bitmap
    public const uint BLACKNESS = 0x00000042; // Fills the destination bitmap with black
    public const uint WHITENESS = 0x00FF0062; // Fills the destination bitmap with white
    public const uint CAPTUREBLT = 0x40000000; // Includes any windows that are layered on top of your window
}