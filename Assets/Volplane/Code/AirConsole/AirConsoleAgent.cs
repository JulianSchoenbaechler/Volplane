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
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public partial class AirConsoleAgent : IDisposable
    {
        protected VolplaneController controllerSingleton;
        protected bool isConnectionReady = false;
        protected IDictionary<int, Device> acDevices;
        protected IList<int> acPlayerNumbers;
        protected int acServerTimeOffset;
        protected string acGameLocation;
        protected StringBuilder sendData;

        private APIData currentData;
        private DateTime epochTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent"/> class.
        /// </summary>
        /// <param name="singleton">Reference to the Unity singleton VolplaneController object.</param>
        public AirConsoleAgent(VolplaneController singleton)
        {
            this.controllerSingleton = singleton;
            this.acDevices = new Dictionary<int, Device>();
            this.acPlayerNumbers = new List<int>();
            this.epochTime = new DateTime(1970, 1, 1);
            this.currentData = new APIData(2048);
            this.sendData = new StringBuilder(512);
        }

        #region AirConsole Events

        /// <summary>
        /// AirConsole API: onConnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int> OnConnect;

        /// <summary>
        /// AirConsole API: onDisconnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int> OnDisconnect;

        /// <summary>
        /// AirConsole API: onReady callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<string> OnReady;

        /// <summary>
        /// AirConsole API: onMessage callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int, string> OnMessage;

        /// <summary>
        /// AirConsole API: onDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int, Device> OnDeviceStateChange;

        /// <summary>
        /// AirConsole API: onCustomDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int, string> OnCustomDeviceStateChange;

        /// <summary>
        /// AirConsole API: onDeviceProfileChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int> OnDeviceProfileChange;

        /// <summary>
        /// AirConsole API: onAdShow callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action OnAdShow;

        /// <summary>
        /// AirConsole API: onAdComplete callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<bool> OnAdComplete;

        /// <summary>
        /// AirConsole API: onPremium callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int> OnPremium;

        /// <summary>
        /// AirConsole API: onConnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<string> OnPersistentDataLoaded;

        /// <summary>
        /// AirConsole API: onPersistentDataStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<string> OnPersistentDataStored;

        /// <summary>
        /// AirConsole API: onHighScores callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<string> OnHighScores;

        /// <summary>
        /// AirConsole API: onHighScoreStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<string> OnHighScoreStored;

        #endregion

        /// <summary>
        /// Processes the data send from AirConsole API.
        /// </summary>
        /// <param name="data">The received data formatted as JSON string.</param>
        public void ProcessData(string data)
        {
            // Reading task(s) from JSON
            APIData.PopulateFromJSON(data, currentData);

            // Calling task(s)
            switch(currentData.Action)
            {
                case "onConnect":
                    OnConnectInternal();
                    break;

                case "onDisconnect":
                    OnDisconnectInternal();
                    break;

                case "onReady":
                    OnReadyInternal();
                    break;

                case "onMessage":
                    OnMessageInternal();
                    break;

                case "onDeviceStateChange":
                    OnDeviceStateChangeInternal();
                    break;

                case "onCustomDeviceStateChange":
                    OnCustomDeviceStateChangeInternal();
                    break;

                case "onDeviceProfileChange":
                    OnDeviceProfileChangeInternal();
                    break;

                case "onAdShow":
                    OnAdShowInternal();
                    break;

                case "onAdComplete":
                    OnAdCompleteInternal();
                    break;

                case "onPremium":
                    OnPremiumInternal();
                    break;

                case "onPersistentDataLoaded":
                    OnPersistentDataLoadedInternal();
                    break;

                case "onPersistentDataStored":
                    OnPersistentDataStoredInternal();
                    break;

                case "onHighScores":
                    OnHighScoresInternal();
                    break;

                case "onHighScoreStored":
                    OnHighScoreStoredInternal();
                    break;

                default:
                    break;
            }
        }

        #region AirConsole Public Methods

        /// <summary>
        /// AirConsole API: message function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceIdReceiver">AirConsole device identifier receiver.</param>
        /// <param name="data">Data in JSON format.</param>
        public void Message(int acDeviceIdReceiver, string data)
        {
            if(!IsConnectionReady("message()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("message");
                writer.WritePropertyName("to");
                writer.WriteValue(acDeviceIdReceiver);
                writer.WritePropertyName("data");
                writer.WriteRawValue(data);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: broadcast function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="data">Data in JSON format.</param>
        public void Broadcast(string data)
        {
            if(!IsConnectionReady("broadcast()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("broadcast");
                writer.WritePropertyName("data");
                writer.WriteRawValue(data);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: getControllerDeviceIds function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The controller device identifiers.</returns>
        public ICollection<int> GetControllerDeviceIds()
        {
            if(!IsConnectionReady("getControllerDeviceIds()"))
                return null;

            ICollection<int> controllerIds = acDevices.Keys
                .Where(id => !(
                    (id == 0) ||
                    (acDevices[id] == null) ||
                    (FormatGameUrl(acDevices[id].Location) != acGameLocation)
                ))
                .ToList() as ICollection<int>;

            return controllerIds;
        }

        /// <summary>
        /// AirConsole API: getMasterControllerDeviceId function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The device identifier of the master controller.</returns>
        public int GetMasterControllerDeviceId()
        {
            if(!IsConnectionReady("getMasterControllerDeviceId()"))
                return -1;

            ICollection<int> premiumIds = GetPremiumDeviceIds();

            if(premiumIds.Count > 0)
            {
                return premiumIds.ElementAt(0);
            }
            else
            {
                ICollection<int> controllerIds = GetControllerDeviceIds();

                if(controllerIds.Count > 0)
                    return controllerIds.ElementAt(0);
            }

            return -1;
        }

        /// <summary>
        /// AirConsole API: getServerTime function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The server time.</returns>
        public long GetServerTime()
        {
            if(!IsConnectionReady("getServerTime()"))
                return -1;

            return (long)DateTime.UtcNow.Subtract(epochTime).TotalMilliseconds + acServerTimeOffset;
        }

        /// <summary>
        /// AirConsole API: getCustomDeviceState function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The JSON formatted custom device state data.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public string GetCustomDeviceState(int acDeviceId = 0)
        {
            if(!IsConnectionReady("getCustomDeviceState()"))
                return null;

            if(acDevices.ContainsKey(acDeviceId))
            {
                return acDevices[acDeviceId].CustomData.ToString();
            }
            else
            {
                Debug.LogErrorFormat("[Volplane (AirConsole Agent)] No device with device id {0:D} connected.", acDeviceId);
                return null;
            }
        }

        /// <summary>
        /// AirConsole API: setCustomDeviceState function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="data">Custom device state data as JSON string.</param>
        public void SetCustomDeviceState(string data)
        {
            if(!IsConnectionReady("setCustomDeviceState()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("setCustomDeviceState");
                writer.WritePropertyName("data");
                writer.WriteRawValue(data);
                writer.WriteEnd();
            }

            if(acDevices.Count > 0)
            {
                acDevices[0].CustomData.Length = 0;
                acDevices[0].CustomData.Append(data);
            }
            else
            {
                Device screenDevice = new Device();
                screenDevice.CustomData.Append(data);
                acDevices.Add(0, screenDevice);
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: setCustomDeviceStateProperty function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value as JSON data string.</param>
        public void SetCustomDeviceStateProperty(string key, string value)
        {
            if(!IsConnectionReady("setCustomDeviceStateProperty()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("setCustomDeviceStateProperty");
                writer.WritePropertyName("key");
                writer.WriteValue(key);
                writer.WritePropertyName("value");
                writer.WriteRawValue(value);
                writer.WriteEnd();
            }

            // Currently changing a specific property of custom data by parsing current data
            // through LINQ and serialize again. This is not optimized yet, but won't
            // be used by Volplane.
            JObject parsedData;

            if(acDevices.Count > 0)
            {
                if(acDevices[0].CustomData.Length > 0)
                {
                    parsedData = JObject.Parse(acDevices[0].CustomData.ToString());

                    if(parsedData["custom"] == null)
                        parsedData.Add("custom", new JObject());

                    if(parsedData["custom"][key] == null)
                        (parsedData["custom"] as JObject).Add("key", JToken.Parse(value));
                    else
                        parsedData["custom"][key] = JToken.Parse(value);
                }
                else
                {
                    parsedData = new JObject();
                    parsedData.Add("custom", new JObject {
                        { key, JToken.Parse(value) }
                    });
                }

                acDevices[0].CustomData.Length = 0;
                acDevices[0].CustomData.Append(parsedData.ToString());
            }
            else
            {
                parsedData = new JObject();
                parsedData.Add("custom", new JObject {
                    { key, JToken.Parse(value) }
                });

                Device screenDevice = new Device();
                screenDevice.CustomData.Append(parsedData.ToString());
                acDevices.Add(0, screenDevice);
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: getNickname function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The users nickname.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public string GetNickname(int acDeviceId = 0)
        {
            if(!IsConnectionReady("getNickname()"))
                return null;

            if(acDevices.ContainsKey(acDeviceId))
            {
                if((acDevices[acDeviceId].Nickname != null) && (acDevices[acDeviceId].Nickname.Length > 0))
                    return acDevices[acDeviceId].Nickname;
                else
                    return String.Format("Guest {0:D}", acDeviceId);
            }
            else
            {
                Debug.LogErrorFormat("[Volplane (AirConsole Agent)] No device with device id {0:D} connected.", acDeviceId);
                return null;
            }
        }

        /// <summary>
        /// AirConsole API: getProfilePicture function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The url to the players profile picture.</returns>
        /// <param name="acUid">User id.</param>
        /// <param name="size">Profile picture size.</param>
        public string GetProfilePicture(string acUid = "", int size = 64)
        {
            if(!IsConnectionReady("getProfilePicture()"))
                return null;

            return String.Format("{0:G}{1:G}&size={2:D}", Config.AirConsoleProfilePictureUrl, acUid, size);
        }

        /// <summary>
        /// AirConsole API: getUID function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The AirConsole user id.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public string GetUID(int acDeviceId = 0)
        {
            if(!IsConnectionReady("getUID()"))
                return null;

            if(acDevices.ContainsKey(acDeviceId))
            {
                return acDevices[acDeviceId].UID;
            }
            else
            {
                Debug.LogErrorFormat("[Volplane (AirConsole Agent)] No device with device id {0:D} connected.", acDeviceId);
                return null;
            }
        }

        /// <summary>
        /// AirConsole API: isUserLoggedIn function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns><c>true</c> if this user is logged in; otherwise, <c>false</c>.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public bool IsUserLoggedIn(int acDeviceId = 0)
        {
            if(!IsConnectionReady("isUserLoggedIn()"))
                return false;

            if(acDevices.ContainsKey(acDeviceId))
                return acDevices[acDeviceId].IsLoggedIn;

            return false;
        }

        /// <summary>
        /// AirConsole API: convertDeviceIdToPlayerNumber function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The specific player number.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public int ConvertDeviceIdToPlayerNumber(int acDeviceId = 0)
        {
            if(!IsConnectionReady("convertDeviceIdToPlayerNumber()"))
                return -1;

            return acPlayerNumbers.IndexOf(acDeviceId);
        }

        /// <summary>
        /// AirConsole API: convertPlayerNumberToDeviceId function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The specific device identifier.</returns>
        /// <param name="acPlayerNumber">AirConsole player number.</param>
        public int ConvertPlayerNumberToDeviceId(int acPlayerNumber = -1)
        {
            if(!IsConnectionReady("convertPlayerNumberToDeviceId()"))
                return -1;

            if((acPlayerNumber >= 0) && (acPlayerNumber < acPlayerNumbers.Count))
                return acPlayerNumbers[acPlayerNumber];

            return -1;
        }

        /// <summary>
        /// AirConsole API: getActivePlayerDeviceIds function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The active player device identifiers.</returns>
        public ICollection<int> GetActivePlayerDeviceIds()
        {
            return acPlayerNumbers as ICollection<int>;
        }

        /// <summary>
        /// AirConsole API: setActivePlayers function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="maxPlayers">Maximum amount of players.</param>
        public void SetActivePlayers(int maxPlayers)
        {
            if(!IsConnectionReady("setActivePlayers()"))
                return;

            ICollection<int> controllerIds = GetControllerDeviceIds();
            int i = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("setActivePlayers");
                writer.WritePropertyName("max_players");
                writer.WriteValue(maxPlayers);
                writer.WriteEnd();
            }

            acPlayerNumbers.Clear();

            foreach(int id in controllerIds)
            {
                if(i < maxPlayers)
                    acPlayerNumbers.Add(id);

                i++;
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: showAd function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public void ShowAd()
        {
            if(!IsConnectionReady("showAd()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("showAd");
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: getPremiumDeviceIds function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns>The premium device identifiers.</returns>
        public ICollection<int> GetPremiumDeviceIds()
        {
            if(!IsConnectionReady("getPremiumDeviceIds()"))
                return null;

            HashSet<int> controllerIds = new HashSet<int>(GetControllerDeviceIds());
            controllerIds.RemoveWhere(id => !IsPremium(id));

            return controllerIds as ICollection<int>;
        }

        /// <summary>
        /// AirConsole API: isPremium function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <returns><c>true</c> if this device is marked as premium; otherwise, <c>false</c>.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public bool IsPremium(int acDeviceId = 0)
        {
            if(!IsConnectionReady("isPremium()"))
                return false;

            if(acDevices.ContainsKey(acDeviceId))
                return acDevices[acDeviceId].IsHero;

            return false;
        }

        /// <summary>
        /// AirConsole API: storePersistentData function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value data as JSON string.</param>
        /// <param name="uid">User id.</param>
        public void StorePersistentData(string key, string value, string uid = null)
        {
            if(!IsConnectionReady("storePersistentData()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("storePersistentData");
                writer.WritePropertyName("key");
                writer.WriteValue(key);
                writer.WritePropertyName("value");
                writer.WriteRawValue(value);
                writer.WritePropertyName("uid");
                writer.WriteValue(uid ?? GetUID());
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: requestPersistentData function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="uids">User ids.</param>
        public void RequestPersistentData(ICollection<string> uids = null)
        {
            if(!IsConnectionReady("requestPersistentData()"))
                return;

            if(uids == null)
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("requestPersistentData");
                writer.WritePropertyName("uids");
                writer.WriteStartArray();

                foreach(string uid in uids)
                    writer.WriteValue(uid);

                writer.WriteEndArray();
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: requestPersistentData function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="uid">User id.</param>
        public void RequestPersistentData(string uid = null)
        {
            if(!IsConnectionReady("requestPersistentData()"))
                return;

            RequestPersistentData(new string[] { uid });
        }

        /// <summary>
        /// AirConsole API: storeHighScore function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="levelVersion">Level version.</param>
        /// <param name="score">Score.</param>
        /// <param name="uids">User ids.</param>
        /// <param name="data">Highscore data as JSON string.</param>
        /// <param name="scoreString">Human readable score.</param>
        public void StoreHighScore(string levelName,
                                   string levelVersion,
                                   float score,
                                   ICollection<string> uids = null,
                                   string data = null,
                                   string scoreString = null)
        {
            if(!IsConnectionReady("storeHighScore()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("storeHighScore");
                writer.WritePropertyName("level_name");
                writer.WriteValue(levelName);
                writer.WritePropertyName("level_version");
                writer.WriteValue(levelVersion);
                writer.WritePropertyName("score");
                writer.WriteValue(score);
                writer.WritePropertyName("uid");
                writer.WriteStartArray();

                foreach(string uid in uids)
                    writer.WriteValue(uid);

                writer.WriteEndArray();
                writer.WritePropertyName("data");
                writer.WriteRawValue(data);
                writer.WritePropertyName("score_string");
                writer.WriteValue(scoreString);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: storeHighScore function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="levelVersion">Level version.</param>
        /// <param name="score">Score.</param>
        /// <param name="uid">User id.</param>
        /// <param name="data">Highscore data as JSON string.</param>
        /// <param name="scoreString">Human readable score.</param>
        public void StoreHighScore(string levelName,
                                   string levelVersion,
                                   float score,
                                   string uid = null,
                                   string data = null,
                                   string scoreString = null)
        {
            if(!IsConnectionReady("storeHighScore()"))
                return;

            StoreHighScore(levelName, levelVersion, score, new string[] { uid }, data, scoreString);
        }

        /// <summary>
        /// AirConsole API: requestHighScores function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="levelVersion">Level version.</param>
        /// <param name="uids">User ids.</param>
        /// <param name="ranks">High score rank types.</param>
        /// <param name="total">Amount of high scores per rank type.</param>
        /// <param name="top">Amount of top high scores per rank type.</param>
        public void RequestHighScores(string levelName,
                                     string levelVersion,
                                     ICollection<string> uids = null,
                                     ICollection<string> ranks = null,
                                     int total = -1,
                                     int top = -1)
        {
            if(!IsConnectionReady("requestHighScores()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("requestHighScores");
                writer.WritePropertyName("level_name");
                writer.WriteValue(levelName);
                writer.WritePropertyName("level_version");
                writer.WriteValue(levelVersion);
                writer.WritePropertyName("uid");
                writer.WriteStartArray();

                foreach(string uid in uids)
                    writer.WriteValue(uid);

                writer.WriteEndArray();
                writer.WritePropertyName("ranks");
                writer.WriteStartArray();

                foreach(string rank in ranks)
                    writer.WriteValue(rank);

                writer.WriteEndArray();
                writer.WritePropertyName("total");
                writer.WriteValue(total);
                writer.WritePropertyName("top");
                writer.WriteValue(top);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: navigateHome function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public void NavigateHome()
        {
            if(!IsConnectionReady("navigateHome()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("navigateHome");
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: navigateTo function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void NavigateTo(string url)
        {
            if(!IsConnectionReady("navigateTo()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("navigateTo");
                writer.WritePropertyName("data");
                writer.WriteValue(url);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        /// <summary>
        /// AirConsole API: showDefaultUI function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="showDefaultUI">If set to <c>true</c> show default UI.</param>
        public void ShowDefaultUI(bool showDefaultUI)
        {
            if(!IsConnectionReady("showDefaultUI()"))
                return;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("showDefaultUI");
                writer.WritePropertyName("data");
                writer.WriteValue(showDefaultUI);
                writer.WriteEnd();
            }

            controllerSingleton.Send(sendData.ToString());
            sendData.Length = 0;
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="Volplane.AirConsole.AirConsoleAgent"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="Volplane.AirConsole.AirConsoleAgent"/> in an unusable state. After calling <see cref="Dispose"/>,
        /// you must release all references to the <see cref="Volplane.AirConsole.AirConsoleAgent"/> so the garbage
        /// collector can reclaim the memory that the <see cref="Volplane.AirConsole.AirConsoleAgent"/> was occupying.</remarks>
        public void Dispose()
        {
            OnConnect = null;
            OnDisconnect = null;
            OnReady = null;
            OnMessage = null;
            OnDeviceStateChange = null;
            OnCustomDeviceStateChange = null;
            OnDeviceProfileChange = null;
            OnAdShow = null;
            OnAdComplete = null;
            OnPremium = null;
            OnPersistentDataLoaded = null;
            OnPersistentDataStored = null;
            OnHighScores = null;
            OnHighScoreStored = null;
        }


        #region AirConsole Data Processing

        /// <summary>
        /// AirConsole API: onConnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnConnectInternal()
        {
            // AirConsole device identifier in current APIData packet

            if(OnConnect != null)
                OnConnect(currentData.DeviceId ?? -1);
        }

        /// <summary>
        /// AirConsole API: onDisconnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnDisconnectInternal()
        {
            // AirConsole device identifier in current APIData packet

            if(OnDisconnect != null)
                OnDisconnect(currentData.DeviceId ?? -1);
        }

        /// <summary>
        /// AirConsole API: onReady callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnReadyInternal()
        {
            if(currentData.DeviceId != 0)
                return;

            if(!isConnectionReady)
                isConnectionReady = true;

            // Initialization data from the web interface will be parsed with LINQ for
            // easier management.
            // This process will run only once -> therefore speed, and memory allocations
            // are quite insignificant.
            JObject initData = JObject.Parse(currentData.Data.ToString());
            JArray acDevices = initData["devices"] as JArray;

            if(acDevices == null)
                return;

            this.acServerTimeOffset = (int)initData["server_time_offset"];
            this.acGameLocation = FormatGameUrl((string)initData["location"]);
            this.acDevices.Clear();

            for(int i = 0; i < acDevices.Count; i++)
            {
                if(acDevices[i] == null)
                    continue;

                if(acDevices[i].HasValues)
                {
                    this.acDevices.Add(
                        i,
                        Device.FromJSON(acDevices[i].ToString())
                    );
                }
            }

            if(OnReady != null)
                OnReady((string)initData["code"]);
        }

        /// <summary>
        /// AirConsole API: onMessage callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnMessageInternal()
        {
            // AirConsole device identifier and message data in current APIData packet

            if(OnMessage != null)
                OnMessage(currentData.DeviceId ?? -1, currentData.Data.ToString());
        }

        /// <summary>
        /// AirConsole API: onDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnDeviceStateChangeInternal()
        {
            // AirConsole device identifier and device state data in current APIData packet

            if(currentData.DeviceId == null)
                return;

            int index = (int)currentData.DeviceId;

            // Assigned data -> if not: disconnected...
            if(currentData.AssignedData)
            {
                if(acDevices.ContainsKey(index))
                {
                    Device.PopulateFromJSON(
                        currentData.Data.ToString(),
                        acDevices[index]
                    );
                }
                else
                {
                    acDevices.Add(
                        index,
                        Device.FromJSON(currentData.Data.ToString())
                    );
                }

                if(OnDeviceStateChange != null)
                    OnDeviceStateChange(index, acDevices[index]);
            }
            else if(acDevices.ContainsKey(index))
            {
                acDevices.Remove(index);

                if(OnDeviceStateChange != null)
                    OnDeviceStateChange(index, null);
            }
        }

        /// <summary>
        /// AirConsole API: onCustomDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnCustomDeviceStateChangeInternal()
        {
            // AirConsole device identifier and custom data in current APIData packet

            if(OnCustomDeviceStateChange != null)
                OnCustomDeviceStateChange(currentData.DeviceId ?? -1, currentData.Data.ToString());
        }

        /// <summary>
        /// AirConsole API: onDeviceProfileChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnDeviceProfileChangeInternal()
        {
            // AirConsole device identifier in current APIData packet

            if(OnDeviceProfileChange != null)
                OnDeviceProfileChange(currentData.DeviceId ?? -1);
        }

        /// <summary>
        /// AirConsole API: onAdShow callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnAdShowInternal()
        {
            if(OnAdShow != null)
                OnAdShow();
        }

        /// <summary>
        /// AirConsole API: onAdComplete callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnAdCompleteInternal()
        {
            // Ad was shown state in current APIData packet

            if(OnAdComplete != null)
                OnAdComplete(currentData.StateData ?? false);
        }

        /// <summary>
        /// AirConsole API: onPremium callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnPremiumInternal()
        {
            // AirConsole device identifier in current APIData packet

            if(OnPremium != null)
                OnPremium(currentData.DeviceId ?? -1);
        }

        /// <summary>
        /// AirConsole API: onPersistentDataLoaded callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnPersistentDataLoadedInternal()
        {
            // Persistent data in current APIData packet

            if(OnPersistentDataLoaded != null)
                OnPersistentDataLoaded(currentData.Data.ToString());
        }

        /// <summary>
        /// AirConsole API: onPersistentDataStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnPersistentDataStoredInternal()
        {
            // Unique user identifier in current APIData packet

            if(OnPersistentDataStored != null)
                OnPersistentDataStored(currentData.StringData);
        }

        /// <summary>
        /// AirConsole API: onHighScores callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnHighScoresInternal()
        {
            // Highscore data in current APIData packet

            if(OnHighScores != null)
                OnHighScores(currentData.Data.ToString());
        }

        /// <summary>
        /// AirConsole API: onHighScoreStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        protected void OnHighScoreStoredInternal()
        {
            // New highscore record data in current APIData packet

            if(OnHighScoreStored != null)
                OnHighScoreStored(currentData.Data.ToString());
        }

        #endregion

        /// <summary>
        /// Determines whether the AirConsole to Unity 3D connection is ready.
        /// This method throws an exception if connection not ready.
        /// </summary>
        /// <returns><c>true</c> if the connection is ready; otherwise, <c>false</c>.</returns>
        /// <param name="acFunction">Ac function.</param>
        protected bool IsConnectionReady(string acFunction = null)
        {
            if(!isConnectionReady)
            {
                if(acFunction != null)
                    throw new AirConsoleNotReadyException("AirConsole to Unity 3D connection is not ready!", acFunction);
                else
                    throw new AirConsoleNotReadyException("AirConsole to Unity 3D connection is not ready!");
            }

            return true;
        }

        /// <summary>
        /// Strips the actual URL on which the game is running so that it can be used for comparisation.
        /// </summary>
        /// <returns>A formatted URL.</returns>
        /// <param name="url">The current game URL.</param>
        protected string FormatGameUrl(string url)
        {
            if(url == null)
                return null;

            url = url.Split('#')[0];
            url = url.Split('?')[0];

            Regex rule = new Regex(@"(http://|https://|screen\.html|controller\.html)");
            url = rule.Replace(url, "");

            return url;
        }
    }
}
