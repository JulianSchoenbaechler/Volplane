﻿/*
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
    using SimpleJSON;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Volplane.IO;

    [CustomEditor(typeof(VolplaneController))]
    public class VolplaneInspector : Editor
    {
        // Volplane Controller object variables
        private string[] excludedProperties;
        private int tempBrowserStart = (int)BrowserStartMode.Standard;
        private bool tempAutoScaleCanvas = true;

        // Volplane Controller Editor variables
        private string controllerFolderPath, controllerDestinationPath;
        private string[] controllerPaths;
        private string[] controllerList;
        private string tempSelectedController;
		private Action<string> OnControllerCreated;

        /// <summary>
        /// Inspector GUI draw call.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Show version info, license, etc...


            // Draw serialized properties
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, excludedProperties);

            serializedObject.ApplyModifiedProperties();


            // Browser start mode and canvas scale settings
            tempBrowserStart = (int)(BrowserStartMode)EditorGUILayout.EnumPopup("Browser Start", (BrowserStartMode)Config.BrowserStart);
            tempAutoScaleCanvas = EditorGUILayout.Toggle("Auto Scale Canvas", Config.AutoScaleCanvas);

            // Controller list popup
            tempSelectedController = controllerList[EditorGUILayout.Popup("Controller",
                                                                          Array.IndexOf<string>(controllerList, tempSelectedController),
                                                                          controllerList)];

            // Store those settings in editor preferences
            if(tempBrowserStart != Config.BrowserStart)
            {
                Config.BrowserStart = tempBrowserStart;
                EditorPrefs.SetInt("BrowserStart", tempBrowserStart);
            }

            if(tempAutoScaleCanvas != Config.AutoScaleCanvas)
            {
                Config.AutoScaleCanvas = tempAutoScaleCanvas;
                EditorPrefs.SetBool("AutoScaleCanvas", tempAutoScaleCanvas);
            }

            // Current controller changed
            if(tempSelectedController != Config.SelectedController)
            {
				// Reload controller data
				ReloadControllerData();
            }

            // Controller Editor
			// Delete controller
            if(Config.SelectedController != null)
            {
                if(GUILayout.Button("Open Controller Editor"))
                {
                    Application.OpenURL(String.Format("http://localhost:{0:D}/volplane/controller-editor.html?controller={1:G}",
                                                  Config.LocalServerPort,
                                                  Config.SelectedController));
                }

				if(GUILayout.Button("Delete Current Controller"))
				{
					if(EditorUtility.DisplayDialog("Delete Controller",
					                               "Are you sure you want to delete this controller?",
					                               "Yes",
					                               "No"))
					{
						string controllerPath = controllerPaths[Array.IndexOf<string>(controllerList, tempSelectedController) - 1];

						// Delete controller
						if(File.Exists(controllerPath))
							File.Delete(controllerPath);
						
						if(File.Exists(controllerPath + ".meta"))
							File.Delete(controllerPath + ".meta");

						// Reload controller list, image-, font- and controller data
						// Reload controller data
						ReloadControllerList();
						ReloadControllerData();
					}
				}
            }

			// Add new controller
			if(GUILayout.Button("Add New Controller"))
			{
				NewControllerWindow.Init();

				// Subscribe controller created event
				NewControllerWindow.window.ControllerCreated -= OnControllerCreated;
				NewControllerWindow.window.ControllerCreated += OnControllerCreated;
			}
        }


        /// <summary>
        /// On enable.
        /// </summary>
        private void OnEnable()
        {
            excludedProperties = new[] { "m_Script" };
            
            controllerFolderPath = String.Format(
                "{0:G}{1:G}/data/controller",
                Application.dataPath,
                Config.WebServerPath
            );
            controllerDestinationPath = String.Format(
                "{0:G}{1:G}/controller.json",
                Application.dataPath,
                Config.WebTemplatePath
            );
            controllerFolderPath = Regex.Replace(controllerFolderPath, @"[\\\/]", Path.DirectorySeparatorChar.ToString());
            controllerDestinationPath = Regex.Replace(controllerDestinationPath, @"[\\\/]", Path.DirectorySeparatorChar.ToString());

            // Reload controller list, image-, font- and controller data
			// Reload controller data
            ReloadControllerList();
			ReloadControllerData();

			// Set delegate for creating new controller
			OnControllerCreated = delegate(string name) {
				
				// Reload controller list, image-, font- and controller data
				ReloadControllerList();

				// Switch to new controller
				tempSelectedController = name;

				// Reload controller data
				ReloadControllerData();

			};
        }

        /// <summary>
        /// Reloads the controller list, image-, font- and controller data.
        /// </summary>
        private void ReloadControllerList()
        {
            JSONArray jsonControllerList = new JSONArray();

            // Get all controllers
            controllerPaths = Directory.GetFiles(controllerFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            controllerList = new string[controllerPaths.Length + 1];

            controllerList[0] = "None";
            tempSelectedController = "None";

            // Iterate through controllers
            for(int i = 1; i < controllerList.Length; i++)
            {
                controllerList[i] = Path.GetFileNameWithoutExtension(controllerPaths[i - 1]);
                jsonControllerList[-1] = controllerList[i];

                if(Config.SelectedController == controllerList[i])
                    tempSelectedController = controllerList[i];

            }

            // Write image list
            FileManager.WriteJSON(
                FileManager.GetFileListFromDirectory(String.Format("{0:G}{1:G}/img", Application.dataPath, Config.WebTemplatePath), "img"),
                String.Format("{0:G}{1:G}/data/img-list.json", Application.dataPath, Config.WebServerPath)
            );

            // Write font list
            FileManager.WriteJSON(
                FileManager.GetFileListFromDirectory(String.Format("{0:G}{1:G}/fonts", Application.dataPath, Config.WebTemplatePath), "fonts"),
                String.Format("{0:G}{1:G}/data/font-list.json", Application.dataPath, Config.WebServerPath)
            );

            // Write controller list
            FileManager.WriteJSON(
                jsonControllerList,
                String.Format("{0:G}{1:G}/data/controller-list.json", Application.dataPath, Config.WebServerPath)
            );
        }

		/// <summary>
		/// Updates the controller data in the WebGL template.
		/// </summary>
		private void ReloadControllerData()
		{
			// Copy controller to WebGL template
			if(tempSelectedController != "None")
			{
				Config.SelectedController = tempSelectedController;
				EditorPrefs.SetString("SelectedController", tempSelectedController);

                // Compare controller data with existing
                // Copy if newer...
                string controllerPath = controllerPaths[Array.IndexOf<string>(controllerList, tempSelectedController) - 1];
                JSONNode dataNew = FileManager.ReadJSON(controllerPath);
                JSONNode dataOld = FileManager.ReadJSON(controllerDestinationPath);

                if((dataOld == null) || (dataNew["lastEdit"].AsInt > dataOld["lastEdit"].AsInt))
                {
                    // Copy selected controller data into WebGL template
                    File.Copy(controllerPath,
                              controllerDestinationPath,
                              true);
                }
			}
			else
			{
				Config.SelectedController = null;
				EditorPrefs.SetString("SelectedController", null);

				// Delete current controller data from WebGL template
				if(File.Exists(controllerDestinationPath))
					File.Delete(controllerDestinationPath);

				// Delete corresponding meta file
				if(File.Exists(controllerDestinationPath + ".meta"))
					File.Delete(controllerDestinationPath + ".meta");
			}
		}
    }
}
