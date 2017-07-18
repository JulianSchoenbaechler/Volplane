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

namespace Volplane.Editor.UI
{
    using SimpleJSON;
    using System;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Volplane.IO;

    public class NewControllerWindow : EditorWindow
    {
        /// <summary>
        /// This window instance.
        /// </summary>
        protected static NewControllerWindow window;

        private string tempName;
        private Regex namingConventions;
        private GUIStyle labelFormat, buttonFormat, inputFormat;

        /// <summary>
        /// Init this instance.
        /// </summary>
        [MenuItem("Window/Popup Volplane")]
        static void Init()
        {
            Rect position = new Rect(400f, 400f, 400f, 140f);
            NewControllerWindow.window = EditorWindow.GetWindowWithRect<NewControllerWindow>(position, true, "Create New Controller", true);
        }

        /// <summary>
        /// On window enabling.
        /// </summary>
        protected virtual void OnEnable()
        {
            namingConventions = new Regex(@"([^a-zA-Z0-9_-]+)");
        }

        /// <summary>
        /// On GUI draw call.
        /// </summary>
        protected virtual void OnGUI()
        {
            // Label style
            labelFormat = new GUIStyle();
            labelFormat.alignment = TextAnchor.MiddleCenter;
            labelFormat.fontSize = 12;

            // Input style
            inputFormat = new GUIStyle(GUI.skin.textField);
            inputFormat.alignment = TextAnchor.LowerCenter;
            inputFormat.fontSize = 12;
            inputFormat.fixedHeight = 20;

            // Button style
            buttonFormat = new GUIStyle(GUI.skin.button);
            buttonFormat.alignment = TextAnchor.MiddleCenter;
            buttonFormat.fixedWidth = 100;



            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Enter a name for this controller:", labelFormat);
            GUILayout.Space(10f);
            tempName = EditorGUILayout.TextField("", inputFormat);
            GUILayout.Space(20f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if(GUILayout.Button("Cancel", buttonFormat))
            {
                NewControllerWindow.window.Close();
            }

            GUILayout.FlexibleSpace();

            if(GUILayout.Button("OK", buttonFormat))
            {
                if(tempName.Length >= 3)
                {
                    if(!namingConventions.Match(tempName).Success)
                    {
                        JSONNode controller = new JSONObject();
                        controller["name"] = tempName;

                        FileManager.WriteJSON(controller, String.Format("{0:G}{1:G}/data/controller/{1:G}.json", Application.dataPath, Config.WebServerPath, tempName));
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            /*
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
            */
        }
    }
}
