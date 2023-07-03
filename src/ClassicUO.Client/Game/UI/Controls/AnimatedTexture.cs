using System;
using System.IO;
using ClassicUO.IO.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls
{
    //This is for gumps, unlikely to ever need to be exported via external libs
    internal class UOAnimatedTexture
    {
        public byte[][] RawTextures;
        public Texture2D[] Textures;
        public int FPS;
        public UOSound SoundData;

        public UOAnimatedTexture(byte[][] textures, int fps, UOSound sound)
        {
            RawTextures = textures;
            Textures = new Texture2D[textures.Length];
            FPS = fps;
            SoundData = sound;
        }

        public Texture2D GetTexture(int num)
        {
            try
            {
                if (num >= Textures.Length)
                {
                    return null;
                }

                if (Textures[num] == null)
                {

                    var texture = Texture2D.FromStream(Client.Game.GraphicsDevice, new MemoryStream(RawTextures[num]));
                    //texture.PushData(pixels);
                    Textures[num] = texture;
                }

                return Textures[num];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }
        }
    }
}