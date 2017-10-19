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

namespace Volplane.AirConsole
{
    using System;

    public class AirConsoleNotReadyException : Exception
    {
        public AirConsoleNotReadyException() : base() { }
        public AirConsoleNotReadyException(string message) : base(message) { }
        public AirConsoleNotReadyException(string message, string acFunction) : this(message, null, acFunction) { }
        public AirConsoleNotReadyException(string message, Exception inner) : base(message, inner) { }
        public AirConsoleNotReadyException(string message, Exception inner, string acFunction) : base(message, inner)
        {
            this.AirConsoleFunction = acFunction;
        }

        public string AirConsoleFunction { get; set; }
    }
}
