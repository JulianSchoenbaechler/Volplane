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
    using System.Text.RegularExpressions;


    public class WebServer
    {
        protected WebListener listener;
        protected int port;
        protected string localPath;
        protected Regex pathSeparatorReg;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.Net.WebServer"/> class.
        /// Creates a local web server running on a specific port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="localPath">Local path.</param>
        public WebServer(int port, string localPath)
        {
            this.port = port;
            this.localPath = localPath;

            this.pathSeparatorReg = new Regex(@"[\\\/]");

            this.listener = new WebListener();
            this.listener.ProcessRequest += this.ProcessRequest;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this webserver is running.
        /// </summary>
        /// <value><c>true</c> if this webserver is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Start web server.
        /// </summary>
        public void Start()
        {
            listener.Start(port);
            IsRunning = true;
        }

        /// <summary>
        /// Stop web server.
        /// </summary>
        public void Stop()
        {
            listener.Stop();
            IsRunning = false;
        }

        /// <summary>
        /// Restart web server.
        /// </summary>
        public void Restart()
        {
            listener.Stop();
            listener.Start(port);
        }

        /// <summary>
        /// Reset web server.
        /// </summary>
        public void Reset()
        {
            listener.ProcessRequest -= ProcessRequest;
            listener.Dispose();

            listener = new WebListener();
            listener.ProcessRequest += ProcessRequest;
        }

        /// <summary>
        /// Standard procedure for processing the incoming request.
        /// </summary>
        /// <param name="context">HttpListener context.</param>
        protected virtual void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            string filePath = localPath + request.Url.LocalPath;

            filePath = pathSeparatorReg.Replace(filePath, Path.DirectorySeparatorChar.ToString());
            filePath = Uri.UnescapeDataString(filePath);

            if(File.Exists(filePath))
            {
                byte[] buffer = File.ReadAllBytes(filePath);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = ReturnMIMEType(Path.GetExtension(filePath));
                context.Response.ContentLength64 = buffer.Length;

                using(Stream s = context.Response.OutputStream)
                {
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentLength64 = 0;
                context.Response.Close();
            }
        }

        /// <summary>
        /// Returns the MIME type of a file by its extension.
        /// </summary>
        /// <returns>The MIME type.</returns>
        /// <param name="extension">A file extension.</param>
        protected string ReturnMIMEType(string extension)
        {
            switch(extension)
            {
                case ".txt":
                    return "text/plain";

                case ".gif":
                    return "image/gif";

                case ".png":
                    return "image/png";

                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";

                case ".bmp":
                    return "image/bmp";

                case ".wav":
                    return "audio/wav";

                case ".mp3":
                    return "audio/mp3";

                case ".html":
                case ".htm":
                    return "text/html";

                case ".css":
                    return "text/css";

                case ".js":
                case ".jsweb":
					return "application/javascript";

				case ".svg":
					return "image/svg+xml";

				case ".ttf":
					return "application/x-font-truetype";

				case ".otf":
					return "application/x-font-opentype";

				case ".woff":
					return "application/font-woff";

				case ".woff2":
					return "application/font-woff2";

				case ".eot":
					return "application/vnd.ms-fontobject";

				case ".sfnt":
					return "application/font-sfnt";

                default:
                    return "application/octet-stream";
            }
        }
    }
}
