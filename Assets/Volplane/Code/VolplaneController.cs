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
    using System.Runtime.InteropServices;
    using UnityEngine;
    using Volplane.AirConsole;
    using Volplane.Net;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public sealed class VolplaneController : MonoBehaviour
    {
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
        /// This singleton.
        /// </summary>
        private static VolplaneController VolplaneSingleton;

        /// <summary>
        /// SendData() external WebGL call.
        /// </summary>
        [DllImport("__Internal")]
        private static extern void SendData(string data);

        /// <summary>
        /// UnityIsReady() external WebGL call.
        /// </summary>
        /// <param name="autoScale">If set to <c>true</c> auto scale.</param>
        /// <param name="objectName">The name of this gameobject for interacting between WebGL and Unity scripting.</param>
        [DllImport("__Internal")]
        private static extern void UnityIsReady(bool autoScale, string objectName);


        // Instance variables

        [SerializeField] private bool usePersistentData = false;

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
            VolplaneController.AirConsole.ProcessData(data);
        }

        /// <summary>
        /// Method for sending data to AirConsole API.
        /// </summary>
        /// <param name="data">Data in JSON format.</param>
        public void Send(string data)
        {
#if UNITY_EDITOR

            websocketService.Message(data);

#else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
                SendData(data.ToString());

#endif
        }

        /// <summary>
        /// On awake.
        /// </summary>
        private void Awake()
        {
            // Use this object as singleton
            if((VolplaneSingleton != null) && (VolplaneSingleton != this))
            {
                Destroy(this.gameObject);
                return;
            }

            VolplaneSingleton = this;
            DontDestroyOnLoad(this.gameObject);

            // AirConsole agent
            VolplaneController.AirConsole = new AirConsoleAgent(this);

            // Volplane agent
            VolplaneController.Main = new VolplaneAgent();

            // Input handling
            VolplaneController.InputHandling = new VInput();

            // Use persistent data for the connected players
            VolplaneController.Main.UsePersistentData(this.usePersistentData);
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
                websocketServer.AddWebSocketService<VolplaneWebsocketService>(Config.WebsocketVirtualPath, delegate (VolplaneWebsocketService websocketService)
                {
                    this.websocketService = websocketService;
                    this.websocketService.dataReceived += ProcessData;
                });
                websocketServer.Start();
            }

#else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
                UnityIsReady(Config.AutoScaleCanvas, gameObject.name);

#endif
        }

        /// <summary>
        /// On application quit.
        /// </summary>
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR

            if(websocketServer == null)
                return;

            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");

#endif

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

        /// <summary>
        /// On disable this component.
        /// </summary>
        private void OnDisable()
        {
#if UNITY_EDITOR

            if(websocketServer == null)
                return;

            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");

#endif
        }

        /// <summary>
        /// Call update methods from main framework and input management.
        /// </summary>
        private void Update()
        {
            VolplaneController.Main.ControllerUpdate();
            VolplaneController.InputHandling.ControllerUpdate();
        }
    }
}
