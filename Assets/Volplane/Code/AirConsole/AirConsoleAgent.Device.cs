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

    public partial class AirConsoleAgent
    {
        protected class Device
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent+Device"/> class.
            /// </summary>
            public Device() { }

            public string Location { get; set; }
            public string Nickname { get; set; }
            public string UID { get; set; }
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
                    string currentProperty = String.Empty;
                    int currentDepth = 0;

                    while(reader.Read())
                    {
                        switch(reader.TokenType)
                        {
                            case JsonToken.PropertyName:
                                
                                currentProperty = reader.Value.ToString();
                                currentDepth = reader.Depth;
                                break;

                            case JsonToken.String:

                                if(currentDepth != 1)
                                    break;
                                
                                if(currentProperty == "location")
                                {
                                    deviceObject.Location = reader.Value.ToString();
                                }
                                else if(currentProperty == "uid")
                                {
                                    deviceObject.UID = reader.Value.ToString();
                                }
                                else if(currentProperty == "nickname")
                                {
                                    deviceObject.Nickname = reader.Value.ToString();
                                }

                                break;

                            case JsonToken.Boolean:

                                if((currentDepth != 1) && (currentProperty == "auth"))
                                {
                                    deviceObject.IsLoggedIn = (bool)reader.Value;
                                }
                                else if((currentDepth != 1) && (currentProperty == "premium"))
                                {
                                    deviceObject.IsHero = (bool)reader.Value;
                                }
                                else if((currentDepth != 2) && (currentProperty == "app"))
                                {
                                    deviceObject.IsUsingBrowser = reader.Value.ToString() == "web";
                                }
                                else if((currentDepth != 1) && (currentProperty == "slow_connection"))
                                {
                                    deviceObject.HasSlowConnection = (bool)reader.Value;
                                }

                                break;
                        }
                    } // while(reader.Read())
                } // using StringReader / JsonTextReader - Dispose
            } // PopulateFromJSON()
        } // Device
    }
}
