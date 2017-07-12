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
    using System.Collections;
    using System.IO;
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
            string[] directories = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            for(int i = 0; i < directories.Length; i++)
            {
                list[i] = prefixPath + directories[i].Replace(directoryPath, "");
            }

            return list;
        }

        /// <summary>
        /// Writes a JSON formatted file. The filename is set by the submitted JSON data.
        /// The data must contain a 'name' property.
        /// </summary>
        /// <param name="inputStream">Input stream.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="prettify">If set to <c>true</c> prettify JSON output.</param>
        public static void WriteJSON(Stream inputStream, string filePath, bool prettify = true)
        {
            StringBuilder sbContent = new StringBuilder(65535);
            JSONNode json;

            // Read file from stream
            using(StreamReader reader = new StreamReader(inputStream, Encoding.UTF8))
            {
                while(!reader.EndOfStream)
                {
                    sbContent.Append(reader.ReadLine());
                }
            }

            json = JSON.Parse(sbContent.ToString());
            sbContent.Length = 0;
            sbContent.Capacity = 65535;

            if(prettify)
                sbContent.Insert(0, json.ToString(4));
            else
                sbContent.Insert(0, json.ToString());

            // Write file
            using(StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.Write(sbContent);
            }
        }
    }
}
