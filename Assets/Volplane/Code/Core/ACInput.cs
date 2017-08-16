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

    public sealed class ACInput : IDisposable
    {
        //private static Dictionary<string, ElementInput> inputs;
        private AirConsoleAgent agent;

        public ACInput(AirConsoleAgent agent)
        {
            this.agent = agent;
            //this.agent.onMessage += CheckACMessage;
        }
        /*
        private static Dictionary<string, ElementInput> Inputs
        {
            get
            {
                if(ACInput.inputs == null)
                    ACInput.inputs = new Dictionary<string, ElementInput>();

                return ACInput.inputs;
            }
        }

        //public static bool GetButton(string name
        */

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
        /*
        private void CheckACMessage(int acDeviceIdSender, JSONNode data)
        {
            // Ignore screen
            if(acDeviceIdSender == 0)
                return;
            
            foreach(string key in data.Keys)
            {
                // Is this a volplane input message?
                if(data[key]["volplane"].Value == "input")
                {
                    ProcessInput(key, data[key]);
                }
            }
        }

        private void ProcessInput(string elementName, JSONNode inputData)
        {
            ElementInput element;
            Vector2 coordinates;

            // First, never occured input?
            if(!ACInput.Inputs.ContainsKey(elementName))
            {
                element = new ElementInput();
                ACInput.Inputs.Add(elementName, element);
            }
            else
            {
                element = ACInput.Inputs[elementName];
            }

            // Handling data according to type
            switch(inputData["type"].Value)
            {
                case "dpad":

                    if(inputData["data"]["x"] == null)
                    {
                        element.State = inputData["data"]["state"].AsBool;
                        element.HadDirections = element.State ? false : inputData["data"]["hadDirections"].AsBool;
                    }
                    else
                    {
                        // Set direction?
                        if(inputData["data"]["state"].AsBool)
                        {
                            // Set direction...
                            coordinates = element.Coordinates;
                            coordinates.Set(
                                inputData["data"]["x"].AsInt == 0 ? coordinates.x : inputData["data"]["x"].AsFloat, // Affected x axis
                                inputData["data"]["y"].AsInt == 0 ? coordinates.y : inputData["data"]["y"].AsFloat  // Affected y axis
                            );
                            element.Coordinates = coordinates;
                        }
                        else
                        {
                            // Reset direction...
                            coordinates = element.Coordinates;
                            coordinates.Set(
                                inputData["data"]["x"].AsInt == 0 ? coordinates.x : 0,                              // Affected x axis
                                inputData["data"]["y"].AsInt == 0 ? coordinates.y : 0                               // Affected y axis
                            );
                            element.Coordinates = coordinates;
                        }
                    }

                    break;

                case "joystick":

                    break;

                case "swipe":

                    break;

                case "touch":

                    break;

                default:

                    element.State = inputData["data"]["state"].AsBool;
                    break;

            }

            // Timestamp
            if(inputData["data"]["timeStamp"].AsLong != 0)
                element.Delay = (int)(agent.GetServerTime() - inputData["data"]["timeStamp"].AsLong);
        }


        private class ElementInput
        {
            protected bool oldState;
            protected Vector2 oldCoordinates;

            public ElementInput()
            {
                this.oldState = false;
                this.oldCoordinates = Vector2.zero;

                this.State = false;
                this.Coordinates = Vector2.zero;
                this.HadDirections = false;
                this.Distance = 0f;
                this.Angle = 0f;
                this.Degree = 0f;
                this.Rotation = Quaternion.identity;
                this.Speed = 0f;
                this.Move = false;
                this.Delay = 0;
            }

            public enum InputDirection
            {
                Up,
                Down,
                Right,
                Left
            }

            public bool State { get; set; }
            public Vector2 Coordinates { get; set; }
            public bool HadDirections { get; set; }
            public float Distance { get; set; }
            public float Angle { get; set; }
            public float Degree { get; set; }
            public Quaternion Rotation { get; set; }
            public float Speed { get; set; }
            public bool Move { get; set; }
            public int Delay { get; set; }

            public bool StateDown
            {
                get
                {
                    return State && !oldState ? true : false;
                }
            }

            public bool StateUp
            {
                get
                {
                    return oldState && !State ? true : false;
                }
            }

            public bool CoordinatesDown(InputDirection dir)
            {
                switch(dir)
                {
                    case InputDirection.Up:
                        return (oldCoordinates.y == 0) && (Coordinates.y > 0);

                    case InputDirection.Down:
                        return (oldCoordinates.y == 0) && (Coordinates.y < 0);

                    case InputDirection.Right:
                        return (oldCoordinates.x == 0) && (Coordinates.x > 0);

                    default:
                        return (oldCoordinates.x == 0) && (Coordinates.x < 0);
                }
            }

            public bool CoordinatesUp(InputDirection dir)
            {
                switch(dir)
                {
                    case InputDirection.Up:
                        return (Coordinates.y == 0) && (oldCoordinates.y > 0);

                    case InputDirection.Down:
                        return (Coordinates.y == 0) && (oldCoordinates.y < 0);

                    case InputDirection.Right:
                        return (Coordinates.x == 0) && (oldCoordinates.x > 0);

                    default:
                        return (Coordinates.x == 0) && (oldCoordinates.x < 0);
                }
            }

            public virtual void Update()
            {
                oldState = State;
                oldCoordinates = Coordinates;
            }
        }*/
    }
}
