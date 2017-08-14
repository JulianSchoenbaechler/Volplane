﻿/*
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
    using SimpleJSON;
    using System;
    using UnityEngine;
    using Volplane.AirConsole;
    using Volplane.Net;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public sealed class VolplaneController : MonoBehaviour
    {
        // This object
        public static VolplaneController VolplaneSingleton;

        #if UNITY_EDITOR
        private WebSocketServer websocketServer;
		private VolplaneWebsocketService websocketService;
        #endif
        private AirConsoleAgent agent;
        private ACInput inputHandler;

        // Implemented AirConsole agent
        public AirConsoleAgent AirConsole
        {
            get { return this.agent; }
        }

        /// <summary>
        /// Gateway method for AirConsole events.
        /// </summary>
        /// <param name="data">JSON formatted data sent from clients implemented AirConsole API.</param>
        public void ProcessData(string data)
        {
			agent.ProcessData(JSON.Parse(data));
        }

        /// <summary>
        /// Method for sending data to AirConsole API.
        /// </summary>
        /// <param name="data">JSON data.</param>
        public void Send(JSONObject data)
        {
            #if UNITY_EDITOR

			if(Application.isEditor)
				websocketService.Message(data);

            #else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
                Application.ExternalCall("window.volplane.processData", data.ToString());

            #endif
        }

        /// <summary>
        /// On awake.
        /// </summary>
        private void Awake()
        {
            // Use this object as singleton
            if((VolplaneSingleton != null) && (VolplaneSingleton != this))
                Destroy(this.gameObject);

            VolplaneSingleton = this;
            DontDestroyOnLoad(this.gameObject);

            // AirConsole agent
            agent = new AirConsoleAgent(this);
            inputHandler = new ACInput(agent);
        }

        /// <summary>
        /// On start.
        /// </summary>
        private void Start()
        {
            Application.runInBackground = true;

            #if UNITY_EDITOR

            if(Application.isEditor)
            {
                // Websocket management
                websocketServer = new WebSocketServer(Config.LocalWebsocketPort);
                websocketServer.AddWebSocketService<VolplaneWebsocketService>(Config.WebsocketVirtualPath, delegate(VolplaneWebsocketService websocketService)
                {
					this.websocketService = websocketService;
                    this.websocketService.dataReceived += ProcessData;
                });
                websocketServer.Start();
            }

            #else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
                Application.ExternalCall("window.volplane.unityIsReady", Config.AutoScaleCanvas);

            #endif
        }

        #if UNITY_EDITOR

        /// <summary>
        /// On application quit.
        /// </summary>
        private void OnApplicationQuit()
        {
			if(websocketServer == null)
				return;

            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");

            inputHandler.Dispose();
            agent.Dispose();
        }

        /// <summary>
        /// On disable this component.
        /// </summary>
        private void OnDisable()
		{
			if(websocketServer == null)
				return;
			
            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");
        }

        #endif

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                print(agent.GetMasterControllerDeviceId());
            }
        }
    }
}
