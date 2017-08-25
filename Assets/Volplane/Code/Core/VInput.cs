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
    using SimpleJSON;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Volplane.AirConsole;

    public sealed class VInput : IDisposable, IControllerUpdate
    {
        /// <summary>
        /// Button events active?.
        /// </summary>
        public static bool ButtonEvents = true;

        /// <summary>
        /// DPad events active?.
        /// </summary>
        public static bool DPadEvents = true;

        /// <summary>
        /// Joystick events active?.
        /// </summary>
        public static bool JoystickEvents = true;

        /// <summary>
        /// Swipe events active?.
        /// </summary>
        public static bool SwipeEvents = true;

        /// <summary>
        /// Touch events active?.
        /// </summary>
        public static bool TouchEvents = true;


        private static List<Dictionary<string, ElementInput>> Inputs;
        private static ElementInput TempInput;

        private ElementInput tempInput;

        public VInput()
        {
            VInput.Inputs = new List<Dictionary<string, ElementInput>>(8);
        }

        public enum Axis
        {
            Horizontal,
            Vertical
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Volplane.ACInput"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="Volplane.ACInput"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="Volplane.ACInput"/> so the garbage
        /// collector can reclaim the memory that the <see cref="Volplane.ACInput"/> was occupying.</remarks>
        public void Dispose()
        {
            //this.agent.onMessage -= CheckACMessage;
        }

        public void ControllerUpdate()
        {
            for(int i = 0; i < VInput.Inputs.Count; i++)
                foreach(ElementInput input in VInput.Inputs[i].Values)
                    input.Update();
        }

        public void ProcessInput(int playerId, JSONNode data)
        {
            int diffPlayerCount = playerId - VInput.Inputs.Count;

            // First input from this player?
            for(int i = 0; i <= diffPlayerCount; i++)
                VInput.Inputs.Add(new Dictionary<string, ElementInput>());

            // Get input object by name, or create new one if not specified yet
            if(!VInput.Inputs[playerId].TryGetValue(data["name"].Value, out tempInput))
            {
                tempInput = new ElementInput();
                Debug.LogFormat("Create new input: playerId {0:D} / name '{1:G}'", playerId, data["name"].Value);
                VInput.Inputs[playerId].Add(data["name"].Value, tempInput);
            }

            tempInput.State = data["data"]["state"].AsBool;
            tempInput.Delay = (int)(VolplaneController.AirConsole.GetServerTime() - data["data"]["timeStamp"].AsLong);

            // Type specific properties
            switch(data["type"].Value)
            {
                case "dpad":
                    tempInput.Type = ElementInput.InputType.DPad;
                    tempInput.Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    tempInput.Tap = !data["data"]["hadDirections"].AsBool;
                    break;

                case "joystick":
                    tempInput.Type = ElementInput.InputType.Joystick;
                    tempInput.Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    tempInput.Tap = !data["data"]["hadDirections"].AsBool;
                    break;

                case "swipe":
                    tempInput.Type = ElementInput.InputType.SwipeField;
                    tempInput.Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    tempInput.Tap = !data["data"]["hadDirections"].AsBool;

                    if(data["data"]["distance"].AsFloat != 0f)
                    {
                        tempInput.Distance = data["data"]["distance"].AsFloat;
                        tempInput.Angle = data["data"]["angle"].AsFloat;
                        tempInput.Degree = data["data"]["degree"].AsFloat;
                        tempInput.Speed = data["data"]["speed"].AsFloat;
                    }
                    break;

                case "touch":
                    tempInput.Type = ElementInput.InputType.TouchArea;
                    tempInput.Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    tempInput.Move = data["data"]["move"].AsBool;
                    break;

                default:
                    tempInput.Type = ElementInput.InputType.Button;
                    break;
            }
        }

        /// <summary>
        /// Returns the value of the virtual axis identified by 'axis'.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetAxis(int playerId, string elementName, Axis axis)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The value of specified axis.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="axis">Virtual axis.</param>
        public static float GetAxis(VPlayer player, string elementName, Axis axis)
        {
            return VInput.GetAxis(player.PlayerId, elementName, axis);
        }

        /// <summary>
        /// Returns the value of the virtual axis identified by 'axis'.
        /// </summary>
        /// <returns>The value of specified axis.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="axis">Virtual axis.</param>
        public static float GetAxis(int playerId, string elementName, Axis axis)
        {
            switch(axis)
            {
                case Axis.Horizontal:
                    return VInput.GetCoordinates(playerId, elementName).x;

                case Axis.Vertical:
                    return VInput.GetCoordinates(playerId, elementName).y;
                    break;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Returns the value of the virtual axes or touch coordinates.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetCoordinates(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The virtual axes or touch coordinates.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static Vector2 GetCoordinates(VPlayer player, string elementName)
        {
            return VInput.GetCoordinates(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns the value of the virtual axes or touch coordinates.
        /// </summary>
        /// <returns>The virtual axes or touch coordinates.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static Vector2 GetCoordinates(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if((VInput.TempInput.Type != ElementInput.InputType.Button) &&
                       (VInput.TempInput.Type != ElementInput.InputType.SwipeField))
                        return VInput.TempInput.Coordinates;
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Indicates whether an input element was touched but has not been moved enough to trigger.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetTap(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, on tap, otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetTap(VPlayer player, string elementName)
        {
            return VInput.GetTap(player.PlayerId, elementName);
        }

        /// <summary>
        /// Indicates whether an input element was touched but has not been moved enough to trigger.
        /// </summary>
        /// <returns><c>true</c>, on tap, otherwise <c>false</c>.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetTap(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.Tap;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> while the user touches a button. Think auto fire.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetButton(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, while touched, otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButton(VPlayer player, string elementName)
        {
            return VInput.GetButton(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns <c>true</c> while the user touches a button. Think auto fire.
        /// </summary>
        /// <returns><c>true</c>, while touched, otherwise <c>false</c>.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButton(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.State;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> during the frame the user starts touching a button.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetButtonDown(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, on touch , otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButtonDown(VPlayer player, string elementName)
        {
            return VInput.GetButtonDown(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns <c>true</c> during the frame the user starts touching a button.
        /// </summary>
        /// <returns><c>true</c>, on touch , otherwise <c>false</c>.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButtonDown(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.StateDown;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> during the frame the user releases a button.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetButtonUp(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, on release , otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButtonUp(VPlayer player, string elementName)
        {
            return VInput.GetButtonUp(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns <c>true</c> during the frame the user releases a button.
        /// </summary>
        /// <returns><c>true</c>, on release , otherwise <c>false</c>.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButtonUp(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.StateUp;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the triggered vector of a swipe field.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetSwipeVector(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The swipe vector.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static Vector2 GetSwipeVector(VPlayer player, string elementName)
        {
            return VInput.GetSwipeVector(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns the triggered vector of a swipe field.
        /// </summary>
        /// <returns>The swipe vector.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static Vector2 GetSwipeVector(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                        return VInput.TempInput.Coordinates;
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Returns the triggered moved distance of a swipe field.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetSwipeDistance(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The swipe distance.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static float GetSwipeDistance(VPlayer player, string elementName)
        {
            return VInput.GetSwipeDistance(player.PlayerId, elementName);
        }

        /// <summary>
        /// Returns the triggered moved distance of a swipe field.
        /// </summary>
        /// <returns>The swipe distance.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static float GetSwipeDistance(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                        return VInput.TempInput.Distance;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Returns the calculated angle in radians of a triggered swipe field.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetSwipeAngle(int playerId, string elementName, bool align = false)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The swipe angle.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="align">Align the angle to global up vector (x = 0 / y = 1).</param>
        public static float GetSwipeAngle(VPlayer player, string elementName, bool align = false)
        {
            return VInput.GetSwipeAngle(player.PlayerId, elementName, align);
        }

        /// <summary>
        /// Returns the calculated angle in radians of a triggered swipe field.
        /// </summary>
        /// <returns>The swipe angle.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="align">Align the angle to global up vector (x = 0 / y = 1).</param>
        public static float GetSwipeAngle(int playerId, string elementName, bool align = false)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                    {
                        if(align)
                        {
                            float aligned = VInput.TempInput.Angle + (Mathf.PI / 4f);

                            return aligned > (2f * Mathf.PI) ? aligned - (2f * Mathf.PI) : aligned;
                        }
                        else
                        {
                            return VInput.TempInput.Angle;
                        }
                    }
                }
            }

            return 0f;
        }

        /// <summary>
        /// Returns the calculated angle in degree of a triggered swipe field.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetSwipeDegree(int playerId, string elementName, bool align = false)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>The swipe angle.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="align">Align the angle to global up vector (x = 0 / y = 1).</param>
        public static float GetSwipeDegree(VPlayer player, string elementName, bool align = false)
        {
            return VInput.GetSwipeAngle(player.PlayerId, elementName, align);
        }

        /// <summary>
        /// Returns the calculated angle in degree of a triggered swipe field.
        /// </summary>
        /// <returns>The swipe angle.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        /// <param name="align">Align the angle to global up vector (x = 0 / y = 1).</param>
        public static float GetSwipeDegree(int playerId, string elementName, bool align = false)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                    {
                        if(align)
                        {
                            float aligned = VInput.TempInput.Degree + 90f;

                            return aligned > 360f ? aligned - 360f : aligned;
                        }
                        else
                        {
                            return VInput.TempInput.Degree;
                        }
                    }
                }
            }

            return 0f;
        }

        /// <summary>
        /// Indicates whether an input on a touch area was moved.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetTouchMove(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, if moved, otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetTouchMove(VPlayer player, string elementName)
        {
            return GetTouchMove(player.PlayerId, elementName);
        }

        /// <summary>
        /// Indicates whether an input on a touch area was moved.
        /// </summary>
        /// <returns><c>true</c>, if moved, otherwise <c>false</c>.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetTouchMove(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.TouchArea)
                        return VInput.TempInput.Move;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the client to server delay from an input on a players controller in milliseconds.
        /// </summary>
        /// <returns>The input delay in milliseconds.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static int GetInputDelay(VPlayer player, string elementName)
        {
            return GetInputDelay(player, elementName);
        }

        /// <summary>
        /// Returns the client to server delay from an input on a players controller in milliseconds.
        /// </summary>
        /// <returns>The input delay in milliseconds.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static int GetInputDelay(int playerId, string elementName)
        {
            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.Delay;
                }
            }

            return -1;
        }



        /// <summary>
        /// Local representation of an element input.
        /// </summary>
        private class ElementInput
        {
            protected bool oldState;
            protected Vector2 coordinates;

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VInput+ElementInput"/> class.
            /// </summary>
            public ElementInput()
            {
                // Standard values
                this.oldState = false;
                this.coordinates = Vector2.zero;

                this.Type = InputType.Button;
                this.State = false;
                this.Coordinates = Vector2.zero;
                this.Tap = false;
                this.Distance = 0f;
                this.Angle = 0f;
                this.Degree = 0f;
                this.Rotation = Quaternion.identity;
                this.Speed = 0f;
                this.Move = false;
                this.Delay = 0;
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
                TouchArea
            }

            public InputType Type { get; set; }
            public bool State { get; set; }
            public bool Tap { get; set; }
            public float Distance { get; set; }
            public float Angle { get; set; }
            public float Degree { get; set; }
            public Quaternion Rotation { get; set; }
            public float Speed { get; set; }
            public bool Move { get; set; }
            public int Delay { get; set; }

            public Vector2 Coordinates
            {
                get { return coordinates; }
                set { coordinates = value; }
            }

            public bool StateDown { get; protected set; }
            public bool StateUp { get; protected set; }

            /// <summary>
            /// Update must be called every frame.
            /// </summary>
            public virtual void Update()
            {
                if(StateUp)
                    StateUp = false;

                if(StateDown)
                {
                    StateDown = false;
                    Tap = false;
                }
                
                if(State && !oldState)
                    StateDown = true;
                else if(!State && oldState)
                    StateUp = true;
                
                oldState = State;
            }
        }
    }
}
