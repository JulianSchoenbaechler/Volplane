/*
 * Copyright - Julian Schoenbaechler
 * https://github.com/JulianSchoenbaechler/*
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

namespace Volplane.UI
{
    using UnityEditor;
    using UnityEngine;
    using Volplane.Editor;

    public class ConfigWindow : EditorWindow
    {
        /// <summary>
        /// This window instance.
        /// </summary>
        protected static ConfigWindow window;

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

            Config.LocalServerPort = EditorGUILayout.IntField("Local Webserver Port:", Config.LocalServerPort);
            Config.LocalWebsocketPort = EditorGUILayout.IntField("Local Websocket Port:", Config.LocalWebsocketPort);
            Config.DebugLog = (int)(DebugState)EditorGUILayout.EnumPopup("Debug Messages:", (DebugState)Config.DebugLog);

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

            GUILayout.Space(40f);

            if(GUILayout.Button("Save User Config"))
            {
                EditorPrefs.SetInt("LocalServerPort", Config.LocalServerPort);
                EditorPrefs.SetInt("LocalWebsocketPort", Config.LocalWebsocketPort);
                EditorPrefs.SetInt("DebugLog", Config.DebugLog);
            }
        }
    }
}
