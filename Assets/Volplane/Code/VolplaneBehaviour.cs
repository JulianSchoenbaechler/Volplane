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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class VolplaneBehaviour : MonoBehaviour
    {
        private Type objectType;
        private Type eventType;
        private MethodInfo methodInfo;
        private MethodInfo addMethodInfo;
        private EventInfo eventInfo;
        private Delegate handler;

        private void Initialize()
        {
            objectType = this.GetType();

            eventType = VolplaneController.InputHandling.GetType();

            SubscribeEvent("OnButton", ref eventType);
            SubscribeEvent("OnDPad", ref eventType);
            SubscribeEvent("OnJoystick", ref eventType);
            SubscribeEvent("OnSwipe", ref eventType);
            SubscribeEvent("OnTouch", ref eventType);
            SubscribeEvent("OnAccelerometer", ref eventType);
            SubscribeEvent("OnGyroscope", ref eventType);
        }

        private void SubscribeEvent(string name, ref Type eventHolderType)
        {
            // Get instances event method info
            methodInfo = objectType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            // Method exists?
            if(methodInfo != null)
            {
                // Event from holder element
                eventInfo = eventHolderType.GetEvent(name);

                // Create delegate from child method
                handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo, false);

                // If event delegate parameters matches with the ones from the chil method -> add event handler
                if(handler != null)
                {
                    VolplaneController.InputHandling.OnButton += (Action<int, bool>)handler;
                }
            }
        }
    }
}
