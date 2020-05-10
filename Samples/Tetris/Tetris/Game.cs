using System;
using System.Numerics;
using Foster.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Tetris
{
    enum Tetro
    {
        I, J, L, O, S, T, Z,
    }

    public class Game: Module
    {
        Batch2D Batcher = new Batch2D();
        FrameBuffer FrameBuffer = new FrameBuffer(320, 180);

        string backgroundAsepritePath = "../../../Assets/background.aseprite";
        string tetroAsepritePath = "../../../Assets/tetro.aseprite";

        Aseprite backgroundAseprite;
        Aseprite tetroAseprite;

        Sprite[,] settledSprites = new Sprite[10, 20];
        Vector2 Offset = new Vector2(120, 10);

        Sprite backgroundSprite;

        Random rng = new Random();

        protected override void Startup()
        {
            // Add a Callback to the Primary Window's Render loop
            // By Default a single Window is created at startup
            // Alternatively App.System.Windows has a list of all open windows
            App.Window.OnRender += Render;

            backgroundAseprite = new Aseprite(backgroundAsepritePath);
            tetroAseprite = new Aseprite(tetroAsepritePath);

            TextureBank.AddAseprite("background", backgroundAseprite);
            TextureBank.AddAseprite("tetro", tetroAseprite);

            TextureBank.PackAndFinalize();

            backgroundSprite = new Sprite("background", backgroundAseprite);

        }

        class Tetromino
        {
            
            public (int x, int y) position;
            public Sprite[,] sprites;

        }

        Tetromino Clone(Tetromino self)
        {
            Tetromino clone = new Tetromino();
            clone.position = self.position;
            int m = self.sprites.GetLength(0);
            int n = self.sprites.GetLength(1);
            clone.sprites = new Sprite[m, n];
            for (int x = 0; x < m; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    clone.sprites[x, y] = CreateTetroSprite(self.sprites[x, y].Color);
                }
            }
            return clone;
        }

        Tetromino tetromino;

        float StepTime = 1.0f;
        float StepTimer = 0f;
        float NormalRate = 1f;          // 1 step in 60 frames, 1 step in 1 second
        float SoftDropRate = 20.0f;     // 1 step in 3 frames, 20 steps in 1 second

        bool Settle = false;

        List<Tetro> Bag = new List<Tetro>();

        protected override void Update()
        {
            if (Settle)
            {
                Settle = false;

                for (int x = 0; x < tetromino.sprites.GetLength(0); x++)
                {
                    for (int y = 0; y < tetromino.sprites.GetLength(1); y++)
                    {
                        if (tetromino.sprites[x, y] != null)
                        {
                            (int x, int y) globalPos = (
                                tetromino.position.x - 1 + x,
                                tetromino.position.y - 1 + y);

                            
                            if (globalPos.y < 0)
                            {
                                // end game
                            }
                            else
                            {
                                settledSprites[globalPos.x, globalPos.y] = tetromino.sprites[x, y];
                            }
                            
                        }
                    }
                }

                tetromino = null;
            }

            if (tetromino == null)
            {
                tetromino = new Tetromino();
                Tetro type;

                if(Bag.Count == 0)
                {
                    foreach(Tetro t in (Tetro[])Enum.GetValues(typeof(Tetro)))
                    {
                        Bag.Add(t);
                    }
                    rng.Shuffle(Bag);
                }

                type = Bag[0];
                Bag.RemoveAt(0);

                switch (type)
                {
                    case Tetro.I:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[4, 4];
                            Color color = new Color(0, 255, 255, 255); // CYAN
                            tetromino.sprites[0, 1] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            tetromino.sprites[3, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.J:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(0, 255, 0, 255); // BLUE
                            tetromino.sprites[0, 0] = CreateTetroSprite(color);
                            tetromino.sprites[0, 1] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.L:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(255, 165, 0, 255); // ORANGE
                            tetromino.sprites[2, 0] = CreateTetroSprite(color);
                            tetromino.sprites[0, 1] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.O:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(255, 255, 0, 255); // YELLOW
                            tetromino.sprites[1, 0] = CreateTetroSprite(color);
                            tetromino.sprites[2, 0] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.S:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(0, 255, 0, 255); // GREEN
                            tetromino.sprites[1, 0] = CreateTetroSprite(color);
                            tetromino.sprites[2, 0] = CreateTetroSprite(color);
                            tetromino.sprites[0, 1] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.T:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(128, 0, 128, 255); // PURPLE
                            tetromino.sprites[1, 0] = CreateTetroSprite(color);
                            tetromino.sprites[0, 1] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.Z:
                        {
                            tetromino.position = (4, -1);
                            tetromino.sprites = new Sprite[3, 3];
                            Color color = new Color(255, 0, 0, 255); // RED
                            tetromino.sprites[0, 0] = CreateTetroSprite(color);
                            tetromino.sprites[1, 0] = CreateTetroSprite(color);
                            tetromino.sprites[1, 1] = CreateTetroSprite(color);
                            tetromino.sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                }

            }

            StepTimer += Time.Delta * (App.Input.Keyboard.Down(Keys.Down)? SoftDropRate : NormalRate);
            if (StepTimer > StepTime)
            {
                StepTimer -= StepTime;
                if (!CollideAt(tetromino, (0, 1)))
                    tetromino.position.y += 1;
                else
                {
                    Console.WriteLine("settle");
                    Settle = true;
                }
            }
            if (App.Input.Keyboard.Repeated(Keys.Left, 0.5f, 0.2f))
            {
                if (!CollideAt(tetromino, (-1, 0)))
                    tetromino.position.x--;
            }
            else if (App.Input.Keyboard.Repeated(Keys.Right, 0.5f, 0.2f))
            {
                if (!CollideAt(tetromino, (1, 0)))
                    tetromino.position.x++;
            }
            else if (App.Input.Keyboard.Pressed(Keys.Up))
            {
                var candidate = RotateTetrominoClock(tetromino);
                if (candidate != null) tetromino = candidate;
            }
            else if (App.Input.Keyboard.Pressed(Keys.Down))
            {
                // increase gravity
            }
            else if (App.Input.Keyboard.Pressed(Keys.Z))
            {
                var candidate = RotateTetrominoCounter(tetromino);
                if (candidate != null) tetromino = candidate;
            }
            else if (App.Input.Keyboard.Pressed(Keys.C, Keys.LeftShift, Keys.RightShift))
            {
                // hold piece
            }
            else if (App.Input.Keyboard.Pressed(Keys.Space))
            {
                // hard drop
            }


            UpdateTetromino();

            backgroundSprite.Play("idle");

            backgroundSprite.Update();

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    if (settledSprites[x, y] != null)
                    {
                        settledSprites[x, y].Update();
                    }
                }
            }

        }

        private bool CollideAt(Tetromino t, (int x, int y) offset)
        {
            for (int x = 0; x < t.sprites.GetLength(0); x++)
            {
                for (int y = 0; y < t.sprites.GetLength(1); y++)
                {
                    if (t.sprites[x, y] != null)
                    {
                        (int x, int y) globalPos = (
                            t.position.x - 1 + x + offset.x,
                            t.position.y - 1 + y + offset.y);

                        if (globalPos.x < 0 || globalPos.x >= 10 || globalPos.y >= 20) return true;
                        if (globalPos.y < 0) continue;
                        if (settledSprites[globalPos.x, globalPos.y] != null) return true;
                    }
                }
            }
            return false;
        }

        void UpdateTetromino()
        {
            (int x, int y) center = (1, 1);
            for (int x = 0; x < tetromino.sprites.GetLength(0); x++)
            {
                for (int y = 0; y < tetromino.sprites.GetLength(1); y++)
                {
                    if (tetromino.sprites[x, y] != null)
                    {
                        (int x, int y) cell = (tetromino.position.x - center.x + x, tetromino.position.y - center.y + y);
                        tetromino.sprites[x, y].Position = new Vector2(Offset.X + 8 * cell.x, Offset.Y + 8 * cell.y);
                        tetromino.sprites[x, y].Update();
                    }
                }
            }
        }

        Tetromino RotateTetrominoClock(Tetromino self)
        {
            Tetromino clone = new Tetromino();
            clone.position = self.position;
            int m = self.sprites.GetLength(0);
            int n = self.sprites.GetLength(1);
            clone.sprites = new Sprite[m, n];

            for (int x = 0; x < m; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    if (self.sprites[x, y] != null)
                    {
                        clone.sprites[m-1-y, x] = CreateTetroSprite(self.sprites[x, y].Color);
                    }
                }
            }

            return clone;
        }

        Tetromino RotateTetrominoCounter(Tetromino self)
        {
            Tetromino clone = new Tetromino();
            clone.position = self.position;
            int m = self.sprites.GetLength(0);
            int n = self.sprites.GetLength(1);
            clone.sprites = new Sprite[m, n];

            for (int x = 0; x < m; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    if (self.sprites[x, y] != null)
                    {
                        clone.sprites[y, n-1-x] = CreateTetroSprite(self.sprites[x, y].Color);
                    }
                }
            }

            return clone;
        }


        void RenderTetromino(Batch2D Batcher)
        {
            for (int x = 0; x < tetromino.sprites.GetLength(0); x++)
            {
                for (int y = 0; y < tetromino.sprites.GetLength(1); y++)
                {
                    if (tetromino.sprites[x, y] != null)
                    {
                        tetromino.sprites[x, y].Render(Batcher);
                    }
                }
            }
        }

        private void RenderGame()
        {
            backgroundSprite.Render(Batcher);

            for (int x = 0; x < 10; x++)
            {
                for(int y = 0; y < 20; y++)
                {
                    if (settledSprites[x, y] != null)
                    {
                        settledSprites[x, y].Render(Batcher);
                    }
                }
            }

            RenderTetromino(Batcher);

        }

        Sprite CreateTetroSprite(Color color)
        {
            var sprite = new Sprite("tetro", tetroAseprite);
            sprite.Play("idle");
            sprite.Color = color;
            return sprite;
        }

        #region Boilerplate
        // This is called when the Application is shutting down
        // (or when the Module is removed)
        protected override void Shutdown()
        {
            // Remove our Callback
            App.Window.OnRender -= Render;
        }

        private void Render(Window window)
        {
            // clear the Window
            App.Graphics.Clear(window, Color.Black);
            App.Graphics.Clear(FrameBuffer, Color.Black);
            Batcher.Clear();

            RenderGame();

            Batcher.Render(FrameBuffer);
            Batcher.Clear();
            int scale = window.RenderHeight / FrameBuffer.RenderHeight;
            Batcher.Image(FrameBuffer, Vector2.Zero, Vector2.One * scale, Vector2.Zero, 0, Color.White);
            Batcher.Render(window);
        }
        #endregion
    }
}
