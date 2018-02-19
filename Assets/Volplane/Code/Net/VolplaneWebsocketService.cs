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

namespace Volplane.Net
{
    using System;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public class VolplaneWebsocketService : WebSocketBehavior
    {
        private string suffix;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.Net.VolplaneWebsocketService"/> class.
        /// </summary>
        public VolplaneWebsocketService() : this(null)
        {
            // Empty...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.Net.VolplaneWebsocketService"/> class.
        /// </summary>
        /// <param name="suffix">Message suffix.</param>
        public VolplaneWebsocketService(string suffix)
        {
            this.suffix = suffix ?? string.Empty;
        }


        public event Action<string> dataReceived;

        /// <summary>
        /// Sending string data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Message(string data)
        {
            Send(data + suffix);
        }

        /// <summary>
        /// Gets called when websocket receives a message.
        /// </summary>
        /// <param name="e">Message data.</param>
        protected override void OnMessage(MessageEventArgs e)
        {
            if(e.IsText)
            {
                if(dataReceived != null)
                    dataReceived(e.Data);
            }
        }

        /// <summary>
        /// Gets called when websocket connection is opened.
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();

            if(Config.DebugLog != (int)DebugState.None)
            {
                VDebug.LogFormat("[Volplane (Websocket Service)] Socket connection opened on port: {0:D}.", Config.LocalWebsocketPort);
            }
        }

        /// <summary>
        /// Gets called when websocket connection is closed.
        /// </summary>
        /// <param name="e">Closed event data.</param>
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            // TODO: Inform VolplaneAgent?

            if(Config.DebugLog != (int)DebugState.None)
            {
                VDebug.LogFormat("[Volplane (Websocket Service)] Socket connection closed. Code: {0:D}.", e.Code);
            }
        }

        /// <summary>
        /// Gets called when websocket connection throws an error.
        /// </summary>
        /// <param name="e">Error event data.</param>
        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);

            if(Config.DebugLog != (int)DebugState.None)
            {
                VDebug.LogErrorFormat("[Volplane (Websocket Service)] {0:G}", e.Message);
                VDebug.LogException(e.Exception);
            }
        }
    }
}
