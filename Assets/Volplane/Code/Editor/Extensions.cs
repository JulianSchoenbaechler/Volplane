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

namespace Volplane.Editor
{
    using SimpleJSON;
    using System;
    using System.IO;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using UnityEditor;
    using UnityEngine;
    using Volplane.Net;

    [InitializeOnLoad]
    public class Extensions
    {
        /// <summary>
        /// The local webserver.
        /// </summary>
        public static VolplaneServer LocalWebserver;

        protected static bool processedEnteringPlaymode;

        /// <summary>
        /// Initializes the <see cref="Volplane.Editor.Extensions"/> class.
        /// Will be loaded with Unity.
        /// </summary>
        static Extensions()
        {
            // Load config
            Extensions.LoadSettings();

            // Setup WebGL template
            Extensions.SetupWebGLTemplate();

            // Start webserver
            Extensions.LocalWebserver = new VolplaneServer(Config.LocalServerPort, Application.dataPath);
            Extensions.LocalWebserver.Start();

            // Register playmode stat change event
            EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
        }

        /// <summary>
        /// Occurs when entering playmode.
        /// </summary>
        public static event Action EnteringPlaymode;

        /// <summary>
        /// Loads the Volplane configuration preferences.
        /// </summary>
        protected static void LoadSettings()
        {
            Config.LocalServerPort = EditorPrefs.GetInt("LocalServerPort", Config.DefaultLocalServerPort);
            Config.LocalWebsocketPort = EditorPrefs.GetInt("LocalWebsocketPort", Config.DefaultLocalWebsocketPort);
            Config.DebugLog = EditorPrefs.GetInt("DebugLog", (int)DebugState.None);
            Config.BrowserStart = EditorPrefs.GetInt("BrowserStart", (int)BrowserStartMode.Standard);
            Config.AutoScaleCanvas = EditorPrefs.GetBool("AutoScaleCanvas", true);
            Config.SelectedController = EditorPrefs.GetString("SelectedController", null);
        }

        /// <summary>
        /// Specifies the correct index file for the WebGL template according to the Unity version.
        /// </summary>
        protected static void SetupWebGLTemplate()
        {
            string savedIndex = EditorPrefs.GetString("TemplateIndex", null);

            if(savedIndex == null)
            {
                #if UNITY_5_6_OR_NEWER
                savedIndex = "5-6";
                #else
                savedIndex = "pre-5-6";
                #endif

                EditorPrefs.SetString("TemplateIndex", savedIndex);

                // Copy specified index file for web template
                File.Copy(
                    String.Format("{0:G}/{1:G}.html", Config.WebTemplateIndexPath, savedIndex),
                    Config.WebTemplatePath + "/index.html",
                    true
                );
            }
            #if UNITY_5_6_OR_NEWER
            else if(savedIndex == "pre-5-6")
            {
                savedIndex = "5-6";
                EditorPrefs.SetString("TemplateIndex", savedIndex);

                // Copy specified index file for web template
                File.Copy(
                    String.Format("{0:G}/{1:G}.html", Config.WebTemplateIndexPath, savedIndex),
                    Config.WebTemplatePath + "/index.html",
                    true
                );
            }
            #else
            else if(savedIndex == "5-6")
            {
                savedIndex = "pre-5-6";
                EditorPrefs.SetString("TemplateIndex", savedIndex);

                // Copy specified index file for web template
                File.Copy(
                    String.Format("{0:G}/{1:G}.html", Config.WebTemplateIndexPath, savedIndex),
                    Config.WebTemplatePath + "/index.html",
                    true
                );
            }
            #endif
        }

        /// <summary>
        /// Playmode state change listener.
        /// Firing entering playmode event.
        /// </summary>
        protected static void PlaymodeStateChanged()
        {
            if(GameObject.FindWithTag("Volplane") == null)
                return;
            
            if(EditorApplication.isPlayingOrWillChangePlaymode &&
               EditorApplication.isPlaying &&
               !Extensions.processedEnteringPlaymode)
            {
                Extensions.processedEnteringPlaymode = true;
                Extensions.StartBrowserPlaySession();

                // Fire entering playmode event
                if(Extensions.EnteringPlaymode != null)
                    Extensions.EnteringPlaymode();
            }
            else
            {
                Extensions.processedEnteringPlaymode = false;
            }
        }

        /// <summary>
        /// Starts the browser and connects to AirConsole in order to test the game.
        /// This method should be called when entering playmode.
        /// </summary>
        protected static void StartBrowserPlaySession()
        {
            StringBuilder url = new StringBuilder(128);

            // Load config
            Config.BrowserStart = EditorPrefs.GetInt("BrowserStart", Config.BrowserStart);
            Config.AutoScaleCanvas = EditorPrefs.GetBool("AutoScaleCanvas", Config.AutoScaleCanvas);

            switch((BrowserStartMode)Config.BrowserStart)
            {
                case BrowserStartMode.Standard:
                    url.Append(Config.AirConsolePlayUrl);
                    break;

                case BrowserStartMode.WithVirtualControllers:
                    url.Append(Config.AirConsoleSimulatorUrl);
                    break;

                default:
                    VolplaneController.AirConsole.ProcessData(JSON.Parse(
                        @"{action:""onReady"",code:""0"", devices:[], server_time_offset: 0, device_id: 0, location: """" }"
                    ));
                    return;
            }

            url.AppendFormat("http://{0}:{1:D}/?unity-editor-websocket-port={2:D}&unity-plugin-version={3:G}",
                             GetLocalIPAddress(),
                             Config.LocalServerPort,
                             Config.LocalWebsocketPort,
                             Config.AirConsoleVersion);
            
            Application.OpenURL(url.ToString());
        }

        /// <summary>
        /// Gets the local IP address of this machine.
        /// </summary>
        /// <returns>The local IP address.</returns>
        protected static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            return "?";
        }
    }
}
