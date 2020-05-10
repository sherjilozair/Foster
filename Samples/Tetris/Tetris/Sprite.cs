using System;
using System.Diagnostics;
using System.Numerics;
using Foster.Framework;

namespace Tetris
{
    public class Sprite
    {
        public Vector2 Position { get; set; }
        public Color Color { get; set; } = Color.White;

        private Aseprite.Tag Tag;

        private (int frameNumber, float extra, bool reverse) state = (0, 0f, false);

        private readonly Aseprite aseprite;

        private string tagName;
        string spriteName;

        public Sprite(string spriteName, Aseprite aseprite)
        {
            this.aseprite = aseprite;
            this.spriteName = spriteName;
        }

        public void Play(string tagName)
        {
            if (this.tagName != tagName)
            {
                this.tagName = tagName;
                Refresh();
            }

        }

        void Refresh()
        {
            bool found = false;
            foreach (Aseprite.Tag tag in aseprite.Tags)
            {
                if (tag.Name == tagName)
                {
                    Tag = tag;
                    found = true;
                    break;
                }
            }
            Debug.Assert(found, "Tag not found", "{0}", tagName);

            state.frameNumber = Tag.From;
            state.extra = 0f;
            state.reverse = false;
        }

        public void Update()
        {
            state.extra += Time.Delta;
            float frameDuration = aseprite.Frames[state.frameNumber].Duration / 1000.0f;
            if (state.extra >= frameDuration)
            {
                UpdateFrameNumber();
                state.extra -= frameDuration;
            }
        }

        private void UpdateFrameNumber()
        {
            if (Tag.LoopDirection == Aseprite.Tag.LoopDirections.Forward || (Tag.LoopDirection == Aseprite.Tag.LoopDirections.PingPong && !state.reverse))
            {
                if (state.frameNumber < Tag.To)
                {
                    state.frameNumber++;
                }
                else
                {
                    state.frameNumber = Tag.From;
                    if (Tag.LoopDirection == Aseprite.Tag.LoopDirections.PingPong)
                    {
                        state.reverse = !state.reverse;
                    }
                }
            }
            if (Tag.LoopDirection == Aseprite.Tag.LoopDirections.Reverse || (Tag.LoopDirection == Aseprite.Tag.LoopDirections.PingPong && state.reverse))
            {
                if (state.frameNumber > Tag.From)
                {
                    state.frameNumber--;
                }
                else
                {
                    state.frameNumber = Tag.To;
                    if (Tag.LoopDirection == Aseprite.Tag.LoopDirections.PingPong)
                    {
                        state.reverse = !state.reverse;
                    }
                }
            }

        }

        public void Render(Batch2D batch)
        {
            Subtexture subtexture = TextureBank.Get(spriteName, state.frameNumber);
            batch.Image(subtexture, Position, Color);
        }
    }
}
