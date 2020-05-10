using System;
using Foster.Framework;
using Foster.OpenGL;
using Foster.SDL2;

namespace Tetris
{
    class Program
    {
        static void Main(string[] args)
        {
            // We're making a pixel art game
            Texture.DefaultTextureFilter = TextureFilter.Nearest;

            App.Modules.Register<SDL_System>();

            // Register our Graphics Module (OpenGL in this case)
            App.Modules.Register<GL_Graphics>();

            // Register our Custom Module, where we will run our own code
            App.Modules.Register<Game>();

            // Begin Application
            App.Start("Tetris", 1280, 720);
        }
    }
}
