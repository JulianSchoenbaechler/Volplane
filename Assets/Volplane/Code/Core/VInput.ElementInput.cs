/*
 * Copyright - Julian Schoenbaechler
 * https://github.com/JulianSchoenbaechler/Volplane
 *
 * This file is part of the Volplane project.
 *
 * The Volplane project is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * The Volplane project is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with the Volplane project.
 * If not, see http://www.gnu.org/licenses/.
 */

namespace Volplane
{
    using Newtonsoft.Json;
    using System.IO;
	using UnityEngine;

    public sealed partial class VInput
    {
        /// <summary>
        /// Local representation of an element input.
        /// </summary>
        private class ElementInput
        {
            protected bool oldState;

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VInput+ElementInput"/> class.
            /// </summary>
            public ElementInput()
            {
                // Standard values
                this.oldState = false;

                this.Type = InputType.Button;
                this.State = false;

                this.StateDown = false;
                this.StateUp = false;
                this.Tap = false;

                this.X = 0f;
                this.Y = 0f;

                this.HadDirections = true;
                this.Move = false;

                this.AX = 0f;
                this.AY = 0f;
                this.AZ = 0f;
                this.Alpha = 0f;
                this.Beta = 0f;
                this.Gamma = 0f;

                this.Distance = 0f;
                this.Angle = 0f;
                this.Degree = 0f;
                this.Speed = 0f;

                this.Delay = 0;
                this.Dirty = false;
            }

            /// <summary>
            /// Input type.
            /// </summary>
            public enum InputType
            {
                Button,
                DPad,
                Joystick,
                SwipeField,
                TouchArea,
                Motion
            }

            public InputType Type { get; set; }             // Input type
            public bool State { get; set; }                 // Input state

            public bool StateDown { get; protected set; }   // Input state per frame - down
            public bool StateUp { get; protected set; }     // Input state per frame -  up
            public bool Tap { get; protected set; }         // Tap input indicator
            public bool Move { get; set; }                  // Moved input indicator
            public bool HadDirections { get; set; }         // Triggered directions input indicator

            public float X  { get; set; }                   // Coordinates
            public float Y  { get; set; }

            public float Distance { get; set; }             // Swipe distance
            public float Angle { get; set; }                // Swipe angle in radians
            public float Degree { get; set; }               // Swipe angle in degree
            public float Speed { get; set; }                // Swipe speed

            public float AX  { get; set; }                  // Accelerometer (x, y, z)
            public float AY  { get; set; }
            public float AZ  { get; set; }
            public float Alpha  { get; set; }               // Gyroscope (alpha, beta, gamma)
            public float Beta  { get; set; }
            public float Gamma  { get; set; }

            public int Delay { get; set; }                  // Connection delay
            public bool Dirty { get; set; }                 // Changed input indicator

            /// <summary>
            /// Update must be called every frame.
            /// </summary>
            public virtual void Update()
            {
                if(Tap)
                    Tap = false;

                if(StateUp)
                {
                    StateUp = false;

                    if(!HadDirections)
                        Tap = true;
                }

                if(StateDown)
                    StateDown = false;

                if(State && !oldState)
                    StateDown = true;
                else if(!State && oldState)
                    StateUp = true;

                oldState = State;

                Dirty = false;
            }
        }
    }
}
