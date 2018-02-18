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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Net.NetworkInformation;
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

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged -= PlaymodeStateChanged;
            EditorApplication.playModeStateChanged += PlaymodeStateChanged;
#else
            EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif
        }

        /// <summary>
        /// Occurs when entering playmode.
        /// </summary>
        public static event Action EnteringPlaymode;

        /// <summary>
        /// Opens the builded project in the AirConsole simulator.
        /// </summary>
        public static void OpenBuild()
        {
            if((Config.BuildPath != null) && (Config.BuildPath.Length > 0))
            {
                Application.OpenURL(
                    String.Format("{0:G}http://{1:G}:{2:D}/build/",
                                  Config.AirConsolePlayUrl,
                                  Config.LocalIPv4,
                                  Config.LocalServerPort)
                );
            }
        }

        /// <summary>
        /// Gets all the local IPv4 addresses of this machine.
        /// </summary>
        /// <returns>A local IPv4 address collection.</returns>
        public static ICollection<string> GetLocalIPAddresses()
        {
            ICollection<string> collection = new List<string>();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach(NetworkInterface network in networkInterfaces)
            {

#if UNITY_EDITOR_WIN

                // Skip disconnected network interfaces
                if(network.OperationalStatus != OperationalStatus.Up)
                    continue;

#endif

                // Read the IP configuration for each network
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses
                foreach(IPAddressInformation address in properties.UnicastAddresses)
                {
                    // Only IPv4 local network
                    if(address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses
                    if(IPAddress.IsLoopback(address.Address))
                        continue;

                    // Add address to collection
                    collection.Add(address.Address.ToString());
                }
            }

            return collection;
        }

        /// <summary>
        /// Creates a Volplane controller instance.
        /// </summary>
        [MenuItem("Assets/Create/Volplane Controller")]
        [MenuItem("GameObject/Create Other/Volplane Controller")]
        protected static void CreateVolplaneInstance()
        {
            VolplaneController ctrl = GameObject.FindObjectOfType<VolplaneController>();

            if(ctrl == null)
            {
                GameObject obj = new GameObject("Volplane", new[] { typeof(VolplaneController) });
                obj.tag = "Volplane";
                EditorGUIUtility.PingObject(obj.GetInstanceID());
            }
            else
            {
                EditorUtility.DisplayDialog("Already exists", "Volplane instance already exists in the current scene", "OK");
                EditorGUIUtility.PingObject(ctrl.GetInstanceID());
            }
        }

        /// <summary>
        /// Loads the Volplane configuration preferences.
        /// </summary>
        protected static void LoadSettings()
        {
            // Load saved editor preferences
            IList<string> IPv4Adresses = GetLocalIPAddresses() as IList<string>;
            int selectedIP = EditorPrefs.GetInt("SelectedLocalIPv4", 0);

            if(IPv4Adresses.Count > selectedIP)
                Config.LocalIPv4 = IPv4Adresses[selectedIP];
            else
                Config.LocalIPv4 = EditorPrefs.GetString("LocalIPv4", "0.0.0.0");

            Config.LocalServerPort = EditorPrefs.GetInt("LocalServerPort", Config.DefaultLocalServerPort);
            Config.LocalWebsocketPort = EditorPrefs.GetInt("LocalWebsocketPort", Config.DefaultLocalWebsocketPort);
            Config.DebugLog = EditorPrefs.GetInt("DebugLog", (int)DebugState.None);
            Config.DebugMessages = EditorPrefs.GetBool("DebugMessages", false);
            Config.DebugWarnings = EditorPrefs.GetBool("DebugWarnings", true);
            Config.DebugErrors = EditorPrefs.GetBool("DebugErrors", true);
            Config.BrowserStart = EditorPrefs.GetInt("BrowserStart", (int)BrowserStartMode.Standard);
            Config.AutoScaleCanvas = EditorPrefs.GetBool("AutoScaleCanvas", true);
            Config.SelectedController = EditorPrefs.GetString("SelectedController", null);
            Config.BuildPath = EditorPrefs.GetString("BuildPath", null);
        }

        /// <summary>
        /// Specifies the correct index file for the WebGL template according to the Unity version.
        /// </summary>
        protected static void SetupWebGLTemplate()
        {
            string savedIndex = EditorPrefs.GetString("TemplateIndex", null);

            if(!File.Exists(String.Format("{0:G}{1:G}/index.html", Application.dataPath, Config.WebTemplatePath)))
                savedIndex = "";

            if(savedIndex.Length == 0)
            {
#if UNITY_5_6_OR_NEWER
                savedIndex = "5-6";
#else
                savedIndex = "pre-5-6";
#endif

                EditorPrefs.SetString("TemplateIndex", savedIndex);

                // Copy specified index file for web template
                File.Copy(
                    String.Format("{0:G}{1:G}/{2:G}.html", Application.dataPath, Config.WebTemplateIndexPath, savedIndex),
                    String.Format("{0:G}{1:G}/index.html", Application.dataPath, Config.WebTemplatePath),
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
                    String.Format("{0:G}{1:G}/{2:G}.html", Application.dataPath, Config.WebTemplateIndexPath, savedIndex),
                    String.Format("{0:G}{1:G}/index.html", Application.dataPath, Config.WebTemplatePath),
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
                    String.Format("{0:G}{1:G}/{2:G}.html", Application.dataPath, Config.WebTemplateIndexPath, savedIndex),
                    String.Format("{0:G}{1:G}/index.html", Application.dataPath, Config.WebTemplatePath),
                    true
                );
            }
#endif
        }

        /// <summary>
        /// Playmode state change listener.
        /// Firing entering playmode event.
        /// </summary>
#if UNITY_2017_2_OR_NEWER
        protected static void PlaymodeStateChanged(PlayModeStateChange state)
        {
            if(GameObject.FindWithTag("Volplane") == null)
                return;

            if(state == PlayModeStateChange.EnteredPlayMode)
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
#else
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
#endif

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
                    VolplaneController.AirConsole.ProcessData(
                        @"{action:""onReady"",code:""0"", devices:[], server_time_offset: 0, device_id: 0, location: """" }"
                    );
                    return;
            }

            url.AppendFormat("http://{0}:{1:D}/?unity-editor-websocket-port={2:D}&unity-plugin-version={3:G}",
                             Config.LocalIPv4,
                             Config.LocalServerPort,
                             Config.LocalWebsocketPort,
                             Config.AirConsoleVersion);

            Application.OpenURL(url.ToString());
        }
    }
}
