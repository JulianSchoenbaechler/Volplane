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
    using System;
    using System.Reflection;

    public class ReflectionEvent
    {
        protected object eventHolder;
        protected EventInfo eventInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionEvent"/> class.
        /// </summary>
        /// <param name="eventHolder">The object instance that holds / owns the event.</param>
        /// <param name="eventInfo">The reflected event.</param>
        public ReflectionEvent(object eventHolder, EventInfo eventInfo)
        {
            if(eventHolder == null)
                throw new ArgumentNullException("eventHolder", "Argument cannot be null!");

            if(eventInfo == null)
                throw new ArgumentNullException("eventInfo", "Argument cannot be null!");

            this.eventHolder = eventHolder;
            this.eventInfo = eventInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionEvent"/> class.
        /// </summary>
        /// <param name="eventHolder">The object instance that holds / owns the event.</param>
        /// <param name="eventInfo">The reflected event.</param>
        /// <param name="handler">The event handler delegate.</param>
        public ReflectionEvent(object eventHolder, EventInfo eventInfo, Delegate handler) : this(eventHolder, eventInfo)
        {
            this.EventDelegate = handler;
        }

        /// <summary>
        /// Gets or sets the delegate that acts as event handler.
        /// </summary>
        /// <value>Event handler delegate.</value>
        public Delegate EventDelegate { get; set; }

        /// <summary>
        /// Subscribe delegate to event through reflection.
        /// </summary>
        public void Add()
        {
            if(EventDelegate == null)
                throw new NullReferenceException("ReflectionEvent delegate cannot be null!");

            eventInfo.GetAddMethod().Invoke(eventHolder, new[] { EventDelegate });
        }

        /// <summary>
        /// Unsubscribe delegate from event through reflection.
        /// </summary>
        public void Remove()
        {
            if(EventDelegate == null)
                throw new NullReferenceException("ReflectionEvent delegate cannot be null!");

            eventInfo.GetRemoveMethod().Invoke(eventHolder, new[] { EventDelegate });
        }
    }
}
