﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace uWindowCapture
{

public class UwcGetBufferExample : MonoBehaviour
{
    [SerializeField]
    UwcWindowTexture uwcTexture;

    Texture2D texture_;
    Color32[] pixels_;
    GCHandle handle_;
    IntPtr ptr_ = IntPtr.Zero;

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

    bool isValid
    {
        get 
        { 
            if (!uwcTexture) return false;

            var window = uwcTexture.window;
            return window != null && window.buffer != IntPtr.Zero;
        }
    }

    void OnDestroy()
    {
        if (ptr_ != IntPtr.Zero) {
            handle_.Free();
        }
    }

    void Update()
    {
        if (!isValid) return;

        var window = uwcTexture.window;
        if (texture_ == null || 
            window.bufferWidth != texture_.width || 
            window.bufferHeight != texture_.height) {
            UpdateTexture();
        }

        CopyTexture();
    }

    void UpdateTexture()
    {
        if (!isValid) return;

        var window = uwcTexture.window;
        var width = window.bufferWidth;
        var height = window.bufferHeight;

        texture_ = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture_.filterMode = FilterMode.Bilinear;
        pixels_ = texture_.GetPixels32();
        handle_ = GCHandle.Alloc(pixels_, GCHandleType.Pinned);
        ptr_ = handle_.AddrOfPinnedObject();

        GetComponent<Renderer>().material.mainTexture = texture_;
    }

    void CopyTexture()
    {
        if (!isValid) return;

        var window = uwcTexture.window;
        var width = window.bufferWidth;
        var height = window.bufferHeight;
        var buffer = window.buffer;

        memcpy(ptr_, buffer, width * height * sizeof(Byte) * 4);

        texture_.SetPixels32(pixels_);
        texture_.Apply();
    }
}

}