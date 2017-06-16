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

    [CustomEditor(typeof(VolplaneController))]
    public class VolplaneInspector : Editor
    {
        private string[] excludedProperties;
        private int tempBrowserStart = (int)BrowserStartMode.Standard;
        private bool tempAutoScaleCanvas = true;

        public override void OnInspectorGUI()
        {
            // Show version info, license, etc...

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, excludedProperties);

            serializedObject.ApplyModifiedProperties();

            // Browser start mode and canvas scale settings
            tempBrowserStart = (int)(BrowserStartMode)EditorGUILayout.EnumPopup("Browser Start", (BrowserStartMode)Config.BrowserStart);
            tempAutoScaleCanvas = EditorGUILayout.Toggle("Auto Scale Canvas", Config.AutoScaleCanvas);

            // Temporarily store those settings in editor preferences
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
        }

        private void OnEnable()
        {
            excludedProperties = new[] { "m_Script" };
        }
    }
}
