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

namespace Volplane.Net
{
    using System;
    using System.IO;
    using System.Net;


    public class VolplaneServer : WebServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.Net.VolplaneServer"/> class.
        /// Creates a local web server with the Volplane configuration running on a specific port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="localPath">Local path.</param>
        public VolplaneServer(int port, string localPath) : base(port, localPath)
        {
            // Empty constructor
        }

        /// <summary>
        /// Volplane server procedure for processing the incoming request.
        /// Request for files: 'screen.html', and 'controller.html' are sent from the AirConsole simulator.
        /// </summary>
        /// <param name="context">HttpListener context.</param>
        protected override void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            byte[] buffer;

            // If POST data is sent -> request coming from controller editor.
            if(!request.HasEntityBody)
            {
                string filePath;
                string serverPath = request.Url.LocalPath;

                if(serverPath.StartsWith("/volplane/") ||
                   String.Equals(Path.GetFileName(serverPath), "screen.html"))
                {
                    serverPath = serverPath.Replace("/volplane/", "/");
                    filePath = localPath + Config.WebServerPath + serverPath;
                }
                else
                {
                    filePath = localPath + Config.WebTemplatePath + serverPath;
                }

                filePath = filePath.Replace('/', '\\');

                if(File.Exists(filePath))
                {
                    buffer = File.ReadAllBytes(filePath);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = ReturnMIMEType(Path.GetExtension(filePath));
                    context.Response.ContentLength64 = buffer.Length;
                }
                else
                {
                    buffer = System.Text.Encoding.UTF8.GetBytes("<html><head><title>404 Not Found.</title></head><body><h1>Error 404 - Not Found.</h1></body></html>");

                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = ReturnMIMEType("text/html");
                    context.Response.ContentLength64 = buffer.Length;
                }

                using(Stream s = context.Response.OutputStream)
                {
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                /*
                // Controller editor data
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentLength64 = 0;
                context.Response.Close();
                */

                using(Stream body = request.InputStream)
                {
                    using(StreamReader reader = new StreamReader(body, request.ContentEncoding))
                    {
                        UnityEngine.Debug.Log(reader.ReadToEnd());
                    }
                }

                buffer = System.Text.Encoding.UTF8.GetBytes("saved");

                // Controller editor data
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = buffer.Length;

                using(Stream s = context.Response.OutputStream)
                {
                    s.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
