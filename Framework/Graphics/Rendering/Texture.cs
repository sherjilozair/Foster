﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Foster.Framework
{
    public abstract class Texture : IDisposable
    {

        /// <summary>
        /// Gets the Width of the Texture
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Gets the Height of the Texture
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// The Texture Data Format
        /// </summary>
        public readonly TextureFormat Format;

        /// <summary>
        /// The Size of the Texture, in bytes
        /// </summary>
        public int Size => Width * Height * (Format switch
        {
            TextureFormat.Color => 4,
            TextureFormat.Red => 1,
            TextureFormat.RG => 2,
            TextureFormat.RGB => 3,
            TextureFormat.DepthStencil => 4,
            _ => throw new Exception("Invalid Texture Format")
        });

        /// <summary>
        /// The Texture Filter to be used while drawing
        /// </summary>
        public TextureFilter Filter
        {
            get => filter;
            set => SetFilter(filter = value);
        }

        /// <summary>
        /// The Horizontal Wrapping mode
        /// </summary>
        public TextureWrap WrapX
        {
            get => wrapX;
            set => SetWrap(wrapX = value, wrapY);
        }

        /// <summary>
        /// The Vertical Wrapping mode
        /// </summary>
        public TextureWrap WrapY
        {
            get => wrapY;
            set => SetWrap(wrapX, wrapY = value);
        }


        /// <summary>
        /// If the Texture should be flipped vertically when drawing
        /// For example, OpenGL Render Targets usually require this
        /// </summary>
        public abstract bool FlipVertically { get; }

        private TextureFilter filter = TextureFilter.Linear;
        private TextureWrap wrapX = TextureWrap.Clamp;
        private TextureWrap wrapY = TextureWrap.Clamp;

        public static Texture Create(int width, int height, TextureFormat format = TextureFormat.Color)
        {
            return App.Graphics.CreateTexture(width, height, format);
        }

        public static Texture Create(Bitmap bitmap)
        {
            var texture = App.Graphics.CreateTexture(bitmap.Width, bitmap.Height, TextureFormat.Color);
            texture.SetData<Color>(bitmap.Pixels);
            return texture;
        }

        protected Texture(int width, int height, TextureFormat format)
        {
            if (format == TextureFormat.None)
                throw new Exception("Invalid Texture Format");

            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        /// Creates a Bitmap with the Texture Color data
        /// </summary>
        public Bitmap AsBitmap()
        {
            var bitmap = new Bitmap(Width, Height);
            GetData<Color>(new Memory<Color>(bitmap.Pixels));
            return bitmap;
        }

        /// <summary>
        /// Sets the Texture Color data from the given buffer
        /// </summary>
        public void SetColor(ReadOnlyMemory<Color> buffer) => SetData<Color>(buffer);

        /// <summary>
        /// Writes the Texture Color data to the given buffer
        /// </summary>
        public void GetColor(Memory<Color> buffer) => GetData<Color>(buffer);

        /// <summary>
        /// Sets the Texture data from the given buffer
        /// </summary>
        public void SetData<T>(ReadOnlyMemory<T> buffer)
        {
            if (Marshal.SizeOf<T>() * buffer.Length < Size)
                throw new Exception("Buffer is smaller than the Size of the Texture");

            SetDataInternal(buffer);
        }

        /// <summary>
        /// Writes the Texture data to the given buffer
        /// </summary>
        public void GetData<T>(Memory<T> buffer)
        {
            if (Marshal.SizeOf<T>() * buffer.Length < Size)
                throw new Exception("Buffer is smaller than the Size of the Texture");

            GetDataInternal(buffer);
        }

        protected abstract void SetFilter(TextureFilter filter);
        protected abstract void SetWrap(TextureWrap x, TextureWrap y);
        protected abstract void SetDataInternal<T>(ReadOnlyMemory<T> buffer);
        protected abstract void GetDataInternal<T>(Memory<T> buffer);

        public abstract void Dispose();
    }
}
