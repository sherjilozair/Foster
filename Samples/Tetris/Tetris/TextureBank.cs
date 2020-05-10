using System;
using System.IO;
using System.Diagnostics;
using Foster.Framework;
using System.Collections.Generic;

namespace Tetris
{
    public class TextureBank
    {

        static Packer Packer = new Packer();
        static List<Texture> TexturePages = new List<Texture>();
        static bool finalized;

        public static void AddAseprite(string name, Aseprite aseprite)
        {
            aseprite.Pack(string.Format("{0}/{1}", name, "{0}"), Packer);
        }

        public static void AddImage(string name, Stream stream)
        {
            Bitmap bitmap = new Bitmap(stream);
            Packer.AddPixels(name, bitmap.Width, bitmap.Height, bitmap.Pixels);
        }

        public static void PackAndFinalize()
        {
            Debug.Assert(!finalized);
            Packer.Pack();
            foreach (Bitmap page in Packer.Packed.Pages)
            {
                TexturePages.Add(new Texture(page));
            }
            finalized = true;
        }

        public static Subtexture Get(string name, int frameNumber)
        {
            Debug.Assert(finalized);
            string key = string.Format("{0}/{1}", name, frameNumber);
            Packer.Entry entry = Packer.Packed.Entries[key];
            Texture texture = TexturePages[entry.Page];
            return new Subtexture(texture, entry.Source, entry.Frame);
        }
    }
}
