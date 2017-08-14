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
        private static Dictionary<string, BasicInput> inputs;
        private AirConsoleAgent agent;

        public ACInput(AirConsoleAgent agent)
        {
            this.agent = agent;
            this.agent.onMessage += CheckACMessage;
        }

        private static Dictionary<string, BasicInput> Inputs
        {
            get
            {
                if(ACInput.inputs == null)
                    ACInput.inputs = new Dictionary<string, BasicInput>();

                return ACInput.inputs;
            }
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
            this.agent.onMessage -= CheckACMessage;
        }

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
            BasicInput element;

            switch(inputData["type"].Value)
            {
                case "dpad":

                    // First, never occured input?
                    if(!ACInput.Inputs.ContainsKey(elementName))
                    {
                        element = new DPadInput();
                        ACInput.Inputs.Add(elementName, element);
                    }
                    else
                    {
                        element = ACInput.Inputs[elementName];
                    }

                    ((DPadInput)element).coordinates = Vector2.up;

                    break;
                
                case "joystick":

                    // First, never occured input?
                    if(!ACInput.Inputs.ContainsKey(elementName))
                    {
                        element = new JoystickInput();
                        ACInput.Inputs.Add(elementName, element);
                    }
                    else
                    {
                        element = ACInput.Inputs[elementName];
                    }

                    break;
                
                case "swipe":

                    // First, never occured input?
                    if(!ACInput.Inputs.ContainsKey(elementName))
                    {
                        element = new SwipeInput();
                        ACInput.Inputs.Add(elementName, element);
                    }
                    else
                    {
                        element = ACInput.Inputs[elementName];
                    }

                    break;
                
                case "touch":

                    // First, never occured input?
                    if(!ACInput.Inputs.ContainsKey(elementName))
                    {
                        element = new TouchInput();
                        ACInput.Inputs.Add(elementName, element);
                    }
                    else
                    {
                        element = ACInput.Inputs[elementName];
                    }

                    break;
                
                default:

                    // First, never occured input?
                    if(!ACInput.Inputs.ContainsKey(elementName))
                    {
                        element = new BasicInput();
                        ACInput.Inputs.Add(elementName, element);
                    }
                    else
                    {
                        element = ACInput.Inputs[elementName];
                    }

                    element.state = inputData["state"].AsBool;
                    element.delay = (int)(agent.GetServerTime() - inputData["timeStamp"].AsLong);

                    break;


            }
        }


        private class BasicInput
        {
            public bool state;
            public int delay;
            protected bool oldState;
            protected bool stateDown;
            protected bool stateUp;

            public BasicInput()
            {
                this.state = false;
                this.oldState = false;
                this.stateDown = false;
                this.stateUp = false;
                this.delay = 0;
            }

            public virtual void Update()
            {
                stateDown = state && !oldState ? true : false;
                stateUp = oldState && !state ? true : false;
                oldState = state;
            }
        }

        private class DPadInput : BasicInput
        {
            public Vector2 coordinates;

            public DPadInput() : base()
            {
                this.coordinates = Vector2.zero;
            }
        }

        private class JoystickInput : DPadInput
        {
            public bool hadDirections;

            public JoystickInput() : base()
            {
                this.hadDirections = false;
            }
        }

        private class SwipeInput : JoystickInput
        {
            public float distance;
            public float angle;
            public float degree;
            public Quaternion rotation;
            public float speed;


            public SwipeInput() : base()
            {
                this.distance = 0f;
                this.angle = 0f;
                this.degree = 0f;
                this.rotation = Quaternion.identity;
                this.speed = 0f;
            }
        }

        private class TouchInput : DPadInput
        {
            public bool move;

            public TouchInput() : base()
            {
                this.move = false;
            }
        }
    }
}
