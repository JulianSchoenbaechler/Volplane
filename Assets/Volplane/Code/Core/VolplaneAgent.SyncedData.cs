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

namespace Volplane
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public partial class VolplaneAgent
    {
        protected class SyncedData
        {
            private StringBuilder dataBuilder;

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VolplaneAgent+SyncedData"/> class.
            /// </summary>
            public SyncedData() : this(256) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.VolplaneAgent+SyncedData"/> class.
            /// </summary>
            /// <param name="capacity">Capacity of the data container.</param>
            public SyncedData(int capacity)
            {
                this.dataBuilder = new StringBuilder(capacity);

                this.Views = new List<string>(8);
                this.Active = new List<bool>(8);
            }

            public List<string> Views { get; set; }
            public List<bool> Active { get; set; }

            /// <summary>
            /// Create SyncedData object directly from a JSON string.
            /// </summary>
            /// <returns>The deserialized data packet.</returns>
            /// <param name="json">JSON string representing the data.</param>
            public static SyncedData FromJSON(string json)
            {
                SyncedData data = new SyncedData();
                SyncedData.PopulateFromJSON(json, data);

                return data;
            }

            /// <summary>
            /// Populate SyncedData object from a JSON string.
            /// </summary>
            /// <param name="json">JSON string representing the data.</param>
            /// <param name="dataObject">The object to populate.</param>
            public static void PopulateFromJSON(string json, SyncedData dataObject)
            {
                string currentProperty = String.Empty;
                int i = 0;

                using(var sr = new StringReader(json))
                using(var reader = new JsonTextReader(sr))
                {
                    while(reader.Read())
                    {
                        if(reader.TokenType == JsonToken.PropertyName)
                        {
                            if(reader.Depth == 1)
                            {
                                currentProperty = reader.Value.ToString();
                            }
                            else if((reader.Depth == 2) && (currentProperty == "volplane"))
                            {
                                // Check nested properties under "volplane"
                                switch(reader.Value.ToString())
                                {
                                    // Read "active" array
                                    case "active":
                                        while(reader.Read() && (reader.TokenType != JsonToken.EndArray))
                                        {
                                            if(reader.TokenType == JsonToken.StartArray)
                                            {
                                                i = 0;
                                            }
                                            else
                                            {
                                                if(dataObject.Active.Count > i)
                                                    dataObject.Active[i] = Boolean.Parse(reader.Value.ToString());
                                                else
                                                    dataObject.Active.Add(Boolean.Parse(reader.Value.ToString()));

                                                i++;
                                            }
                                        }
                                        break;

                                        // Read "views" array
                                    case "views":
                                        while(reader.Read() && (reader.TokenType != JsonToken.EndArray))
                                        {
                                            if(reader.TokenType == JsonToken.StartArray)
                                            {
                                                i = 0;
                                            }
                                            else
                                            {
                                                if(dataObject.Views.Count > i)
                                                    dataObject.Views[i] = reader.Value.ToString();
                                                else
                                                    dataObject.Views.Add(reader.Value.ToString());

                                                i++;
                                            }
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        } // reader.TokenType == JsonToken.PropertyName
                    } // while(reader.Read())
                } // using StringReader / JsonTextReader - Dispose
            } // PopulateFromJSON()

            /// <summary>
            /// Parse this object data to a JSON formatted string
            /// (nested into a "volplane" property).
            /// </summary>
            /// <returns>The JSON string.</returns>
            public string ToJSON()
            {
                dataBuilder.Length = 0;

                using(var sw = new StringWriter(dataBuilder))
                using(var writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("volplane");
                    writer.WriteStartObject();

                    if(Active.Count > 0)
                    {
                        writer.WritePropertyName("active");
                        writer.WriteStartArray();

                        for(int i = 0; i < Active.Count; i++)
                        {
                            writer.WriteValue(Active[i]);
                        }

                        writer.WriteEndArray();
                    }

                    if(Views.Count > 0)
                    {
                        writer.WritePropertyName("views");
                        writer.WriteStartArray();

                        for(int i = 0; i < Views.Count; i++)
                        {
                            writer.WriteValue(Views[i]);
                        }

                        writer.WriteEndArray();
                    }

                    writer.WriteEndObject();
                    writer.WriteEnd();
                }

                return dataBuilder.ToString();
            } // FromJSON()
        } // SyncedData
    }
}
