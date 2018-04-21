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
    using System;
    using System.IO;
    using System.Text;

    public partial class AirConsoleAgent
    {
        public class Device
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent+Device"/> class.
            /// </summary>
            public Device() : this(256) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent+Device"/> class.
            /// </summary>
            /// <param name="capacity">Data string capacity.</param>
            public Device(int capacity)
            {
                this.CustomData = new StringBuilder(capacity);
            }

            public string Location { get; set; }
            public string Nickname { get; set; }
            public string UID { get; set; }
            public StringBuilder CustomData { get; set; }
            public bool IsLoggedIn { get; set; }
            public bool IsHero { get; set; }
            public bool IsUsingBrowser { get; set; }
            public bool HasSlowConnection { get; set; }

            /// <summary>
            /// Create AirConsole device object directly from a JSON string.
            /// </summary>
            /// <returns>The deserialized device object.</returns>
            /// <param name="json">JSON string representing the data.</param>
            public static Device FromJSON(string json)
            {
                Device deviceObject = new Device();
                Device.PopulateFromJSON(json, deviceObject);

                return deviceObject;
            }

            /// <summary>
            /// Populate AirConsole device object from a JSON string.
            /// </summary>
            /// <param name="json">JSON string representing the data.</param>
            /// <param name="deviceObject">The object to populate.</param>
            public static void PopulateFromJSON(string json, Device deviceObject)
            {
                using(var sr = new StringReader(json))
                using(var reader = new JsonTextReader(sr))
                {
                    // Use buffer
                    reader.ArrayPool = JSONArrayPool.Instance;

                    while(reader.Read())
                    {
                        if(reader.TokenType == JsonToken.PropertyName)
                        {
                            switch(reader.Value.ToString())
                            {
                                case "location":
                                    if(reader.Depth == 1)
                                        deviceObject.Location = reader.ReadAsString();
                                    break;

                                case "uid":
                                    if(reader.Depth == 1)
                                        deviceObject.UID = reader.ReadAsString();
                                    break;

                                case "nickname":
                                    if(reader.Depth == 1)
                                        deviceObject.Nickname = reader.ReadAsString();
                                    break;

                                case "auth":
                                    if(reader.Depth == 1)
                                        deviceObject.IsLoggedIn = reader.ReadAsBoolean() ?? false;
                                    break;

                                case "premium":
                                    if(reader.Depth == 1)
                                        deviceObject.IsHero = reader.ReadAsBoolean() ?? false;
                                    break;

                                case "app":
                                    if(reader.Depth == 2)
                                        deviceObject.IsUsingBrowser = reader.ReadAsString() == "web";
                                    break;

                                case "slow_connection":
                                    if(reader.Depth == 1)
                                        deviceObject.HasSlowConnection = reader.ReadAsBoolean() ?? false;
                                    break;

                                case "custom":
                                    if(reader.Depth != 1)
                                        break;

                                    // Reset StringBuilder
                                    deviceObject.CustomData.Length = 0;

                                    using(var sw = new StringWriter(deviceObject.CustomData))
                                    using(var writer = new JsonTextWriter(sw))
                                    {
                                        // Use buffer
                                        writer.ArrayPool = JSONArrayPool.Instance;

                                        reader.Read();
                                        writer.WriteToken(reader);
                                    }
                                    break;

                                default:
                                    break;
                            }
                        } // if(reader.TokenType == JsonToken.PropertyName)
                    } // while(reader.Read())
                } // using StringReader / JsonTextReader - Dispose
            } // PopulateFromJSON()
        } // Device
    }
}
