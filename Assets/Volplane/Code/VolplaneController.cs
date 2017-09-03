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
    using SimpleJSON;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using Volplane.AirConsole;
    using Volplane.Net;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public sealed class VolplaneController : MonoBehaviour
    {
        // This object
        // TODO after implementing to recommended JS -> WebGL communication,
        //      maybe also update 'Extensions.cs' and make this object private
        public static VolplaneController VolplaneSingleton;

        /// <summary>
        /// AirConsole implementation.
        /// </summary>
        /// <value>AirConsole agent.</value>
        public static AirConsoleAgent AirConsole { get; private set; }

        /// <summary>
        /// All Volplane functionality.
        /// </summary>
        /// <value>Volplane agent.</value>
        public static VolplaneAgent Main { get; private set; }

        /// <summary>
        /// Controller input handling.
        /// </summary>
        /// <value>Implemented input handling object.</value>
        public static VInput InputHandling { get; private set; }

        /// <summary>
        /// Hello() external WebGL call.
        /// </summary>
        [DllImport("__Internal")]
        private static extern void Hello(bool test);

        #if UNITY_EDITOR
        private WebSocketServer websocketServer;
        private VolplaneWebsocketService websocketService;
        #endif

        /// <summary>
        /// Gateway method for AirConsole events.
        /// </summary>
        /// <param name="data">JSON formatted data sent from clients implemented AirConsole API.</param>
        public void ProcessData(string data)
        {
            VolplaneController.AirConsole.ProcessData(JSON.Parse(data));
        }

        /// <summary>
        /// Method for sending data to AirConsole API.
        /// </summary>
        /// <param name="data">JSON data.</param>
        public void Send(JSONObject data)
        {
            #if UNITY_EDITOR

			websocketService.Message(data);

            #else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Application.ExternalCall("window.volplane.processData", data.ToString());
                Hello(true);
            }

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
            VolplaneController.AirConsole = new AirConsoleAgent(this);

            // Volplane agent
            VolplaneController.Main = new VolplaneAgent();

            // Input handling
            VolplaneController.InputHandling = new VInput();

            // Initialize all VolplaneBehaviours in the Scene
            VolplaneBehaviour[] volplaneInstances = Resources.FindObjectsOfTypeAll<VolplaneBehaviour>();

            // Invoke initialization on every VolplaneBehaviour instance
            for(int i = 0; i < volplaneInstances.Length; i++)
            {
                typeof(VolplaneBehaviour).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(volplaneInstances[0], null);
            }
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
            {
                Application.ExternalCall("window.volplane.unityIsReady", Config.AutoScaleCanvas);
                Hello(false);
            }

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

            VolplaneController.AirConsole.Dispose();
            VolplaneController.AirConsole = null;
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

            if(VolplaneController.AirConsole != null)
            {
                VolplaneController.AirConsole.Dispose();
                VolplaneController.AirConsole = null;
            }

            if(VolplaneController.Main != null)
            {
                VolplaneController.Main.Dispose();
                VolplaneController.Main = null;
            }

            if(VolplaneController.InputHandling != null)
            {
                VolplaneController.InputHandling.Dispose();
                VolplaneController.InputHandling = null;
            }
        }

        #endif

        void Update()
        {
            VolplaneController.Main.ControllerUpdate();
            VolplaneController.InputHandling.ControllerUpdate();
        }
    }
}
