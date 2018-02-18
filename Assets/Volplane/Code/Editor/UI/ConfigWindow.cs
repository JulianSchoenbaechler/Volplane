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
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class ConfigWindow : EditorWindow
    {
        /// <summary>
        /// This window instance.
        /// </summary>
        protected static ConfigWindow window;

        /// <summary>
        /// All loaded IPv4 addresses from this machines network interface.
        /// </summary>
        private static IList<string> LocalIPv4Adresses;

        private int tempSelectedIP;
        private string tempIPv4;
        private int tempServerPort;
        private int tempWebsocketPort;
        private int tempDebugLog;
        private bool tempDebugMessages, tempDebugWarnings, tempDebugErrors;

        private GUIStyle redStyle, greenStyle;

        /// <summary>
        /// Init this instance.
        /// </summary>
        [MenuItem("Window/Volplane Configuration")]
        static void Init()
        {
            Rect position = new Rect(400f, 100f, 400f, 240f);

            ConfigWindow.window = EditorWindow.GetWindowWithRect<ConfigWindow>(position, true, "Volplane Configuration", true);
            ConfigWindow.LocalIPv4Adresses = Extensions.GetLocalIPAddresses() as IList<string>;
        }

        /// <summary>
        /// On GUI draw call.
        /// </summary>
        protected virtual void OnGUI()
        {
            // Format and Styles
            if((redStyle == null) || (greenStyle == null))
            {
                redStyle = new GUIStyle(GUI.skin.label);
                greenStyle = new GUIStyle(GUI.skin.label);
                redStyle.normal.textColor = new Color(0.6f, 0f, 0f);
                greenStyle.normal.textColor = new Color(0f, 0.6f, 0f);
            }

            // Rendering window
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10f);

            if(ConfigWindow.LocalIPv4Adresses != null)
            {
                if(ConfigWindow.LocalIPv4Adresses.Count > 0)
                {
                    tempSelectedIP = ConfigWindow.LocalIPv4Adresses.IndexOf(Config.LocalIPv4);
                    tempSelectedIP = EditorGUILayout.Popup("Local IPv4 Address:", Mathf.Max(tempSelectedIP, 0), ConfigWindow.LocalIPv4Adresses.ToArray());
                }
                else
                {
                    tempIPv4 = EditorGUILayout.TextField("Local IPv4 Address:", Config.LocalIPv4);
                }
            }

            tempServerPort = EditorGUILayout.IntField("Local Webserver Port:", Config.LocalServerPort);
            tempWebsocketPort = EditorGUILayout.IntField("Local Websocket Port:", Config.LocalWebsocketPort);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            tempDebugLog = (int)(DebugState)EditorGUILayout.EnumPopup("Debug:", (DebugState)Config.DebugLog);
            tempDebugMessages = EditorGUILayout.Toggle("Debug Messages", Config.DebugMessages);
            tempDebugWarnings = EditorGUILayout.Toggle("Debug Warnings", Config.DebugWarnings);
            tempDebugErrors = EditorGUILayout.Toggle("Debug Errors", Config.DebugErrors);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if(Extensions.LocalWebserver.IsRunning)
            {
                GUILayout.Label("Local webserver is running...", greenStyle);

                if(GUILayout.Button("Stop Server"))
                    Extensions.LocalWebserver.Stop();
            }
            else
            {
                GUILayout.Label("Local webserver has stopped.", redStyle);

                if(GUILayout.Button("Restart Server"))
                    Extensions.LocalWebserver.Start();
            }

            EditorGUILayout.EndHorizontal();

            // No changes made?
            if(!EditorGUI.EndChangeCheck())
                return;

            // Saving edited preferences
            if(ConfigWindow.LocalIPv4Adresses.Count > tempSelectedIP)
            {
                if(ConfigWindow.LocalIPv4Adresses[tempSelectedIP] != Config.LocalIPv4)
                {
                    Config.LocalIPv4 = ConfigWindow.LocalIPv4Adresses[tempSelectedIP];
                    EditorPrefs.SetInt("SelectedLocalIPv4", tempSelectedIP);
                }
            }
            else
            {
                Config.LocalIPv4 = tempIPv4;
                EditorPrefs.SetString("LocalIPv4", tempIPv4);
            }

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

            if(tempDebugMessages != Config.DebugMessages)
            {
                Config.DebugMessages = tempDebugMessages;
                EditorPrefs.SetBool("DebugMessages", tempDebugMessages);
            }

            if(tempDebugWarnings != Config.DebugWarnings)
            {
                Config.DebugWarnings = tempDebugWarnings;
                EditorPrefs.SetBool("DebugWarnings", tempDebugWarnings);
            }

            if(tempDebugErrors != Config.DebugErrors)
            {
                Config.DebugErrors = tempDebugErrors;
                EditorPrefs.SetBool("DebugErrors", tempDebugErrors);
            }
        }
    }
}
