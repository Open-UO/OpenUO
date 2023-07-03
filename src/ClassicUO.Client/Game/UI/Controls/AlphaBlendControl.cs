#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System.Drawing;
using System.Drawing.Drawing2D;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ClassicUO.Game.UI.Controls
{
    internal sealed class AlphaBlendControl : Control
    {
        public AlphaBlendControl(float alpha = 0.5f, Color? color = null)
        {
            if (color == null)
                Color = Color.Black;
            else
                Color = color.Value;
            Alpha = alpha;
            AcceptMouseInput = false;
        }

        public ushort Hue { get; set; }
        public Color Color { get; set; }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, false, Alpha);

            batcher.Draw
            (
                SolidColorTextureCache.GetTexture(Color),
                new Rectangle
                (
                    x,
                    y,
                    Width,
                    Height
                ),
                hueVector
            );

            return true;
        }
    }
    internal sealed class AlphaBlendControlRounded : Control
    {
        
        public System.Drawing.Color Color { get; set; }
        
        private Texture2D GetTexture(GraphicsDevice dev, System.Drawing.Bitmap bmp)
        {
            int[] imgData = new int[bmp.Width * bmp.Height];
            Texture2D texture = new Texture2D(dev, bmp.Width, bmp.Height);

            unsafe
            {
                // lock bitmap
                System.Drawing.Imaging.BitmapData origdata =
                    bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                uint* byteData = (uint*)origdata.Scan0;

                // Switch bgra -> rgba
                for (int i = 0; i < imgData.Length; i++)
                {
                    byteData[i] = (byteData[i] & 0x000000ff) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }

                // copy data
                System.Runtime.InteropServices.Marshal.Copy(origdata.Scan0, imgData, 0, bmp.Width * bmp.Height);

                byteData = null;

                // unlock bitmap
                bmp.UnlockBits(origdata);
            }

            texture.SetData(imgData);

            return texture;
        }

        private Bitmap DrawRoundedRectangle(int width, int height, int cornerradius)
        {
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics newGraphics = Graphics.FromImage(bmp))
            {
                using (Brush brush = new SolidBrush(Color))
                {
                    using (GraphicsPath path = RoundedRect
                    (
                        new System.Drawing.Rectangle(0, 0, width - 2, height - 2), cornerradius, cornerradius, cornerradius,
                        cornerradius
                    ))
                    {

                        newGraphics.FillPath(brush, path);
                    }
                }
            }

            return bmp;
        }
        private GraphicsPath RoundedRect(System.Drawing.Rectangle bounds, int radius1, int radius2, int radius3, int radius4)
        {
            int diameter1 = radius1 * 2;
            int diameter2 = radius2 * 2;
            int diameter3 = radius3 * 2;
            int diameter4 = radius4 * 2;

            System.Drawing.Rectangle arc1 = new System.Drawing.Rectangle(bounds.Location, new Size(diameter1, diameter1));
            System.Drawing.Rectangle arc2 = new System.Drawing.Rectangle(bounds.Location, new Size(diameter2, diameter2));
            System.Drawing.Rectangle arc3 = new System.Drawing.Rectangle(bounds.Location, new Size(diameter3, diameter3));
            System.Drawing.Rectangle arc4 = new System.Drawing.Rectangle(bounds.Location, new Size(diameter4, diameter4));
            GraphicsPath path = new GraphicsPath();

            // top left arc  
            if (radius1 == 0)
            {
                path.AddLine(arc1.Location, arc1.Location);
            }
            else
            {
                path.AddArc(arc1, 180, 90);
            }

            // top right arc  
            arc2.X = bounds.Right - diameter2;
            if (radius2 == 0)
            {
                path.AddLine(arc2.Location, arc2.Location);
            }
            else
            {
                path.AddArc(arc2, 270, 90);
            }

            // bottom right arc  

            arc3.X = bounds.Right - diameter3;
            arc3.Y = bounds.Bottom - diameter3;
            if (radius3 == 0)
            {
                path.AddLine(arc3.Location, arc3.Location);
            }
            else
            {
                path.AddArc(arc3, 0, 90);
            }

            // bottom left arc 
            arc4.X = bounds.Right - diameter4;
            arc4.Y = bounds.Bottom - diameter4;
            arc4.X = bounds.Left;
            if (radius4 == 0)
            {
                path.AddLine(arc4.Location, arc4.Location);
            }
            else
            {
                path.AddArc(arc4, 90, 90);
            }



            path.CloseFigure();
            return path;
        }

        public AlphaBlendControlRounded(float alpha, int width, int height, int cornerradius = 10, System.Drawing.Color? color = null)
        {
            Width = width;
            Height = height;
            Alpha = alpha;
            AcceptMouseInput = false;
            
            if (color == null)
                Color = System.Drawing.Color.Black;
            else
                Color = color.Value;
            
            Texture = GetTexture(Client.Game.GraphicsDevice, DrawRoundedRectangle(Width, Height, 10));
        }

        public ushort Hue { get; set; }
        public Texture2D Texture { get; set; }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            Vector3 hueVec = ShaderHueTranslator.GetHueVector(Hue, false, Alpha);

            batcher.Draw(
                Texture,
                new Rectangle(
                    x,
                    y,
                    Width,
                    Height),
                hueVec);

            return true;
        }
    }
}