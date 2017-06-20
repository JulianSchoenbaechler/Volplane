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

namespace Volplane.AirConsole
{
    using SimpleJSON;
    using System;
    using System.Collections.Generic;

    public class AirConsoleAgent : IDisposable
    {
        protected bool isConnectionReady = false;
        protected IDictionary<int, JSONNode> acDevices;
		protected int acServerTimeOffset;
		protected string acGameLocation;

        public AirConsoleAgent()
        {
			this.acDevices = new Dictionary<int, JSONNode>();
		}

        // AirConsole events
        public event Action<int>            onConnect;
        public event Action<int>            onDisconnect;
        public event Action<string>         onReady;
        public event Action<int, JSONNode>  onMessage;
        public event Action<int, JSONNode>  onDeviceStateChange;
        public event Action<int, JSONNode>  onCustomDeviceStateChange;
        public event Action<int>            onDeviceProfileChange;
        public event Action                 onAdShow;
        public event Action<bool>           onAdComplete;
        public event Action<int>            onPremium;
        public event Action<JSONNode>       onPersistentDataLoaded;
        public event Action<string>         onPersistentDataStored;
        public event Action<JSONNode>       onHighScores;
        public event Action<JSONNode>       onHighScoreStored;
        public event Action                 onGameEnd;

        /// <summary>
        /// Processes the data send from AirConsole API.
        /// </summary>
        /// <param name="data">The received data.</param>
        public void ProcessData(JSONNode data)
        {
            switch(data["action"].Value)
            {
                case "onConnect":
                    OnConnect(data["device_id"].AsInt);
                    break;

                case "onDisconnect":
                    OnDisconnect(data["device_id"].AsInt);
                    break;

                case "onReady":
                    OnReady(data["code"].Value,
                            data["device_id"].AsInt,
                            data["devices"],
                            data["server_time_offset"].AsInt,
                            data["location"].Value);
                    break;

                case "onMessage":
                    OnMessage(data["from"].AsInt, data["data"]);
                    break;

                case "onDeviceStateChange":
                    OnDeviceStateChange(data["device_id"].AsInt, data["device_data"]);
                    break;

                case "onCustomDeviceStateChange":
                    OnCustomDeviceStateChange(data["device_id"].AsInt);
                    break;

                case "onDeviceProfileChange":
                    OnDeviceProfileChange(data["device_id"].AsInt);
                    break;

                case "onAdShow":
                    OnAdShow();
                    break;

                case "onAdComplete":
                    OnAdComplete(data["ad_was_shown"].AsBool);
                    break;

                case "onPremium":
                    OnPremium(data["device_id"].AsInt);
                    break;

                case "onPersistentDataLoaded":
                    OnPersistentDataLoaded(data["data"]);
                    break;

                case "onPersistentDataStored":
                    OnPersistentDataStored(data["uid"].Value);
                    break;

                case "onHighScores":
                    OnHighScores(data["highscores"]);
                    break;

                case "onHighScoreStored":
                    OnHighScoreStored(data["highscore"]);
                    break;

                case "onGameEnd":
                    OnGameEnd();
                    break;

                default:
                    break;
            }
        }

        public void Dispose()
        {
            onConnect = null;
            onDisconnect = null;
            onReady = null;
            onMessage = null;
            onDeviceStateChange = null;
            onCustomDeviceStateChange = null;
            onDeviceProfileChange = null;
            onAdShow = null;
            onAdComplete = null;
            onPremium = null;
            onPersistentDataLoaded = null;
            onPersistentDataStored = null;
            onHighScores = null;
            onHighScoreStored = null;
            onGameEnd = null;
        }


        // AirConsole data processing

		/// <summary>
		/// AirConsole API: onConnect callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnConnect(int acDeviceId)
        {
			if(onConnect != null)
                onConnect(acDeviceId);
        }

		/// <summary>
		/// AirConsole API: onDisconnect callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnDisconnect(int acDeviceId)
        {
			if(onDisconnect != null)
                onDisconnect(acDeviceId);
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
        protected void OnReady(string acGameCode,
                               int acDeviceId,
                               JSONNode acDevices,
                               int acServerTimeOffset,
                               string acLocation)
        {
			if(acDeviceId != 0)
				return;
			
            if(!isConnectionReady)
                isConnectionReady = true;

			this.acServerTimeOffset = acServerTimeOffset;
			this.acGameLocation = acLocation;
			this.acDevices.Clear();

			for(int i = 0; i < acDevices.Count; i++)
			{
				if(acDevices.AsArray[i].AsObject.Count > 0)
					this.acDevices.Add(i, acDevices.AsArray[i]);
			}
            
            if(onReady != null)
                onReady(acGameCode);
        }

		/// <summary>
		/// AirConsole API: onMessage callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceIdSender">AirConsole device identifier sender.</param>
		/// <param name="data">Message data.</param>
        protected void OnMessage(int acDeviceIdSender, JSONNode data)
        {
			UnityEngine.Debug.Log(data.Value);
			/*
			switch(data["someKey"].Value)
			{
				case "someVolplaneAction":
					// redirect
					break;

				default:
					break;
			}
			*/

            if(onMessage != null)
                onMessage(acDeviceIdSender, data);
        }

		/// <summary>
		/// AirConsole API: onDeviceStateChange callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
		/// <param name="state">Devic state.</param>
        protected void OnDeviceStateChange(int acDeviceId, JSONNode state)
        {
			if(acDevices.ContainsKey(acDeviceId))
				acDevices[acDeviceId] = state;
			else
				acDevices.Add(acDeviceId, state);

            if(onDeviceStateChange != null)
                onDeviceStateChange(acDeviceId, state);
        }

		/// <summary>
		/// AirConsole API: onCustomDeviceStateChange callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnCustomDeviceStateChange(int acDeviceId)
        {
			JSONNode customState = acDeviceId;

            if(onCustomDeviceStateChange != null)
				onCustomDeviceStateChange(acDeviceId, customState);
        }

		/// <summary>
		/// AirConsole API: onDeviceProfileChange callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnDeviceProfileChange(int acDeviceId)
        {
            if(onDeviceProfileChange != null)
                onDeviceProfileChange(acDeviceId);
        }

		/// <summary>
		/// AirConsole API: onAdShow callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
        protected void OnAdShow()
        {
            if(onAdShow != null)
                onAdShow();
        }

		/// <summary>
		/// AirConsole API: onAdComplete callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acAdWasShown">If set to <c>true</c> AirConsole ad was shown.</param>
        protected void OnAdComplete(bool acAdWasShown)
        {
            if(onAdComplete != null)
                onAdComplete(acAdWasShown);
        }

		/// <summary>
		/// AirConsole API: onPremium callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void OnPremium(int acDeviceId)
        {
            if(onPremium != null)
                onPremium(acDeviceId);
        }

		/// <summary>
		/// AirConsole API: onPersistentDataLoaded callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="data">Loaded data.</param>
        protected void OnPersistentDataLoaded(JSONNode data)
        {
            if(onPersistentDataLoaded != null)
                onPersistentDataLoaded(data);
        }

		/// <summary>
		/// AirConsole API: onPersistentDataStored callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acUid">Unique user identifier.</param>
        protected void OnPersistentDataStored(string acUid)
        {
            if(onPersistentDataStored != null)
                onPersistentDataStored(acUid);
        }

		/// <summary>
		/// AirConsole API: onHighScores callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acHighscoreData">AirConsole highscore data.</param>
        protected void OnHighScores(JSONNode acHighscoreData)
        {
            if(onHighScores != null)
                onHighScores(acHighscoreData);
        }

		/// <summary>
		/// AirConsole API: onHighScoreStored callback.
		/// See <see href="https://developers.airconsole.com/#!/api">https://developers.airconsole.com/#!/api</see>
		/// for the AirConsole documentation.
		/// </summary>
		/// <param name="acNewRecordData">AirConsole new highscore record data.</param>
        protected void OnHighScoreStored(JSONNode acNewRecordData)
        {
            if(onHighScoreStored != null)
                onHighScoreStored(acNewRecordData);
        }

		/// <summary>
		/// AirConsole: Game browser window closed.
		/// </summary>
        protected void OnGameEnd()
        {
            if(isConnectionReady)
                isConnectionReady = false;
            
            if(onGameEnd != null)
                onGameEnd();
        }
    }
}
