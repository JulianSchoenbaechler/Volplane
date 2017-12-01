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
    using UnityEngine;

    public sealed partial class VInput : IDisposable, IControllerUpdate
    {
        #region Input Event Flags

        /// <summary>
        /// Button events active?
        /// </summary>
        public static bool ButtonEvents = true;

        /// <summary>
        /// DPad events active?
        /// </summary>
        public static bool DPadEvents = true;

        /// <summary>
        /// Joystick events active?
        /// </summary>
        public static bool JoystickEvents = true;

        /// <summary>
        /// Swipe events active?
        /// </summary>
        public static bool SwipeEvents = true;

        /// <summary>
        /// Touch events active?
        /// </summary>
        public static bool TouchEvents = true;

        /// <summary>
        /// Motion events active?
        /// </summary>
        public static bool MotionEvents = true;

        #endregion

        #region Variables and Constructior / Initialization

        private static List<Dictionary<string, ElementInput>> Inputs;
        private static ElementInput TempInput;

        private ElementInput tempInput;
        private Queue<Action> updateQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.VInput"/> class.
        /// </summary>
        public VInput()
        {
            VInput.Inputs = new List<Dictionary<string, ElementInput>>(8);
            this.updateQueue = new Queue<Action>(4);
        }

        /// <summary>
        /// Axis representation.
        /// </summary>
        public enum Axis
        {
            Horizontal,
            Vertical
        }

        #endregion

        #region Input Events

        /// <summary>
        /// Occurs when a button input gets registered.
        /// </summary>
        public event Action<int, string, bool> OnButton;

        /// <summary>
        /// Occurs when a DPad input gets registered.
        /// </summary>
        public event Action<int, string, Vector2> OnDPad;

        /// <summary>
        /// Occurs when a joystick input gets registered.
        /// </summary>
        public event Action<int, string, Vector2> OnJoystick;

        /// <summary>
        /// Occurs when a swipe input gets registered.
        /// </summary>
        public event Action<int, string, Vector2> OnSwipe;

        /// <summary>
        /// Occurs when a touch input gets registered.
        /// </summary>
        public event Action<int, string, Vector2> OnTouch;

        /// <summary>
        /// Occurs when a device motion gets registered.
        /// </summary>
        public event Action<int, Vector3> OnAccelerometer;

        /// <summary>
        /// Occurs when a device motion gets registered.
        /// </summary>
        public event Action<int, Vector3> OnGyroscope;

        #endregion

        #region Public Input Methods

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
            if(player == null)
                return 0f;

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
            if(VInput.Inputs == null)
                return 0f;

            switch(axis)
            {
                case Axis.Horizontal:
                    return VInput.GetCoordinates(playerId, elementName).x;

                case Axis.Vertical:
                    return VInput.GetCoordinates(playerId, elementName).y;

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
            if(player == null)
                return Vector2.zero;

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
            if(VInput.Inputs == null)
                return Vector2.zero;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if((VInput.TempInput.Type != ElementInput.InputType.Button) &&
                       (VInput.TempInput.Type != ElementInput.InputType.SwipeField) &&
                       (VInput.TempInput.Type != ElementInput.InputType.Motion))
                        return ((AdvancedElementInput)VInput.TempInput).Coordinates;
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
            if(player == null)
                return false;

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
            if(VInput.Inputs == null)
                return false;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if((VInput.TempInput.Type != ElementInput.InputType.Button) &&
                        (VInput.TempInput.Type != ElementInput.InputType.Motion))
                        return ((AdvancedElementInput)VInput.TempInput).Tap;
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates whether an input element has been triggered. This method is intended to be used
        /// for touch areas and swipe fields.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetTap(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, if input was received, <c>false</c> otherwise.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetInput(VPlayer player, string elementName)
        {
            if(player == null)
                return false;

            return VInput.GetInput(player.PlayerId, elementName);
        }

        /// <summary>
        /// Indicates whether an input element has been triggered. This method is intended to be used
        /// for touch areas and swipe fields.
        /// </summary>
        /// <returns><c>true</c>, if input was received, <c>false</c> otherwise.</returns>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetInput(int playerId, string elementName)
        {
            if(VInput.Inputs == null)
                return false;

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
        /// Returns <c>true</c> while the user touches a button. Think auto fire.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetButton(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns><c>true</c>, while touched, otherwise <c>false</c>.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static bool GetButton(VPlayer player, string elementName)
        {
            if(player == null)
                return false;

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
            if(VInput.Inputs == null)
                return false;

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
            if(player == null)
                return false;

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
            if(VInput.Inputs == null)
                return false;

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
            if(player == null)
                return false;

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
            if(VInput.Inputs == null)
                return false;

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
            if(player == null)
                return Vector2.zero;

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
            if(VInput.Inputs == null)
                return Vector2.zero;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                        return ((AnalogElementInput)VInput.TempInput).Coordinates;
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
            if(player == null)
                return 0f;

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
            if(VInput.Inputs == null)
                return 0f;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                        return ((AnalogElementInput)VInput.TempInput).Distance;
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
            if(player == null)
                return 0f;

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
            if(VInput.Inputs == null)
                return 0f;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                    {
                        if(align)
                        {
                            float aligned = ((AnalogElementInput)VInput.TempInput).Angle + (Mathf.PI / 4f);

                            return aligned > (2f * Mathf.PI) ? aligned - (2f * Mathf.PI) : aligned;
                        }
                        else
                        {
                            return ((AnalogElementInput)VInput.TempInput).Angle;
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
            if(player == null)
                return 0f;

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
            if(VInput.Inputs == null)
                return 0f;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.SwipeField)
                    {
                        if(align)
                        {
                            float aligned = ((AnalogElementInput)VInput.TempInput).Degree + 90f;

                            return aligned > 360f ? aligned - 360f : aligned;
                        }
                        else
                        {
                            return ((AnalogElementInput)VInput.TempInput).Degree;
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
            if(player == null)
                return false;

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
            if(VInput.Inputs == null)
                return false;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.TouchArea)
                        return ((AdvancedElementInput)VInput.TempInput).Move;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a Vector representing the controller acceleration.
        /// Only returns valid data if the players controller has the 'Track Device Motion' flag set.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetTouchMove(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>Acceleration vector.</returns>
        /// <param name="player">Player object.</param>
        public static Vector3 GetAccelerometer(VPlayer player)
        {
            if(player == null)
                return Vector3.zero;

            return GetAccelerometer(player.PlayerId);
        }

        /// <summary>
        /// Returns a Vector representing the controller acceleration.
        /// Only returns valid data if the players controller has the 'Track Device Motion' flag set.
        /// </summary>
        /// <returns>Acceleration vector.</returns>
        /// <param name="playerId">Player identifier.</param>
        public static Vector3 GetAccelerometer(int playerId)
        {
            if(VInput.Inputs == null)
                return Vector3.zero;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue("volplane-device-motion", out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.Motion)
                        return ((DeviceMotionInput)VInput.TempInput).Accelerometer;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Returns a Vector representing the controller rotation.
        /// Only returns valid data if the players controller has the 'Track Device Motion' flag set.
        /// </summary>
        /// <remarks>Consider using <see cref="VInput.GetTouchMove(int playerId, string elementName)"/>
        /// for a reduced performance impact.</remarks>
        /// <returns>Acceleration vector.</returns>
        /// <param name="player">Player object.</param>
        public static Vector3 GetGyroscope(VPlayer player)
        {
            if(player == null)
                return Vector3.zero;

            return GetGyroscope(player.PlayerId);
        }

        /// <summary>
        /// Returns a Vector representing the controller rotation.
        /// Only returns valid data if the players controller has the 'Track Device Motion' flag set.
        /// </summary>
        /// <returns>Rotation vector (x = alpha, y = beta, z = gamma).</returns>
        /// <param name="playerId">Player identifier.</param>
        public static Vector3 GetGyroscope(int playerId)
        {
            if(VInput.Inputs == null)
                return Vector3.zero;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue("volplane-device-motion", out VInput.TempInput))
                {
                    if(VInput.TempInput.Type == ElementInput.InputType.Motion)
                        return ((DeviceMotionInput)VInput.TempInput).Gyroscope;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Returns the client to server delay from an input on a players controller in milliseconds.
        /// </summary>
        /// <returns>The input delay in milliseconds.</returns>
        /// <param name="player">Player object.</param>
        /// <param name="elementName">Name of the element to check.</param>
        public static int GetInputDelay(VPlayer player, string elementName)
        {
            if(player == null)
                return Int32.MaxValue;

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
            if(VInput.Inputs == null)
                return Int32.MaxValue;

            if(VInput.Inputs.Count > playerId)
            {
                if(VInput.Inputs[playerId].TryGetValue(elementName, out VInput.TempInput))
                {
                    return VInput.TempInput.Delay;
                }
            }

            return -1;
        }

        #endregion

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
            this.OnButton = null;
            this.OnDPad = null;
            this.OnJoystick = null;
            this.OnSwipe = null;
            this.OnTouch = null;
            this.OnAccelerometer = null;
            this.OnGyroscope = null;
        }

        /// <summary>
        /// Processing every frame.
        /// </summary>
        public void ControllerUpdate()
        {
            for(int i = 0; i < VInput.Inputs.Count; i++)
                foreach(ElementInput input in VInput.Inputs[i].Values)
                    input.Update();

            while(updateQueue.Count > 0)
                updateQueue.Dequeue().Invoke();
        }

        #region Input Handling

        /// <summary>
        /// Process inputs.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="data">Input data.</param>
        public void ProcessInput(int playerId, JSONNode data)
        {
            int diffPlayerCount = playerId - VInput.Inputs.Count;

            // First input from this player?
            for(int i = 0; i <= diffPlayerCount; i++)
                VInput.Inputs.Add(new Dictionary<string, ElementInput>());

            // Get input object by name, or create new one if not specified yet
            if(!VInput.Inputs[playerId].TryGetValue(data["name"].Value, out tempInput))
            {
                switch(data["type"].Value)
                {
                    case "button":
                        tempInput = new ElementInput();
                        break;

                    case "swipe":
                        tempInput = new AnalogElementInput();
                        break;

                    case "motion":
                        tempInput = new DeviceMotionInput();
                        break;

                    default:
                        tempInput = new AdvancedElementInput();
                        break;
                }

                VInput.Inputs[playerId].Add(data["name"].Value, tempInput);

                if(Config.DebugLog == (int)DebugState.All)
                    VDebug.LogFormat("[Volplane (Input Handling)] New input registered: '{0:G}'.", data["name"].Value);
            }

            // Enqueue processing if the same input changed state in the same frame
            if(tempInput.Dirty)
            {
                updateQueue.Enqueue(delegate {
                    ProcessInput(playerId, data);
                });

                if(Config.DebugLog == (int)DebugState.All)
                    VDebug.Log("[Volplane (Input Handling)] Queueing multiple inputs from same element.");

                return;
            }

            tempInput.State = data["data"]["state"].AsBool;
            tempInput.Delay = (int)(VolplaneController.AirConsole.GetServerTime() - data["data"]["timeStamp"].AsLong);

            // Type specific properties
            switch(data["type"].Value)
            {
                case "dpad":

                    tempInput.Type = ElementInput.InputType.DPad;
                    ((AdvancedElementInput)tempInput).Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    ((AdvancedElementInput)tempInput).HadDirections = data["data"]["hadDirections"].AsBool;
                    tempInput.Dirty = true;

                    // Event
                    if(VInput.DPadEvents && (OnDPad != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnDPad(playerId, data["name"].Value, ((AdvancedElementInput)tempInput).Coordinates);
                        });
                    }

                    break;

                case "joystick":

                    tempInput.Type = ElementInput.InputType.Joystick;
                    ((AdvancedElementInput)tempInput).Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    ((AdvancedElementInput)tempInput).HadDirections = data["data"]["hadDirections"].AsBool;
                    tempInput.Dirty = true;

                    // Event
                    if(VInput.JoystickEvents && (OnJoystick != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnJoystick(playerId, data["name"].Value, ((AdvancedElementInput)tempInput).Coordinates);
                        });
                    }

                    break;

                case "swipe":

                    tempInput.Type = ElementInput.InputType.SwipeField;
                    ((AdvancedElementInput)tempInput).Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    ((AdvancedElementInput)tempInput).HadDirections = data["data"]["hadDirections"].AsBool;

                    if(data["data"]["distance"].AsFloat != 0f)
                    {
                        ((AnalogElementInput)tempInput).Distance = data["data"]["distance"].AsFloat;
                        ((AnalogElementInput)tempInput).Angle = data["data"]["angle"].AsFloat;
                        ((AnalogElementInput)tempInput).Degree = data["data"]["degree"].AsFloat;
                        ((AnalogElementInput)tempInput).Speed = data["data"]["speed"].AsFloat;
                    }

                    tempInput.Dirty = true;

                    // Event
                    if(VInput.SwipeEvents && (OnSwipe != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnSwipe(playerId, data["name"].Value, ((AdvancedElementInput)tempInput).Coordinates);
                        });
                    }

                    break;

                case "touch":

                    tempInput.Type = ElementInput.InputType.TouchArea;
                    ((AdvancedElementInput)tempInput).Coordinates = new Vector2(data["data"]["x"].AsFloat, data["data"]["y"].AsFloat);
                    ((AdvancedElementInput)tempInput).Move = data["data"]["move"].AsBool;
                    tempInput.Dirty = true;

                    // Event
                    if(VInput.TouchEvents && (OnTouch != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnTouch(playerId, data["name"].Value, ((AdvancedElementInput)tempInput).Coordinates);
                        });
                    }

                    break;

                case "motion":

                    tempInput.Type = ElementInput.InputType.Motion;
                    ((DeviceMotionInput)tempInput).Accelerometer = new Vector3(data["data"]["x"].AsFloat,
                                                                               data["data"]["y"].AsFloat,
                                                                               data["data"]["z"].AsFloat);
                    ((DeviceMotionInput)tempInput).Gyroscope = new Vector3(data["data"]["alpha"].AsFloat,
                                                                           data["data"]["beta"].AsFloat,
                                                                           data["data"]["gamma"].AsFloat);
                    tempInput.Dirty = true;

                    // Events
                    if(VInput.MotionEvents && (OnAccelerometer != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnAccelerometer(playerId, ((DeviceMotionInput)tempInput).Accelerometer);
                        });
                    }

                    if(VInput.MotionEvents && (OnGyroscope != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnGyroscope(playerId, ((DeviceMotionInput)tempInput).Gyroscope);
                        });
                    }

                    break;

                default:

                    tempInput.Type = ElementInput.InputType.Button;
                    tempInput.Dirty = true;

                    // Event
                    if(VInput.ButtonEvents && (OnButton != null))
                    {
                        updateQueue.Enqueue(delegate {
                            OnButton(playerId, data["name"].Value, tempInput.State);
                        });
                    }

                    break;
            }
        }

        #endregion
    }
}
