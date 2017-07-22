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

namespace Volplane.Editor.UI
{
    using UnityEditor;
    using UnityEngine;

    public class ConfigWindow : EditorWindow
    {
        /// <summary>
        /// This window instance.
        /// </summary>
        protected static ConfigWindow window;

        private int tempServerPort;
        private int tempWebsocketPort;
        private int tempDebugLog;

        /// <summary>
        /// Init this instance.
        /// </summary>
        [MenuItem("Window/Volplane Configuration")]
        static void Init()
        {
            Rect position = new Rect(400f, 100f, 400f, 400f);

            ConfigWindow.window = EditorWindow.GetWindowWithRect<ConfigWindow>(position, true, "Volplane Configuration", true);
        }

        /// <summary>
        /// On window enabling.
        /// </summary>
        protected virtual void OnEnable()
        {
            // Format and Styles
        }

        /// <summary>
        /// On GUI draw call.
        /// </summary>
        protected virtual void OnGUI()
        {
            GUILayout.Space(10f);

            tempServerPort = EditorGUILayout.IntField("Local Webserver Port:", Config.LocalServerPort);
            tempWebsocketPort = EditorGUILayout.IntField("Local Websocket Port:", Config.LocalWebsocketPort);
            tempDebugLog = (int)(DebugState)EditorGUILayout.EnumPopup("Debug Messages:", (DebugState)Config.DebugLog);

            GUILayout.Space(40f);

            if(Extensions.LocalWebserver.IsRunning)
            {
                GUILayout.Label("Local webserver is running...");

                if(GUILayout.Button("Stop Server"))
                    Extensions.LocalWebserver.Stop();
            }
            else
            {
                GUILayout.Label("Local webserver has stopped.");

                if(GUILayout.Button("Restart Server"))
                    Extensions.LocalWebserver.Start();
            }

            // Saving edited preferences
            if(tempServerPort != Config.LocalServerPort)
            {
                Config.LocalServerPort = tempServerPort;
                EditorPrefs.SetInt("LocalServerPort", tempServerPort);
            }

            if(tempWebsocketPort != Config.LocalWebsocketPort)
            {
                Config.LocalWebsocketPort = tempWebsocketPort;
                EditorPrefs.SetInt("LocalWebsocketPort", tempWebsocketPort);
            }

            if(tempDebugLog != Config.LocalServerPort)
            {
                Config.DebugLog = tempDebugLog;
                EditorPrefs.SetInt("DebugLog", tempDebugLog);
            }
        }
    }
}
