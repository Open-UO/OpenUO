#region license

// Copyright (c) 2021, openuo
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
//    This product includes software developed by ultima-tony - https://github.com/ultima-tony
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

using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers
{
    internal class SelectedMobileManager
    {



        public bool IsEnabled => ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowLastTarget;
        public bool SplitLastTargets => ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.SplitLastTarget;

        public void Draw(UltimaBatcher2D batcher)
        {
            int screenW = ProfileManager.CurrentProfile.GameWindowSize.X;
            int screenH = ProfileManager.CurrentProfile.GameWindowSize.Y;


            if (!IsEnabled)
            {
                return;
            }

            if (SerialHelper.IsMobile(TargetManager.LastTargetInfo.Serial))
                DrawHealthLineWithMath(batcher, TargetManager.LastTargetInfo.Serial, screenW, screenH, 0x7576);

            if (SplitLastTargets)
            {
                if (TargetManager.LastTargetInfo.Serial == TargetManager.LastBeneficialTargetInfo.Serial)
                {
                    DrawHealthLineWithMath
                    (
                        batcher, TargetManager.LastBeneficialTargetInfo.Serial, screenW, screenH,
                        0x7573
                    );
                }
                else if (SerialHelper.IsMobile(TargetManager.LastBeneficialTargetInfo.Serial))
                {
                    DrawHealthLineWithMath
                    (
                        batcher, TargetManager.LastBeneficialTargetInfo.Serial, screenW, screenH,
                        0x7571
                    );
                }
            }
        }

        private void DrawHealthLineWithMath(UltimaBatcher2D batcher, uint serial, int screenW, int screenH, uint indicator)
        {
            Entity entity = World.Get(serial);

            if (entity == null)
            {
                return;
            }

            Point p = entity.RealScreenPosition;
            p.X += (int)entity.Offset.X + 22;
            p.Y -= entity.FrameInfo.Top;

            p = Client.Game.Scene.Camera.WorldToScreen(p);

            DrawHealthLine
            (
                batcher,
                entity,
                p.X,
                p.Y,
                false,
                indicator
            );
        }

        private void DrawHealthLine(UltimaBatcher2D batcher, Entity entity, int x, int y, bool passive, uint indicator)
        {
            if (entity == null)
            {
                return;
            }

            Mobile mobile = entity as Mobile;


            float alpha = passive ? 0.5f : 1.0f;
            ushort hue = mobile != null ? Notoriety.GetHue(mobile.NotorietyFlag) : Notoriety.GetHue(NotorietyFlag.Gray);

            Vector3 hueVec = ShaderHueTranslator.GetHueVector(0, false, alpha);

            if (mobile == null)
            {
                y += 22;
            }


            const int MULTIPLER = 1;

            var texture = GumpsLoader.Instance.GetGumpTexture(0x756F, out var bounds);


            batcher.Draw
            (
                texture,
                new Rectangle
                (
                    x - (bounds.Width / 2),
                    y,
                    bounds.Width,
                    bounds.Height
                ),
                bounds,
                hueVec
            );

            texture = GumpsLoader.Instance.GetGumpTexture(indicator, out bounds);


            batcher.Draw
            (
                texture,
                new Rectangle
                (
                    x - (bounds.Width / 2),
                    y,
                    bounds.Width,
                    bounds.Height
                ),
                bounds,
                hueVec
            );
        }
    }
}