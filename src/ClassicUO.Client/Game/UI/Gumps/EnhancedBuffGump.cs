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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class EnhancedBuffGump : Gump
    {
        private GumpPic _background;
        private Button _button;
        private ushort _graphic;
        private DataBox _box;
        private int _width;
        private bool _expanding;
        private AlphaBlendControl _alphaBlendControl;

        public EnhancedBuffGump() : base(0, 0)
        {
            _expanding = false;
            CanMove = true;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
        }

        public EnhancedBuffGump(int x, int y) : this()
        {
            X = x;
            Y = y;

            _graphic = 0x7580;


            SetInScreen();

            BuildGump();
        }

        public override GumpType GumpType => GumpType.Buff;

        private void BuildGump()
        {

            if (_width < 40)
                _width = 40;
            
            WantUpdateSize = true;

            _box?.Clear();
            _box?.Children.Clear();

            Clear();



            Add(_alphaBlendControl = new AlphaBlendControl(0.1f, Color.Aquamarine)
            {
                Width = _width,
                Height = 50,
            });


            Add
            (
                _button = new Button(0, 0x7585, 0x7589, 0x7589)
                {
                    ButtonAction = ButtonAction.Activate,
                    X = _width - 13,
                    Y = 50 - 13
                }
            );

            _button.MouseDown += (sender, args) =>
            {
                _expanding = true;
                
            };

            _button.MouseUp += (sender, args) =>
            {
                _expanding = false;
            };


            Add
            (
                _box = new DataBox(0, 0, 0, 0)
                {
                    WantUpdateSize = true
                }
            );

            if (World.Player != null)
            {
                foreach (KeyValuePair<uint, EnhancedBuffIcon> k in World.Player.EnhancedBuffIcons)
                {
                    _box.Add(new BuffControlEntry(World.Player.EnhancedBuffIcons[k.Key]));
                }
            }


            UpdateElements();
        }

        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);
            writer.WriteAttributeString("graphic", _graphic.ToString());
            writer.WriteAttributeString("width", ((int)_width).ToString());
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);

            _graphic = ushort.Parse(xml.GetAttribute("graphic"));

            try
            {
                _width = Math.Max(int.Parse(xml.GetAttribute("width")), 40);
            }
            catch
            {
                _width = 200;
            }

            BuildGump();
        }

        protected override void UpdateContents()
        {
            BuildGump();
        }

        private void UpdateElements()
        {
            int yoffset = 0;
            for (int i = 0, xoffset = 0; i < _box.Children.Count; i++, xoffset += 31)
            {
                Control e = _box.Children[i];

                if (5 + xoffset + 31 > _width)
                {
                    xoffset = 0;
                    yoffset += 31;
                }
                e.X = 5 + xoffset;
                e.Y = 5 + yoffset;
            }

            yoffset += 31;
            Height = yoffset + 5;
            _alphaBlendControl.Height = Height;
            _button.Y = yoffset - 7;
        }

        public override void Update()
        {
            if (Mouse.LButtonPressed && _expanding)
            {
                var width = Mouse.Position.X - (X);

                _width = Width = Math.Max(width, 40);

                RequestUpdateContents();
            }
            else
            {
                _expanding = false;
            }

            base.Update();
        }


        private class BuffControlEntry : GumpPic
        {
            private byte _alpha;
            private bool _decreaseAlpha;
            private readonly RenderedText _gText_Stacks;
            private readonly RenderedText _gText_Duration;
            private float _updateTooltipTime;

            public BuffControlEntry(EnhancedBuffIcon icon) : base(0, 0, (ushort)icon.Graphic, 0)
            {
                if (IsDisposed)
                {
                    return;
                }

                Icon = icon;
                _alpha = 0xFF;
                _decreaseAlpha = true;


                _gText_Stacks = RenderedText.Create
                (
                    $"",
                    0xFFFF,
                    2,
                    true,
                    FontStyle.Fixed | FontStyle.BlackBorder,
                    TEXT_ALIGN_TYPE.TS_CENTER,
                    Width
                );

                _gText_Duration = RenderedText.Create
                (
                    "",
                    0xFFFF,
                    2,
                    true,
                    FontStyle.Fixed | FontStyle.BlackBorder,
                    TEXT_ALIGN_TYPE.TS_CENTER,
                    Width
                );


                AcceptMouseInput = true;
                WantUpdateSize = false;
                CanMove = true;

                SetTooltip(icon.Text);
            }

            public EnhancedBuffIcon Icon { get; }


            public override void Update()
            {
                base.Update();

                if (!IsDisposed && Icon != null)
                {
                    int delta = (int) (Icon.Timer - Time.Ticks);

                    if (_updateTooltipTime < Time.Ticks && delta > 0)
                    {
                        TimeSpan span = TimeSpan.FromMilliseconds(delta);

                        SetTooltip
                        (
                            span.TotalDays >= 1.0 ? string.Format(ResGumps.TimeLeftWithDays, Icon.Text, span.Days, span.Hours, span.Minutes, span.Seconds) 
                                : string.Format(ResGumps.TimeLeft, Icon.Text, span.Hours, span.Minutes, span.Seconds)
                        );

                        _updateTooltipTime = (float)Time.Ticks + 1000;

                        if (span.Days > 0)
                            _gText_Duration.Text = string.Format(ResGumps.Span0Days, span.Days);
                        else if (span.Hours > 0)
                            _gText_Duration.Text = string.Format(ResGumps.Span0Hours, span.Hours);
                        else
                            _gText_Duration.Text = span.Minutes > 0 ? $"{span.Minutes}:{span.Seconds:D2}" : $"{span.Seconds:D2}";
                    }

                    if (Icon.Stacks > 1)
                        _gText_Stacks.Text = $"{Icon.Stacks}";

                    if (Icon.Timer != 0xFFFF_FFFF && delta < 10000)
                    {
                        if (delta <= 0)
                        {
                            ((EnhancedBuffGump) Parent.Parent)?.RequestUpdateContents();
                        }
                        else
                        {
                            int alpha = _alpha;
                            int addVal = (10000 - delta) / 600;

                            if (_decreaseAlpha)
                            {
                                alpha -= addVal;

                                if (alpha <= 60)
                                {
                                    _decreaseAlpha = false;
                                    alpha = 60;
                                }
                            }
                            else
                            {
                                alpha += addVal;

                                if (alpha >= 255)
                                {
                                    _decreaseAlpha = true;
                                    alpha = 255;
                                }
                            }

                            _alpha = (byte) alpha;
                        }
                    }
                }
            }

            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                Vector3 hueVec = ShaderHueTranslator.GetHueVector
                (
                    0,
                    false,
                    _alpha / 255f,
                    true
                );

                var texture = GumpsLoader.Instance.GetGumpTexture(Graphic, out var bounds);

                if (texture != null)
                {
                    batcher.Draw
                    (
                        texture,
                        new Vector2(x, y),
                        bounds,
                        hueVec
                    );

                    if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.BuffBarTime)
                    {
                        _gText_Duration.Draw(batcher, x - 3, y + bounds.Height + 3, hueVec.Z);
                    }
                    _gText_Stacks.Draw(batcher, x + bounds.Width / 10, y + bounds.Height / 5, hueVec.Z);
                    //_gText_Stacks.Draw(batcher, x + texture.Width / 2 - 5, y + texture.Height / 2, HueVector.Z);
                }

                return true;
            }

            public override void Dispose()
            {
                _gText_Duration?.Destroy();
                _gText_Stacks?.Destroy();
                base.Dispose();
            }
        }
    }
}