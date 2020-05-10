using System;
using System.Numerics;
using Foster.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        static Aseprite backgroundAseprite;
        static Aseprite tetroAseprite;

        Sprite[,] settledSprites = new Sprite[10, 20];
        Vector2 Offset = new Vector2(120, 10);
        Vector2 NextTetrominoOffset = new Vector2(71, 17);
        Vector2 HeldTetrominoOffset = new Vector2(217, 17);

        Sprite backgroundSprite;

        Random rng = new Random();

        SpriteFont font;

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
            

            RefillBag();

            font = new SpriteFont("../../../Assets/ChevyRay - Little League.ttf", 7, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890", TextureFilter.Nearest);

        }

        class Tetromino
        {
            public Tetro type;
            public (int x, int y) position;
            public Sprite[,] sprites;

            internal Tetromino(Tetro type, (int x, int y) position)
            {
                this.type = type;
                this.position = position;
                switch (type)
                {
                    case Tetro.I:
                        {
                            sprites = new Sprite[4, 4];
                            Color color = new Color(0, 255, 255, 255); // CYAN
                            sprites[0, 1] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            sprites[3, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.J:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(0, 255, 0, 255); // BLUE
                            sprites[0, 0] = CreateTetroSprite(color);
                            sprites[0, 1] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.L:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(255, 165, 0, 255); // ORANGE
                            sprites[2, 0] = CreateTetroSprite(color);
                            sprites[0, 1] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.O:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(255, 255, 0, 255); // YELLOW
                            sprites[1, 0] = CreateTetroSprite(color);
                            sprites[2, 0] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.S:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(0, 255, 0, 255); // GREEN
                            sprites[1, 0] = CreateTetroSprite(color);
                            sprites[2, 0] = CreateTetroSprite(color);
                            sprites[0, 1] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.T:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(128, 0, 128, 255); // PURPLE
                            sprites[1, 0] = CreateTetroSprite(color);
                            sprites[0, 1] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                    case Tetro.Z:
                        {
                            sprites = new Sprite[3, 3];
                            Color color = new Color(255, 0, 0, 255); // RED
                            sprites[0, 0] = CreateTetroSprite(color);
                            sprites[1, 0] = CreateTetroSprite(color);
                            sprites[1, 1] = CreateTetroSprite(color);
                            sprites[2, 1] = CreateTetroSprite(color);
                            break;
                        }
                }
            }
        }

        Tetromino Clone(Tetromino self)
        {
            Tetromino clone = new Tetromino(self.type, self.position);
            int m = self.sprites.GetLength(0);
            int n = self.sprites.GetLength(1);
            clone.sprites.Initialize();
            for (int x = 0; x < m; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    clone.sprites[x, y] = CreateTetroSprite(self.sprites[x, y].Color);
                }
            }
            return clone;
        }

        Tetromino playingTetromino;
        Tetromino nextTetromino;
        Tetromino heldTetromino;

        float StepTime = 1.0f;
        float StepTimer = 0f;
        float NormalRate = 1f;          // 1 step in 60 frames, 1 step in 1 second
        float SoftDropRate = 20.0f;     // 1 step in 3 frames, 20 steps in 1 second

        bool Settle = false;
        bool EndGame = false;
        bool CanHold = false;
        bool AfterHold = false;
        Tetro? HeldPiece = null;
        Tetro? ForceNext = null;

        int GhostYOffset;

        List<Tetro> Bag = new List<Tetro>();

        protected override void Update()
        {
            if (Settle)
            {
                Settle = false;

                for (int x = 0; x < playingTetromino.sprites.GetLength(0); x++)
                {
                    for (int y = 0; y < playingTetromino.sprites.GetLength(1); y++)
                    {
                        if (playingTetromino.sprites[x, y] != null)
                        {
                            (int x, int y) globalPos = (
                                playingTetromino.position.x - 1 + x,
                                playingTetromino.position.y - 1 + y);


                            if (globalPos.y < 0)
                            {
                                EndGame = true;
                                break;
                            }
                            else
                            {
                                settledSprites[globalPos.x, globalPos.y] = playingTetromino.sprites[x, y];
                            }
                        }
                    }
                    if (EndGame) break;
                }

                playingTetromino = null;

                // check for cleared lines

                int read_y = 20 - 1, write_y = 20 - 1;
                while (read_y >= 0)
                {
                    bool deleteRow = true;
                    for (int x = 0; x < 10; x++)
                    {
                        if (settledSprites[x, read_y] == null)
                        {
                            deleteRow = false;
                            break;
                        }
                    }
                    if (!deleteRow)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            settledSprites[x, write_y] = settledSprites[x, read_y];
                        }
                        write_y--;

                    }
                    read_y--;
                }
            }

            if (EndGame)
            {
                EndGame = false;
                // Handle end game
            }

            if (App.Input.Keyboard.Pressed(Keys.C, Keys.LeftShift, Keys.RightShift))
            {
                if (CanHold)
                {
                    CanHold = false;
                    Console.WriteLine("Can Hold");
                    if (HeldPiece == null)
                    {
                        HeldPiece = playingTetromino.type;
                        playingTetromino = null;
                        AfterHold = true;
                        Console.WriteLine("Adding to Held Queue: {0}", HeldPiece);

                    }
                    else
                    {
                        ForceNext = HeldPiece;
                        HeldPiece = playingTetromino.type;
                        playingTetromino = null;
                        Console.WriteLine("Swapping from Held Queue");
                    }
                    heldTetromino = new Tetromino(HeldPiece.Value, (1, 1));
                }
                else
                {
                    Console.WriteLine("Cannot Hold");
                }
            }

            if (playingTetromino == null)
            {
                Tetro type;

                if (ForceNext == null)
                {
                    
                    type = Bag[0];
                    Bag.RemoveAt(0);
                    RefillBag();
                    if (!AfterHold)
                    {
                        CanHold = true;
                    }
                    AfterHold = false;
                }
                else
                {
                    type = ForceNext.Value;
                    ForceNext = null;
                }

                playingTetromino = new Tetromino(type, (4, -1));
                nextTetromino = new Tetromino(Bag[0], (1, 1));
            }

            StepTimer += Time.Delta * (App.Input.Keyboard.Down(Keys.Down) ? SoftDropRate : NormalRate);
            if (StepTimer > StepTime)
            {
                StepTimer -= StepTime;
                if (CollideAt(playingTetromino, (0, 1)) == CollisionType.None)
                    playingTetromino.position.y += 1;
                else
                {
                    Settle = true;
                    StepTimer = 0f;
                }
            }
            if (App.Input.Keyboard.Pressed(Keys.Space))
            {
                playingTetromino.position.y += GhostYOffset;
                Console.WriteLine("hard dropping with {0} to {1}", GhostYOffset, playingTetromino.position.y);
                Settle = true;
            }
            else if (App.Input.Keyboard.Repeated(Keys.Left, 0.5f, 0.1f))
            {
                if (CollideAt(playingTetromino, (-1, 0)) == CollisionType.None)
                    playingTetromino.position.x--;
            }
            else if (App.Input.Keyboard.Repeated(Keys.Right, 0.5f, 0.1f))
            {
                if (CollideAt(playingTetromino, (1, 0)) == CollisionType.None)
                    playingTetromino.position.x++;
            }
            else if (App.Input.Keyboard.Pressed(Keys.Up))
            {
                var candidate = RotateTetrominoClock(playingTetromino);
                if (candidate != null) playingTetromino = candidate;
            }
            else if (App.Input.Keyboard.Pressed(Keys.Z))
            {
                var candidate = RotateTetrominoCounter(playingTetromino);
                if (candidate != null) playingTetromino = candidate;
            }

            UpdateTetromino(playingTetromino, Offset, (1, 1));

            GhostYOffset = 0;
            while (true)
            {
                if (CollideAt(playingTetromino, (0, GhostYOffset + 1)) != CollisionType.None)
                {
                    break;
                }
                GhostYOffset++;
            }

            UpdateTetromino(nextTetromino, NextTetrominoOffset, (1, 1));
            if (heldTetromino != null)
                UpdateTetromino(heldTetromino, HeldTetrominoOffset, (1, 1));

            backgroundSprite.Play("idle");

            backgroundSprite.Update();

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    if (settledSprites[x, y] != null)
                    {
                        settledSprites[x, y].Position.X = Offset.X + 8 * x;
                        settledSprites[x, y].Position.Y = Offset.Y + 8 * y;
                        settledSprites[x, y].Update();
                    }
                }
            }
        }

        private void RefillBag()
        {
            if (Bag.Count == 0)
            {
                Bag.AddRange((Tetro[])Enum.GetValues(typeof(Tetro)));
                rng.Shuffle(Bag);
            }
        }

        private void UpdateTetromino(Tetromino t, Vector2 offset, (int x, int y) center)
        {
            for (int x = 0; x < t.sprites.GetLength(0); x++)
            {
                for (int y = 0; y < t.sprites.GetLength(1); y++)
                {
                    if (t.sprites[x, y] != null)
                    {
                        (int x, int y) cell = (t.position.x - center.x + x, t.position.y - center.y + y);
                        t.sprites[x, y].Position = new Vector2(offset.X + 8 * cell.x, offset.Y + 8 * cell.y);
                        t.sprites[x, y].Update();
                    }
                }
            }
        }

        enum CollisionType { None, Left, Right, Bottom };
        private CollisionType CollideAt(Tetromino t, (int x, int y) offset)
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

                        if (globalPos.x < 0) return CollisionType.Left;
                        if (globalPos.x >= 10) return CollisionType.Right;
                        if (globalPos.y >= 20) return CollisionType.Bottom;
                        if (globalPos.y < 0) continue;
                        if (settledSprites[globalPos.x, globalPos.y] != null)
                        {
                            if (x > 1) return CollisionType.Right;
                            if (x <= 1) return CollisionType.Left;
                            if (y > 1) return CollisionType.Bottom;
                        };
                    }
                }
            }
            return CollisionType.None;
        }

        
        Tetromino RotateTetrominoClock(Tetromino self)
        {
            Tetromino clone = new Tetromino(self.type, self.position);
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

            // wall kicking (probably not fully compliant)
            CollisionType collision = CollideAt(clone, (0, 0));
            if (collision == CollisionType.Left)
            {
                clone.position.x++;
            }
            if (collision == CollisionType.Right)
            {
                clone.position.x--;
            }
            if (collision == CollisionType.Bottom)
            {
                clone.position.y--;
            }


            if (CollideAt(clone, (0, 0)) == CollisionType.None)
                return clone;
            return null;
        }

        Tetromino RotateTetrominoCounter(Tetromino self)
        {
            Tetromino clone = new Tetromino(self.type, self.position);
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

        private void RenderTetromino(Batch2D Batcher, Tetromino t, Vector2? offset = null, Color? color = null)
        {
            for (int x = 0; x < t.sprites.GetLength(0); x++)
            {
                for (int y = 0; y < t.sprites.GetLength(1); y++)
                {
                    var sprite = t.sprites[x, y];
                    if (sprite != null)
                    {
                        //const float flashProb = 0.001f;
                        //if (rng.NextFloat() < flashProb)
                        //{
                        //    sprite.Play("flash", () => { sprite.Play("idle"); });
                        //}
                        sprite.Render(Batcher, offset, color);
                    }
                }
            }
        }

        private void RenderGame()
        {
            backgroundSprite.Render(Batcher);

            Batcher.SetScissor(new RectInt((int)Offset.X, (int)Offset.Y, 8 * 10, 8 * 20));
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
            RenderTetromino(Batcher, playingTetromino);

            // Render ghost
            if (GhostYOffset > 0)
            {
                
                Vector2 offset = new Vector2(0, GhostYOffset * 8);
                Color ghostColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                RenderTetromino(Batcher, playingTetromino, offset, ghostColor);
            }

            Batcher.SetScissor(null);

            RenderTetromino(Batcher, nextTetromino);
            if (heldTetromino != null)
            {
                RenderTetromino(Batcher, heldTetromino);
            }

            Batcher.Text(font, "Next", new Vector2(79, 9), new Color(19, 41, 30, 255));
            Batcher.Text(font, "Held", new Vector2(225, 9), new Color(19, 41, 30, 255));
        }

        static Sprite CreateTetroSprite(Color color)
        {
            var sprite = new Sprite("tetro", tetroAseprite);
            sprite.Play("idle");
            sprite.Color = color;
            return sprite;
        }

        protected override void Shutdown()
        {
            App.Window.OnRender -= Render;
        }

        private void Render(Window window)
        {
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
    }
}
