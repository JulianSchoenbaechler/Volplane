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

namespace Volplane
{
    using SimpleJSON;
    using UnityEngine;
    using Volplane.Net;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public class VolplaneController : MonoBehaviour
    {
        public static VolplaneController VolplaneSingleton;

        #if UNITY_EDITOR
        private WebSocketServer websocketServer;
        #endif

        public void ProcessData(string data)
        {
            Debug.Log(data);
            JSON.Parse(data);
        }

        public void Send(string data)
        {
            
        }

        private void Awake()
        {
            if((VolplaneSingleton != null) && (VolplaneSingleton != this))
                Destroy(this.gameObject);

            VolplaneSingleton = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            Application.runInBackground = true;

            #if UNITY_EDITOR

            if(Application.isEditor)
            {
                websocketServer = new WebSocketServer(Config.LocalWebsocketPort);
                websocketServer.AddWebSocketService<VolplaneWebsocketService>(Config.WebsocketVirtualPath, delegate(VolplaneWebsocketService websocketService)
                {
                    websocketService.dataReceived += ProcessData;
                });
                websocketServer.Start();
            }

            #else

            if(Application.platform == RuntimePlatform.WebGLPlayer)
                Application.ExternalCall("unityIsReady", Config.AutoScaleCanvas);

            #endif
        }

        #if UNITY_EDITOR

        private void OnApplicationQuit()
        {
            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");
        }

        private void OnDisable()
        {
            if(websocketServer.IsListening)
                websocketServer.Stop(CloseStatusCode.Normal, "Application has quit.");
        }

        #endif
    }
}
