//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using Composition.WindowsRuntimeHelpers;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;

namespace CaptureSampleCore
{
    public class BasicCapture : IDisposable
    {
        public Bitmap Bitmap => bitmap;

        private GraphicsCaptureItem item;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private SizeInt32 lastSize;

        private IDirect3DDevice device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.DXGI.SwapChain1 swapChain;
        public Bitmap bitmap = null!;

        public BasicCapture(IDirect3DDevice d, GraphicsCaptureItem i)
        {
            item = i;
            device = d;
            d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);

            var dxgiFactory = new SharpDX.DXGI.Factory2();
            var description = new SharpDX.DXGI.SwapChainDescription1()
            {
                Width = item.Size.Width,
                Height = item.Size.Height,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SharpDX.DXGI.SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = SharpDX.DXGI.AlphaMode.Premultiplied,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };
            swapChain = new SharpDX.DXGI.SwapChain1(dxgiFactory, d3dDevice, ref description);

            framePool = Direct3D11CaptureFramePool.Create(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                i.Size);
            session = framePool.CreateCaptureSession(i);
            lastSize = i.Size;

            framePool.FrameArrived += OnFrameArrived;
        }

        public void Dispose()
        {
            session?.Dispose();
            framePool?.Dispose();
            swapChain?.Dispose();
            d3dDevice?.Dispose();
        }

        public void StartCapture()
        {
            session.StartCapture();
        }

        public ICompositionSurface CreateSurface(Compositor compositor)
        {
            return compositor.CreateCompositionSurfaceForSwapChain(swapChain);
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            var newSize = false;

            using (var frame = sender.TryGetNextFrame())
            {
                if (frame.ContentSize.Width != lastSize.Width ||
                    frame.ContentSize.Height != lastSize.Height)
                {
                    // The thing we have been capturing has changed size.
                    // We need to resize the swap chain first, then blit the pixels.
                    // After we do that, retire the frame and then recreate the frame pool.
                    newSize = true;
                    lastSize = frame.ContentSize;
                    swapChain.ResizeBuffers(
                        2, 
                        lastSize.Width, 
                        lastSize.Height, 
                        SharpDX.DXGI.Format.B8G8R8A8_UNorm, 
                        SharpDX.DXGI.SwapChainFlags.None);
                }

                using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
                using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                {
                    d3dDevice.ImmediateContext.CopyResource(bitmap, backBuffer);

                    var sysBitmap = ConvertTexture2DToBitmap(bitmap);
                    this.bitmap?.Dispose();
                    this.bitmap = sysBitmap;
                }

            } // Retire the frame.

            swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);

            if (newSize)
            {
                framePool.Recreate(
                    device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    lastSize);
            }
        }


        private Bitmap ConvertTexture2DToBitmap(Texture2D texture)
        {
            var description = texture.Description;
            if (description.Format != SharpDX.DXGI.Format.B8G8R8A8_UNorm)
            {
                throw new ArgumentException("Texture format must be B8G8R8A8_UNorm", nameof(texture));
            }

            var stagingDescription = new Texture2DDescription
            {
                Width = description.Width,
                Height = description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };

            var stagingTexture = new SharpDX.Direct3D11.Texture2D(texture.Device, stagingDescription);
            texture.Device.ImmediateContext.CopyResource(texture, stagingTexture);

            DataBox dataBox = texture.Device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

            var bitmap = new System.Drawing.Bitmap(description.Width, description.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, description.Width, description.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

            IntPtr srcPtr = dataBox.DataPointer;
            IntPtr destPtr = bitmapData.Scan0;
            int bytesWidth = description.Width * 4;
            int srcStride = dataBox.RowPitch;
            int destStride = bitmapData.Stride;

            for (int row = 0; row < description.Height; row++)
            {
                Utilities.CopyMemory(destPtr, srcPtr, bytesWidth);
                srcPtr = IntPtr.Add(srcPtr, srcStride);
                destPtr = IntPtr.Add(destPtr, destStride);
            }

            bitmap.UnlockBits(bitmapData);
            texture.Device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
            stagingTexture.Dispose();

            return bitmap;
        }
    }
}
