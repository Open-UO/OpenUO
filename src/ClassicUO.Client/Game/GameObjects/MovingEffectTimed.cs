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

using System;
using ClassicUO.Game.Managers;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects
{
    internal sealed class MovingEffectTimed : GameEffect
    {        
        public MovingEffectTimed
        (
            EffectManager manager,
            uint src,
            uint trg,
            ushort xSource,
            ushort ySource,
            sbyte zSource,
            ushort xTarget,
            ushort yTarget,
            sbyte zTarget,
            ushort graphic,
            ushort hue,
            short fixedDir,
            int duration,
            byte speed,
            TimeSpan durationToTarget, short spinning
        ) : base(manager, graphic, hue, duration, speed)
        {
            _DurationToTarget = durationToTarget;
            FixedDir = fixedDir;
            if (FixedDir != -1)
                AngleToTarget = FixedDir / 360f;

            _RealStartTime = _StartTime = DateTime.UtcNow;
            _Spinning = spinning;

            // we override interval time with speed
            IntervalInMs = speed;
            _MoveSpeed = speed;

            if (speed > 7)
                _MoveSpeed = 7;
            
            Offset.X += 22;

            Entity source = World.Get(src);

            if (SerialHelper.IsValid(src) && source != null)
            {
                SetSource(source);
                _StartX = source.X;
                _StartY = source.Y;
                _StartZ = source.Z;
            }
            else
            {
                SetSource(xSource, ySource, zSource);
                _StartX = (ushort)xSource;
                _StartY = (ushort)ySource;
                _StartZ = (sbyte)zSource;
            }


            Entity target = World.Get(trg);

            if (SerialHelper.IsValid(trg) && target != null)
            {
                SetTarget(target);
            }
            else
            {
                SetTarget(xTarget, yTarget, zTarget);
            }
        }

        public readonly short FixedDir;
        private TimeSpan? _DurationToTarget = null;
        private DateTime _StartTime;
        private ushort _StartX;
        private ushort _StartY;
        private sbyte _StartZ;
        private DateTime _RealStartTime;
        private short _Spinning = 0;
        private uint _MoveSpeed;


        public override void Update()
        {
            base.Update();
            UpdateOffset();
        }


        private void UpdateOffset()
        {
            if (Target != null && Target.IsDestroyed)
            {
                TargetX = Target.X;
                TargetY = Target.Y;
                TargetZ = Target.Z;
            }
            int playerX = World.Player.X;
            int playerY = World.Player.Y;
            int playerZ = World.Player.Z;

            (int sX, int sY, int sZ) = GetSource();
            sX = _StartX;
            sY = _StartY;
            sZ = _StartZ;
            int offsetSourceX = sX - playerX;
            int offsetSourceY = sY - playerY;
            int offsetSourceZ = sZ - playerZ;

            (int tX, int tY, int tZ) = GetTarget();
            int offsetTargetX = tX - playerX;
            int offsetTargetY = tY - playerY;
            int offsetTargetZ = tZ - playerZ;
           

            Vector2 source = new Vector2((offsetSourceX - offsetSourceY) * 22, (offsetSourceX + offsetSourceY) * 22 - offsetSourceZ * 4);

            //source.X += Offset.X;
            //source.Y += Offset.Y;

            var time = (DateTime.UtcNow - _StartTime);
            var totalSeconds = _DurationToTarget.Value.TotalSeconds;

            Vector2 target = new Vector2((offsetTargetX - offsetTargetY) * 22, (offsetTargetX + offsetTargetY) * 22 - offsetTargetZ * 4);

            //if (target.X >= 0)
            //    target.Y += 22;
            
            //Vector2.Subtract(ref target, ref source, out Vector2 offset);
            var offset = new Vector2(target.X - source.X, target.Y - source.Y);
            Vector2.Distance(ref source, ref target, out float distance);
            //distance -= 22;
            //Vector2.Multiply(ref offset, (IntervalInMs / distance) , out Vector2 s0);
            //Vector2 s0 = new Vector2(distance);

            //Console.WriteLine($"{offset}");

            if (time.TotalSeconds > totalSeconds)
            {
                RemoveMe();
                return;
            }

            IsPositionChanged = true;

            if (_Spinning == 0)
            {
                if (FixedDir == -1)
                    AngleToTarget = (float)Math.Atan2(-offset.Y, -offset.X);
                //oldfactor = Math.Max(-0.5f,Math.Min(0.5f, (oldfactor + ((RandomHelper.GetValue(1, 3) - 2) / 100f))));
                //AngleToTarget += oldfactor;
            }
            else
            {
                AngleToTarget = (float)((DateTime.UtcNow - _RealStartTime).TotalSeconds * _Spinning / 57f);
            }
            
            
            Vector2.Multiply(ref offset, (float)((time.TotalSeconds / totalSeconds)) , out Vector2 s0);
            Offset.X = s0.X;
            Offset.Y = s0.Y;
        }


        private void RemoveMe()
        {
            CreateExplosionEffect();

            Destroy();
        }

        private static void TileOffsetOnMonitorToXY(ref int ofsX, ref int ofsY, out int x, out int y)
        {
            y = 0;

            if (ofsX == 0)
            {
                x = y = ofsY >> 1;
            }
            else if (ofsY == 0)
            {
                x = ofsX >> 1;
                y = -x;
            }
            else
            {
                int absX = Math.Abs(ofsX);
                int absY = Math.Abs(ofsY);
                x = ofsX;

                if (ofsY > ofsX)
                {
                    if (ofsX < 0 && ofsY < 0)
                    {
                        y = absX - absY;
                    }
                    else if (ofsX > 0 && ofsY > 0)
                    {
                        y = absY - absX;
                    }
                }
                else if (ofsX > ofsY)
                {
                    if (ofsX < 0 && ofsY < 0)
                    {
                        y = -(absY - absX);
                    }
                    else if (ofsX > 0 && ofsY > 0)
                    {
                        y = -(absX - absY);
                    }
                }

                if (y == 0 && ofsY != ofsX)
                {
                    if (ofsY < 0)
                    {
                        y = -(absX + absY);
                    }
                    else
                    {
                        y = absX + absY;
                    }
                }

                y /= 2;
                x += y;
            }
        }
    }
}