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
    using System.IO;
    using System.Net;
    using Volplane.IO;


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

                if(serverPath.StartsWith("/build/"))
                {
                    // Local build path
                    serverPath = serverPath.Replace("/build/", "/");
                    filePath = String.Format(
                        "{0:G}{1:G}",
                        Config.BuildPath,
                        serverPath
                    );
                }
                else if(serverPath.StartsWith("/volplane/"))
                {
                    // Local web server path
                    serverPath = serverPath.Replace("/volplane/", "/");
                    filePath = localPath + Config.WebServerPath + serverPath;
                }
                else
                {
                    // WebGL template path
                    filePath = localPath + Config.WebTemplatePath + serverPath;

                    if(String.Equals(Path.GetFileName(filePath), "screen.html"))
                        filePath = filePath.Replace("screen.html", "index.html");
                }

                filePath = pathSeparatorReg.Replace(filePath, Path.DirectorySeparatorChar.ToString());
                filePath = Uri.UnescapeDataString(filePath);

                if(File.Exists(filePath))
                {
                    buffer = File.ReadAllBytes(filePath);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = ReturnMIMEType(Path.GetExtension(filePath));
                    context.Response.ContentLength64 = buffer.Length;
                }
                else
                {
                    // File not found
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
                // Saving controller data
                buffer = System.Text.Encoding.UTF8.GetBytes("saved");

                string controllerName = null;

                try
                {
                    controllerName = FileManager.WriteJSON(request.InputStream, String.Format("{0:G}{1:G}/data/controller", localPath, Config.WebServerPath));

                    // If the controller is currently used
                    if(Config.SelectedController == controllerName)
                    {
                        // Copy selected controller data into WebGL template
                        File.Copy(String.Format("{0:G}{1:G}/data/controller/{2:G}.json", localPath, Config.WebServerPath, controllerName),
                                  String.Format("{0:G}{1:G}/controller.json", localPath, Config.WebTemplatePath),
                                  true);
                    }

                    if(Config.DebugLog == (int)DebugState.All)
                    {
                        VDebug.Log("[Volplane (File Manager)] Controller saved!");
                    }
                }
                catch(Exception e)
                {
                    buffer = System.Text.Encoding.UTF8.GetBytes(
                        String.Format("Could not write data to path: {0:G}{1:G}/data/controller", localPath, Config.WebServerPath)
                    );

                    if(Config.DebugLog == (int)DebugState.All)
                    {
                        VDebug.LogErrorFormat("[Volplane (File Manager)] Unable to write file to: '{0:G}{1:G}/data/controller'.", localPath, Config.WebServerPath);
                        VDebug.LogException(e);
                    }
                }
                finally
                {
                    // Response
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
}
