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

namespace Volplane.Editor.Build
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;

    public class PostProcessor
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if(target != BuildTarget.WebGL)
                return;

            // Delete existing 'screen.html' (previous build)
            if(File.Exists(path + "/screen.html"))
                File.Delete(path + "/screen.html");

            // Rename 'index.html' to 'screen.hmtl'
            File.Move(path + "/index.html", path + "/screen.html");

            #if UNITY_5_6_OR_NEWER

            // Delete existing 'game.json' (previous build)
            if(File.Exists(path + "/Build/game.json"))
                File.Delete(path + "/Build/game.json");

            // Rename '<projectName>.json' to 'game.json'
            File.Move(
                String.Format("{0:G}/Build/{1:G}.json", path, Path.GetFileName(path)),
                path + "/Build/game.json"
            );

            #endif

            // Save build path in preferences
            EditorPrefs.SetString("BuildPath", path);
            Config.BuildPath = path;

            VDebug.Log("[Volplane] Completed build!");
        }
    }
}
