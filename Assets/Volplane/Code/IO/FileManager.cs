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

namespace Volplane.IO
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class FileManager
    {
        /// <summary>
        /// Generates a file list from a specific directory as JSONArray.
        /// </summary>
        /// <returns>The file list as JSONArray.</returns>
        /// <param name="directoryPath">Path to the directory.</param>
        /// <param name="prefixPath">Optional prefix path to add.</param>
        public static JArray GetFileListFromDirectory(string directoryPath, string prefixPath = "")
        {
            JArray list = new JArray();
            IEnumerable<string> directories;

            directoryPath = Regex.Replace(directoryPath, @"[\\\/]", Path.DirectorySeparatorChar.ToString());

            // Ignore meta files
            directories = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where<string>(name => !name.EndsWith(".meta"));

            // Write all filenames from directory
            // -> Replace backslash '\' to slash '/' for web and unix compatibility
            foreach(string directory in directories)
            {
                list.Add(prefixPath + directory.Replace(directoryPath, "").Replace('\\', '/'));
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

            return WriteJSON(JToken.Parse(sbContent.ToString()), filePath, prettify);
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
        public static string WriteJSON(JToken jsonData, string filePath, bool prettify = true)
        {
            StringBuilder sbContent = new StringBuilder(1024);

            if(prettify)
                sbContent.Insert(0, jsonData.ToString(Newtonsoft.Json.Formatting.Indented));
            else
                sbContent.Insert(0, jsonData.ToString());

            filePath = Regex.Replace(filePath, @"[\\\/]", Path.DirectorySeparatorChar.ToString());

            // Check if path is a directory
            if(Path.GetExtension(filePath) == String.Empty)
            {
                if((jsonData.Type == JTokenType.Object) && (jsonData["name"] == null))
                {
                    if(Config.DebugLog == (int)DebugState.All)
                        VDebug.LogError("[Volplane (FileManager)] Invalid file path. Could not write file.");

                    return null;
                }

                filePath = Path.Combine(filePath, String.Format("{0:G}.json", (string)jsonData["name"]));
            }

            // Write file
            using(StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.NewLine = "\n";
                writer.Write(sbContent);
                writer.WriteLine();
            }

            if((jsonData.Type == JTokenType.Object) && (jsonData["name"] != null))
                return (string)jsonData["name"];

            return null;
        }

        /// <summary>
        /// Reads JSON data from a file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static JToken ReadJSON(string filePath)
        {
            filePath = Regex.Replace(filePath, @"[\\\/]", Path.DirectorySeparatorChar.ToString());

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

            return JToken.Parse(sbContent.ToString());
        }
    }
}
