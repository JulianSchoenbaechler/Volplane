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
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnityEngine;

    public class AirConsoleAgent : IDisposable
    {
        protected VolplaneController controllerSingleton;
        protected bool isConnectionReady = false;
        protected IDictionary<int, JSONNode> acDevices;
        protected IList<int> acPlayerNumbers;
        protected int acServerTimeOffset;
        protected string acGameLocation;

        private DateTime epochTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.AirConsole.AirConsoleAgent"/> class.
        /// </summary>
        /// <param name="singleton">Reference to the Unity singleton VolplaneController object.</param>
        public AirConsoleAgent(VolplaneController singleton)
        {
            this.controllerSingleton = singleton;
            this.acDevices = new Dictionary<int, JSONNode>();
            this.acPlayerNumbers = new List<int>();
            this.epochTime = new DateTime(1970, 1, 1);
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
        public event Action<int, JSONNode> OnMessage;

        /// <summary>
        /// AirConsole API: onDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int, JSONNode> OnDeviceStateChange;

        /// <summary>
        /// AirConsole API: onCustomDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<int, JSONNode> OnCustomDeviceStateChange;

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
        public event Action<JSONNode> OnPersistentDataLoaded;

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
        public event Action<JSONNode> OnHighScores;

        /// <summary>
        /// AirConsole API: onHighScoreStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        public event Action<JSONNode> OnHighScoreStored;

        /// <summary>
        /// Occurs when the game browser window is being closed.
        /// </summary>
        public event Action OnGameEnd;

        #endregion

        /// <summary>
        /// Processes the data send from AirConsole API.
        /// </summary>
        /// <param name="data">The received data.</param>
        public void ProcessData(JSONNode data)
        {
            switch(data["action"].Value)
            {
                case "onConnect":
                    OnConnectInternal(data["device_id"].AsInt);
                    break;

                case "onDisconnect":
                    OnDisconnectInternal(data["device_id"].AsInt);
                    break;

                case "onReady":
                    OnReadyInternal(data["code"].Value,
                                    data["device_id"].AsInt,
                                    data["devices"].AsArray,
                                    data["server_time_offset"].AsInt,
                                    data["location"].Value);
                    break;

                case "onMessage":
                    OnMessageInternal(data["from"].AsInt, data["data"]);
                    break;

                case "onDeviceStateChange":
                    OnDeviceStateChangeInternal(data["device_id"].AsInt, data["device_data"]);
                    break;

                case "onCustomDeviceStateChange":
                    OnCustomDeviceStateChangeInternal(data["device_id"].AsInt, data["custom_data"]);
                    break;

                case "onDeviceProfileChange":
                    OnDeviceProfileChangeInternal(data["device_id"].AsInt);
                    break;

                case "onAdShow":
                    OnAdShowInternal();
                    break;

                case "onAdComplete":
                    OnAdCompleteInternal(data["ad_was_shown"].AsBool);
                    break;

                case "onPremium":
                    OnPremiumInternal(data["device_id"].AsInt);
                    break;

                case "onPersistentDataLoaded":
                    OnPersistentDataLoadedInternal(data["data"]);
                    break;

                case "onPersistentDataStored":
                    OnPersistentDataStoredInternal(data["uid"].Value);
                    break;

                case "onHighScores":
                    OnHighScoresInternal(data["highscores"]);
                    break;

                case "onHighScoreStored":
                    OnHighScoreStoredInternal(data["highscore"]);
                    break;

                case "onGameEnd":
                    OnGameEndInternal();
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
        /// <param name="data">Data.</param>
        public void Message(int acDeviceIdReceiver, JSONNode data)
        {
            if(!IsConnectionReady("message()"))
                return;

            JSONObject stream = new JSONObject();
            stream["action"] = "message";
            stream["to"].AsInt = acDeviceIdReceiver;
            stream["data"] = data;

            controllerSingleton.Send(stream);
        }

        /// <summary>
        /// AirConsole API: broadcast function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Broadcast(JSONNode data)
        {
            if(!IsConnectionReady("broadcast()"))
                return;

            JSONObject stream = new JSONObject();
            stream["action"] = "broadcast";
            stream["data"] = data;

            controllerSingleton.Send(stream);
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

            List<int> controllerIds = acDevices.Keys.ToList();
            controllerIds.Remove(0);

            return controllerIds as ICollection<int>;
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
        /// <returns>The custom device state data.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public JSONNode GetCustomDeviceState(int acDeviceId = 0)
        {
            if(!IsConnectionReady("getCustomDeviceState()"))
                return null;

            if(acDevices.ContainsKey(acDeviceId))
            {
                return acDevices[acDeviceId]["custom"];
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
        /// <param name="data">Custom device state data.</param>
        public void SetCustomDeviceState(JSONNode data)
        {
            if(!IsConnectionReady("setCustomDeviceState()"))
                return;

            JSONObject stream = new JSONObject();
            stream["action"] = "setCustomDeviceState";
            stream["data"] = data;

            if(acDevices.Count > 0)
            {
                acDevices[0]["custom"] = data;
            }
            else
            {
                JSONNode state = new JSONObject();
                state["custom"] = data;
                acDevices.Add(0, state);
            }

            controllerSingleton.Send(stream);
        }

        /// <summary>
        /// AirConsole API: setCustomDeviceStateProperty function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void SetCustomDeviceStateProperty(string key, JSONNode value)
        {
            if(!IsConnectionReady("setCustomDeviceStateProperty()"))
                return;

            JSONObject stream = new JSONObject();
            stream["action"] = "setCustomDeviceStateProperty";
            stream["key"] = key;
            stream["value"] = value;

            if(acDevices.Count > 0)
            {
                acDevices[0]["custom"][key] = value;
            }
            else
            {
                JSONNode state = new JSONObject();
                state["custom"][key] = value;
                acDevices.Add(0, state);
            }

            controllerSingleton.Send(stream);
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
                if(acDevices[acDeviceId]["nickname"] != null)
                    return acDevices[acDeviceId]["nickname"].Value;
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
                return acDevices[acDeviceId]["uid"].Value;
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
            {
                return acDevices[acDeviceId]["auth"].AsBool;
            }
            else
            {
                return false;
            }
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
            JSONObject stream = new JSONObject();

            stream["action"] = "setActivePlayers";
            stream["max_players"].AsInt = maxPlayers;

            acPlayerNumbers.Clear();

            foreach(int id in controllerIds)
            {
                acPlayerNumbers.Add(id);
            }

            controllerSingleton.Send(stream);
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

            JSONObject stream = new JSONObject();
            stream["action"] = "showAd";

            controllerSingleton.Send(stream);
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
            {
                return acDevices[acDeviceId]["premium"].AsBool;
            }

            return false;
        }

        /// <summary>
        /// AirConsole API: storePersistentData function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value data.</param>
        /// <param name="uid">User id.</param>
        public void StorePersistentData(string key, JSONNode value, string uid = null)
        {
            if(!IsConnectionReady("storePersistentData()"))
                return;

            JSONObject stream = new JSONObject();
            stream["action"] = "storePersistentData";
            stream["key"] = key;
            stream["value"] = value;
            stream["uid"] = uid == null ? GetUID() : uid;

            controllerSingleton.Send(stream);
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

            JSONArray streamUids = new JSONArray();

            foreach(string uid in uids)
            {
                streamUids[-1] = uid;
            }

            JSONObject stream = new JSONObject();
            stream["action"] = "requestPersistentData";
            stream["uids"] = streamUids;

            controllerSingleton.Send(stream);
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
        /// <param name="data">Highscore data.</param>
        /// <param name="scoreString">Human readable score.</param>
        public void StoreHighScore(string levelName,
                                   string levelVersion,
                                   float score,
                                   ICollection<string> uids = null,
                                   JSONNode data = null,
                                   string scoreString = null)
        {
            if(!IsConnectionReady("storeHighScore()"))
                return;

            JSONArray streamUids = null;

            if(uids != null)
            {
                streamUids = new JSONArray();

                foreach(string uid in uids)
                {
                    streamUids[-1] = uid;
                }
            }

            JSONObject stream = new JSONObject();
            stream["action"] = "storeHighScore";
            stream["level_name"] = levelName;
            stream["level_version"] = levelName;
            stream["score"].AsFloat = score;
            stream["uid"] = streamUids;
            stream["data"] = data;
            stream["score_string"] = scoreString;

            controllerSingleton.Send(stream);
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
        /// <param name="data">Highscore data.</param>
        /// <param name="scoreString">Human readable score.</param>
        public void StoreHighScore(string levelName,
                                   string levelVersion,
                                   float score,
                                   string uid = null,
                                   JSONNode data = null,
                                   string scoreString = null)
        {
            if(!IsConnectionReady("storeHighScore()"))
                return;

            StoreHighScore(levelName, levelVersion, score, new string[] { uid }, data, scoreString);
        }

        /// <summary>
        /// AirConsole API: requestHighScore function.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="levelVersion">Level version.</param>
        /// <param name="uids">User ids.</param>
        /// <param name="ranks">High score rank types.</param>
        /// <param name="total">Amount of high scores per rank type.</param>
        /// <param name="top">Amount of top high scores per rank type.</param>
        public void RequestHighScore(string levelName,
                                     string levelVersion,
                                     ICollection<string> uids = null,
                                     ICollection<string> ranks = null,
                                     int total = -1,
                                     int top = -1)
        {
            if(!IsConnectionReady("requestHighScore()"))
                return;

            // UIDs
            JSONArray streamUids = null;

            if(uids != null)
            {
                streamUids = new JSONArray();

                foreach(string uid in uids)
                {
                    streamUids[-1] = uid;
                }
            }

            // Ranks
            JSONArray streamRanks = null;

            if(ranks != null)
            {
                streamRanks = new JSONArray();

                foreach(string rank in ranks)
                {
                    streamRanks[-1] = rank;
                }
            }

            JSONObject stream = new JSONObject();
            stream["action"] = "requestHighScore";
            stream["level_name"] = levelName;
            stream["level_version"] = levelName;
            stream["uid"] = streamUids;
            stream["ranks"] = streamRanks;
            stream["total"] = total;
            stream["total"] = top;

            controllerSingleton.Send(stream);
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

            JSONObject stream = new JSONObject();
            stream["action"] = "navigateHome";

            controllerSingleton.Send(stream);
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

            JSONObject stream = new JSONObject();
            stream["action"] = "navigateTo";
            stream["data"] = url;

            controllerSingleton.Send(stream);
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

            JSONObject stream = new JSONObject();
            stream["action"] = "showDefaultUI";
            stream["data"].AsBool = showDefaultUI;

            controllerSingleton.Send(stream);
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
            OnGameEnd = null;
        }


        #region AirConsole Data Processing

        /// <summary>
        /// AirConsole API: onConnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnConnectInternal(int acDeviceId)
        {
            if(OnConnect != null)
                OnConnect(acDeviceId);
        }

        /// <summary>
        /// AirConsole API: onDisconnect callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnDisconnectInternal(int acDeviceId)
        {
            if(OnDisconnect != null)
                OnDisconnect(acDeviceId);
        }

        /// <summary>
        /// AirConsole API: onReady callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acGameCode">AirConsole game code.</param>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        /// <param name="acDevices">AirConsole connected devices.</param>
        /// <param name="acServerTimeOffset">AirConsole server time offset.</param>
        /// <param name="acLocation">AirConsole game url.</param>
        protected void OnReadyInternal(string acGameCode,
                                       int acDeviceId,
                                       JSONNode acDevices,
                                       int acServerTimeOffset,
                                       string acLocation)
        {
            if((acDeviceId != 0) || (acDevices == null))
                return;

            if(!isConnectionReady)
                isConnectionReady = true;

            this.acServerTimeOffset = acServerTimeOffset;
            this.acGameLocation = acLocation;
            this.acDevices.Clear();

            for(int i = 0; i < acDevices.Count; i++)
            {
                if(acDevices.AsArray[i].AsObject == null)
                    continue;

                if(acDevices.AsArray[i].AsObject.Count > 0)
                    this.acDevices.Add(i, acDevices.AsArray[i]);
            }

            if(OnReady != null)
                OnReady(acGameCode);
        }

        /// <summary>
        /// AirConsole API: onMessage callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceIdSender">AirConsole device identifier sender.</param>
        /// <param name="data">Message data.</param>
        protected void OnMessageInternal(int acDeviceIdSender, JSONNode data)
        {
            if(OnMessage != null)
                OnMessage(acDeviceIdSender, data);
        }

        /// <summary>
        /// AirConsole API: onDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        /// <param name="state">Devic state.</param>
        protected void OnDeviceStateChangeInternal(int acDeviceId, JSONNode state)
        {
            if(acDevices.ContainsKey(acDeviceId))
                acDevices[acDeviceId] = state;
            else
                acDevices.Add(acDeviceId, state);

            if(OnDeviceStateChange != null)
                OnDeviceStateChange(acDeviceId, state);
        }

        /// <summary>
        /// AirConsole API: onCustomDeviceStateChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnCustomDeviceStateChangeInternal(int acDeviceId, JSONNode state)
        {
            if(OnCustomDeviceStateChange != null)
                OnCustomDeviceStateChange(acDeviceId, state);
        }

        /// <summary>
        /// AirConsole API: onDeviceProfileChange callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnDeviceProfileChangeInternal(int acDeviceId)
        {
            if(OnDeviceProfileChange != null)
                OnDeviceProfileChange(acDeviceId);
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
        /// <param name="acAdWasShown">If set to <c>true</c> AirConsole ad was shown.</param>
        protected void OnAdCompleteInternal(bool acAdWasShown)
        {
            if(OnAdComplete != null)
                OnAdComplete(acAdWasShown);
        }

        /// <summary>
        /// AirConsole API: onPremium callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnPremiumInternal(int acDeviceId)
        {
            if(OnPremium != null)
                OnPremium(acDeviceId);
        }

        /// <summary>
        /// AirConsole API: onPersistentDataLoaded callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="data">Loaded data.</param>
        protected void OnPersistentDataLoadedInternal(JSONNode data)
        {
            if(OnPersistentDataLoaded != null)
                OnPersistentDataLoaded(data);
        }

        /// <summary>
        /// AirConsole API: onPersistentDataStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acUid">Unique user identifier.</param>
        protected void OnPersistentDataStoredInternal(string acUid)
        {
            if(OnPersistentDataStored != null)
                OnPersistentDataStored(acUid);
        }

        /// <summary>
        /// AirConsole API: onHighScores callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acHighscoreData">AirConsole highscore data.</param>
        protected void OnHighScoresInternal(JSONNode acHighscoreData)
        {
            if(OnHighScores != null)
                OnHighScores(acHighscoreData);
        }

        /// <summary>
        /// AirConsole API: onHighScoreStored callback.
        /// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
        /// for the AirConsole documentation.
        /// </summary>
        /// <param name="acNewRecordData">AirConsole new highscore record data.</param>
        protected void OnHighScoreStoredInternal(JSONNode acNewRecordData)
        {
            if(OnHighScoreStored != null)
                OnHighScoreStored(acNewRecordData);
        }

        /// <summary>
        /// AirConsole: Game browser window closed.
        /// </summary>
        protected void OnGameEndInternal()
        {
            if(isConnectionReady)
                isConnectionReady = false;

            if(OnGameEnd != null)
                OnGameEnd();
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
    }
}
