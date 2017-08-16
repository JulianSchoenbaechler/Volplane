﻿/*
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
    public enum DebugState
    {
        None,
        Connection,
        All
    }

    public enum BrowserStartMode
    {
        Standard,
        WithVirtualControllers,
        NoBrowserStart
    }

    public static class Config
    {
        public const string Version                         = "v0.0.0";
        public const string AirConsoleVersion               = "1.7";

        public static int BrowserStart                      = (int)BrowserStartMode.Standard;
        public static bool AutoScaleCanvas                  = true;
        public static string SelectedController             = null;

        public const int DefaultLocalServerPort             = 7860;
        public const int DefaultLocalWebsocketPort          = 7861;
        public static int LocalServerPort                   = 7860;
        public static int LocalWebsocketPort                = 7861;

        public const string WebServerPath                   = "/Volplane/WebServer";
        public const string WebTemplatePath                 = "/WebGLTemplates/Volplane";
        public const string WebsocketVirtualPath            = "/Volplane";

        public const string VolplaneUrl                     = "https://volplane.julian-s.ch/";
        public const string AirConsoleUrl                   = "https://www.airconsole.com/";
        public const string AirConsolePlayUrl               = "http://www.airconsole.com/#";
        public const string AirConsoleSimulatorUrl          = "http://www.airconsole.com/simulator/#";
        public const string AirConsoleProfilePictureUrl     = "https://www.airconsole.com/api/profile-picture?uid=";

        public static int DebugLog                          = (int)DebugState.None;
    }
}
