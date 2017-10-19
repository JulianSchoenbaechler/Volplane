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
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Net;
    using UnityEngine;


    public sealed class WebListener : IDisposable
    {
        private readonly HttpListener listener;
        private readonly ManualResetEvent stop, ready;
        private Thread listenerThread, workerThread;
        private Queue<HttpListenerContext> contextQueue;


        /// <summary>
        /// Initializes a new instance of the <see cref="WebListener"/> class.
        /// </summary>
        public WebListener()
        {
            this.listener = new HttpListener();
            this.contextQueue = new Queue<HttpListenerContext>();

            this.stop = new ManualResetEvent(false);
            this.ready = new ManualResetEvent(false);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="WebListener"/> is
        /// reclaimed by garbage collection.
        /// </summary>
        ~WebListener()
        {
            this.Dispose();
        }

        /// <summary>
        /// Occurs when a request is ready to be processed.
        /// </summary>
        public event Action<HttpListenerContext> ProcessRequest;

        /// <summary>
        /// Start listening on a specific port.
        /// </summary>
        /// <param name="port">The port.</param>
        public void Start(int port)
        {
            if(!listener.IsListening)
            {
                listener.Prefixes.Add(String.Format(@"http://*:{0}/", port));
                listener.Start();

                // Reset events
                stop.Reset();
                ready.Reset();

                // Threads
                listenerThread = new Thread(HandleRequest);
                workerThread = new Thread(Worker);

                listenerThread.Start();
                workerThread.Start();

            }
            else
            {
                if(Config.DebugLog != (int)DebugState.None)
                    VDebug.LogWarningFormat("[Volplane (Web Listener)] WebListener already running on port: {0}.", port);
            }
        }

        /// <summary>
        /// Stop web listener.
        /// </summary>
        public void Stop()
        {
            if(listener.IsListening)
            {
                stop.Set();
                listenerThread.Join();
                workerThread.Join();
                listener.Stop();
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebListener"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="WebListener"/>. The <see cref="Dispose"/>
        /// method leaves the <see cref="WebListener"/> in an unusable state. After calling <see cref="Dispose"/>, you must
        /// release all references to the <see cref="WebListener"/> so the garbage collector can reclaim the memory that the
        /// <see cref="WebListener"/> was occupying.</remarks>
        public void Dispose()
        {
            if(listener != null)
            {
                Stop();
                listener.Close();
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Thread for handling requests.
        /// </summary>
        private void HandleRequest()
        {
            while(listener.IsListening)
            {
                IAsyncResult context = listener.BeginGetContext(ContextReady, null);

                // Wait until any of the handles receive a signal
                // End thread if the reset event "stop" is set (index 0)
                if(WaitHandle.WaitAny(new[] { stop, context.AsyncWaitHandle }) == 0)
                    return;
            }
        }

        /// <summary>
        /// Gets called when a new HTTP context is received.
        /// </summary>
        /// <param name="result">Context result.</param>
        private void ContextReady(IAsyncResult result)
        {
            try
            {
                // Enqueue context and flag as ready
                lock(contextQueue)
                {
                    contextQueue.Enqueue(listener.EndGetContext(result));
                    ready.Set();
                }
            }
            catch(Exception e)
            {
                if((Config.DebugLog != (int)DebugState.None) && listener.IsListening)
                {
                    VDebug.LogError("[Volplane (Web Listener)] WebListener failed resolving context.");
                    VDebug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Processes the received packets.
        /// </summary>
        private void Worker()
        {
            WaitHandle[] waitHandles = new[] { ready, stop };
            HttpListenerContext context;

            // Wait for reset events
            // "ready" for executing the loop, "stop" for ending thread
            while(WaitHandle.WaitAny(waitHandles) == 0)
            {
                lock(contextQueue)
                {
                    if(contextQueue.Count > 0)
                    {
                        context = contextQueue.Dequeue();
                    }
                    else
                    {
                        ready.Reset();
                        continue;
                    }
                }

                // Fire event for context processing
                if(ProcessRequest != null)
                    ProcessRequest(context);
            }
        }
    }
}
