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

namespace Volplane.IO
{
    using SimpleJSON;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class FileManager
    {
        /// <summary>
        /// Generates a file list from a specific directory as JSONArray.
        /// </summary>
        /// <returns>The file list as JSONArray.</returns>
        /// <param name="directoryPath">Path to the directory.</param>
        /// <param name="prefixPath">Optional prefix path to add.</param>
        public static JSONArray GetFileListFromDirectory(string directoryPath, string prefixPath = "")
        {
            JSONArray list = new JSONArray();
            IEnumerable<string> directories;

            directoryPath = directoryPath.Replace('/', '\\');

            // Ignore meta files
            directories = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where<string>(name => !name.EndsWith(".meta"));

            foreach(string directory in directories)
            {
                list[-1] = prefixPath + directory.Replace(directoryPath, "").Replace('\\', '/');
            }

            return list;
        }

        /// <summary>
        /// Writes a JSON formatted file. The filename can be set by the submitted JSON data.
        /// If the given path leads to a directory instead of a file, the data must contain a 'name' property
        /// which will be used for file creation.
        /// </summary>
        /// <returns>The name property of the JSON data, or null if no name property is specified.</returns>
        /// <param name="inputStream">Input stream.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="prettify">If set to <c>true</c> prettify JSON output.</param>
        public static string WriteJSON(Stream inputStream, string filePath, bool prettify = true)
        {
            StringBuilder sbContent = new StringBuilder(1024);

            // Read file from stream
            using(StreamReader reader = new StreamReader(inputStream, Encoding.UTF8))
            {
                while(!reader.EndOfStream)
                {
                    sbContent.Append(reader.ReadLine());
                }
            }

            return WriteJSON(JSON.Parse(sbContent.ToString()), filePath, prettify);
        }

        /// <summary>
        /// Writes a JSON formatted file. The filename can be set by the submitted JSON data.
        /// If the given path leads to a directory instead of a file, the data must contain a 'name' property
        /// which will be used for file creation.
        /// </summary>
        /// <returns>The name property of the JSON data, or null if no name property is specified.</returns>
        /// <param name="jsonData">JSON data.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="prettify">If set to <c>true</c> prettify JSON output.</param>
        public static string WriteJSON(JSONNode jsonData, string filePath, bool prettify = true)
        {
            StringBuilder sbContent = new StringBuilder(1024);

            if(prettify)
                sbContent.Insert(0, jsonData.ToString(4));
            else
                sbContent.Insert(0, jsonData.ToString());

            // Check if path is a directory
            if(Path.GetExtension(filePath) == String.Empty)
            {
                if(jsonData["name"] == null)
                {
                    if(Config.DebugLog == (int)DebugState.All)
                        UnityEngine.Debug.LogError("[Volplane (FileManager)] Invalid file path. Could not write file.");

                    return null;
                }

                filePath = String.Format("{0:G}/{1:G}.json", filePath, jsonData["name"].Value);
                filePath = filePath.Replace('/', '\\');
            }

            // Write file
            using(StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.Write(sbContent);
            }

            return jsonData["name"] != null ? jsonData["name"].Value : null;
        }

        /// <summary>
        /// Reads JSON data from a file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static JSONNode ReadJSON(string filePath)
        {
            filePath = filePath.Replace('/', '\\');

            if(!File.Exists(filePath))
                return null;
            
            StringBuilder sbContent = new StringBuilder(1024);

            using(FileStream fileStream = File.OpenRead(filePath))
            {
                using(StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    while(!reader.EndOfStream)
                    {
                        sbContent.Append(reader.ReadLine());
                    }
                }
            }

            return JSON.Parse(sbContent.ToString());
        }
    }
}
