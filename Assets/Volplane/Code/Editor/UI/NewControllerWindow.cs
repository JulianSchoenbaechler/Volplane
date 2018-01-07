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
    using Newtonsoft.Json.Linq;
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
		public static NewControllerWindow window;

        private string tempName;
		private string errorString;
        private Regex namingConventions;
        private GUIStyle labelFormat, errorFormat, buttonFormat, inputFormat;

		/// <summary>
		/// Occurs when a new controller was created.
		/// </summary>
		public event Action<string> ControllerCreated;

        /// <summary>
        /// Init this instance.
        /// </summary>
        public static void Init()
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
			tempName = "";
			errorString = "";
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

			// Label error style
			errorFormat = new GUIStyle(labelFormat);
			errorFormat.normal.textColor = Color.red;

            // Input style
            inputFormat = new GUIStyle(GUI.skin.textField);
            inputFormat.alignment = TextAnchor.LowerCenter;
            inputFormat.fontSize = 12;
            inputFormat.fixedHeight = 20;

            // Button style
            buttonFormat = new GUIStyle(GUI.skin.button);
            buttonFormat.alignment = TextAnchor.MiddleCenter;
            buttonFormat.fixedWidth = 100;


			// Name
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Enter a name for this controller:", labelFormat);
            GUILayout.Space(10f);

			// Input
			tempName = EditorGUILayout.TextField(tempName, inputFormat);
            GUILayout.Space(14f);

			// Error
			EditorGUILayout.LabelField(errorString, errorFormat);
			GUILayout.Space(20f);

			// Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

			// Cancel button
            if(GUILayout.Button("Cancel", buttonFormat))
            {
                NewControllerWindow.window.Close();
            }

            GUILayout.FlexibleSpace();

			// Create new controller
            if(GUILayout.Button("OK", buttonFormat))
			{
				// Check length
                if(tempName.Length >= 3)
				{
					// Check for special chars
					if(!namingConventions.Match(tempName).Success)
					{
                        JObject controller = new JObject();
                        controller.Add("name", tempName);
                        controller.Add("views", new JObject());

						FileManager.WriteJSON(controller, String.Format("{0:G}{1:G}/data/controller/{2:G}.json", Application.dataPath, Config.WebServerPath, tempName));

						// Fire controller created event
						if(ControllerCreated != null)
							ControllerCreated(tempName);

						NewControllerWindow.window.Close();
					}
					else
					{
						errorString = "The controller name must not have any special characters\nwith the exception of '-' and '_'.";
					}
				}
				else
				{
					errorString = "The controller name must be at least three characters long.";
				}
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

        }
    }
}
