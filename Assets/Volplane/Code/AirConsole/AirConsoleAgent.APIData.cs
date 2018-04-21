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

namespace Volplane.AirConsole
{
    using Newtonsoft.Json;
    using System.IO;
    using System.Text;

    public partial class AirConsoleAgent
    {
        /// <summary>
        /// AirConsole API data structure.
        /// </summary>
        private class APIData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent+APIData"/> class.
            /// </summary>
            public APIData() : this(256) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent+APIData"/> class.
            /// </summary>
            /// <param name="dataCapacity">Data string capacity.</param>
            public APIData(int dataCapacity)
            {
                this.Data = new StringBuilder(dataCapacity);
            }

            public string Action { get; set; }
            public int? DeviceId { get; set; }
            public bool? StateData { get; set; }
            public string StringData { get; set; }
            public StringBuilder Data { get; set; }
            public bool AssignedData { get; private set; }

            /// <summary>
            /// Create APIData object directly from a JSON string.
            /// </summary>
            /// <returns>The deserialized data packet.</returns>
            /// <param name="json">JSON string representing the data.</param>
            public static APIData FromJSON(string json)
            {
                APIData data = new APIData();
                APIData.PopulateFromJSON(json, data);

                return data;
            }

            /// <summary>
            /// Populate APIData object from a JSON string.
            /// </summary>
            /// <param name="json">JSON string representing the data.</param>
            /// <param name="dataObject">The object to populate.</param>
            public static void PopulateFromJSON(string json, APIData dataObject)
            {
                dataObject.AssignedData = false;

                using(var sr = new StringReader(json))
                using(var reader = new JsonTextReader(sr))
                {
                    // Use buffer
                    reader.ArrayPool = JSONArrayPool.Instance;

                    while(reader.Read())
                    {
                        if((reader.TokenType == JsonToken.PropertyName) && (reader.Depth == 1))
                        {
                            switch(reader.Value.ToString())
                            {
                                case "action":
                                    dataObject.Action = reader.ReadAsString();
                                    break;

                                case "device_id":
                                case "from":
                                    dataObject.DeviceId = reader.ReadAsInt32();
                                    break;

                                case "uid":
                                    dataObject.StringData = reader.ReadAsString();
                                    break;

                                case "ad_was_shown":
                                    dataObject.StateData = reader.ReadAsBoolean();
                                    break;

                                default:
                                    // Reset StringBuilder
                                    dataObject.Data.Length = 0;

                                    using(var sw = new StringWriter(dataObject.Data))
                                    using(var writer = new JsonTextWriter(sw))
                                    {
                                        // Use buffer
                                        writer.ArrayPool = JSONArrayPool.Instance;

                                        reader.Read();
                                        writer.WriteToken(reader);
                                    }

                                    dataObject.AssignedData = true;
                                    break;
                            }
                        }
                    } // while(reader.Read())
                } // using StringReader / JsonTextReader - Dispose
            } // PopulateFromJSON()
        } // APIData
    }
}
