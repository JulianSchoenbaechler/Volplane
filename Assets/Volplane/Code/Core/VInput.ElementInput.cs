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
                this.Delay = 0;

                this.StateDown = false;
                this.StateUp = false;

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

            public InputType Type { get; set; }
            public bool State { get; set; }
            public int Delay { get; set; }

            public bool StateDown { get; protected set; }
            public bool StateUp { get; protected set; }

            public bool Dirty { get; set; }

            /// <summary>
            /// Update must be called every frame.
            /// </summary>
            public virtual void Update()
            {
                if(StateUp)
                    StateUp = false;

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

        /// <summary>
        /// Local representation of an advanced element input.
        /// </summary>
        private class AdvancedElementInput : ElementInput
        {
            protected Vector2 coordinates;

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VInput+AdvancedElementInput"/> class.
            /// </summary>
            public AdvancedElementInput() : base()
            {
                // Standard values
                this.coordinates = Vector2.zero;

                this.HadDirections = true;
                this.Move = false;
                this.Rotation = Quaternion.identity;

                this.StateDown = false;
                this.StateUp = false;
                this.Tap = false;
            }

            public bool HadDirections { get; set; }
            public bool Move { get; set; }
            public Quaternion Rotation { get; set; }

            public Vector2 Coordinates
            {
                get { return coordinates; }
                set { coordinates = value; }
            }

            public bool Tap { get; protected set; }

            /// <summary>
            /// Update must be called every frame.
            /// </summary>
            public override void Update()
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

        /// <summary>
        /// Local representation of an analog element input.
        /// </summary>
        private class AnalogElementInput : AdvancedElementInput
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VInput+AnalogElementInput"/> class.
            /// </summary>
            public AnalogElementInput() : base()
            {
                // Standard values
                this.Distance = 0f;
                this.Angle = 0f;
                this.Degree = 0f;
                this.Speed = 0f;
            }

            public float Distance { get; set; }
            public float Angle { get; set; }
            public float Degree { get; set; }
            public float Speed { get; set; }
        }

        /// <summary>
        /// Local representation of a device motion input.
        /// </summary>
        private class DeviceMotionInput : ElementInput
        {
            protected Vector3 accelerometer, gyroscope;

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VInput+DeviceMotionInput"/> class.
            /// </summary>
            public DeviceMotionInput() : base()
            {
                // Standard values
                this.accelerometer = Vector3.zero;
                this.gyroscope = Vector3.zero;
            }

            public Vector3 Accelerometer
            {
                get { return accelerometer; }
                set { accelerometer = value; }
            }

            public Vector3 Gyroscope
            {
                get { return gyroscope; }
                set { gyroscope = value; }
            }
        }
    }
}
