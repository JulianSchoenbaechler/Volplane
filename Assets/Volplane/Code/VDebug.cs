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

    public class VDebug
    {
        /// <summary>
        /// Logs a message to the Unity console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(object message)
        {
            if(Config.DebugMessages)
                Debug.Log(message);
        }

        /// <summary>
        /// Logs a formatted message to the Unity console.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogFormat(string format, params object[] args)
        {
            if(Config.DebugMessages)
                Debug.LogFormat(format, args);
        }

        /// <summary>
        /// Logs a warning message to the Unity console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(object message)
        {
            if(Config.DebugWarnings)
                Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity console.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogWarningFormat(string format, params object[] args)
        {
            if(Config.DebugWarnings)
                Debug.LogWarningFormat(format, args);
        }

        /// <summary>
        /// Logs an error message to the Unity console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(object message)
        {
            if(Config.DebugErrors)
                Debug.LogError(message);
        }

        /// <summary>
        /// Logs a formatted error message to the Unity console.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
            if(Config.DebugErrors)
                Debug.LogErrorFormat(format, args);
        }

        /// <summary>
        /// Logs an exception to the Unity console.
        /// </summary>
        /// <param name="exception">Runtime Exception.</param>
        public static void LogException(System.Exception exception)
        {
            if(Config.DebugErrors)
                Debug.LogException(exception);
        }
    }
}
