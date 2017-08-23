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
    using SimpleJSON;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Volplane.AirConsole;

    public class VolplaneAgent : IDisposable
    {
        // Main player list
        // This list indices can be hardcoded
        protected static List<VPlayer> Players;
        protected static JSONNode CustomState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.PlayerManager"/> class.
        /// </summary>
        public VolplaneAgent()
        {
            if(VolplaneAgent.CustomState == null)
                VolplaneAgent.CustomState = new JSONObject();

            VolplaneController.AirConsole.OnConnect += AddPlayer;
        }

        public static string StandardView { get; set; }

        /// <summary>
        /// Get a player by its identifier.
        /// </summary>
        /// <returns>The player object.</returns>
        /// <param name="playerId">Player identifier.</param>
        public VPlayer GetPlayer(int playerId)
        {
            if(VolplaneAgent.Players != null)
            {
                if(playerId < VolplaneAgent.Players.Count)
                    return VolplaneAgent.Players[playerId];
            }

            return null;
        }

        /// <summary>
        /// Gets the player identifier.
        /// </summary>
        /// <returns>The player identifier.</returns>
        /// <param name="player">Player object.</param>
        public int GetPlayerId(VPlayer player)
        {
            if(VolplaneAgent.Players != null)
                return VolplaneAgent.Players.FindIndex(vp => vp.DeviceId == player.DeviceId);

            return -1;
        }

        /// <summary>
        /// Sets the specified amount of players active.
        /// This method will pick the earliest connected players.
        /// </summary>
        /// <param name="count">Number of players to pick.</param>
        public void SetPlayersActive(int count)
        {
            for(int i = 0; i < VolplaneAgent.Players.Count; i++)
            {
                if(!VolplaneAgent.Players[i].IsConnected)
                    continue;
                
                if(i < count)
                    VolplaneAgent.Players[i].SetActive(true);
                else
                    VolplaneAgent.Players[i].SetActive(false);
            }
        }

        /// <summary>
        /// Sets all connected players active.
        /// </summary>
        public void SetAllPlayersActive()
        {
            for(int i = 0; i < VolplaneAgent.Players.Count; i++)
            {
                if(VolplaneAgent.Players[i].IsConnected)
                    VolplaneAgent.Players[i].SetActive(true);
            }
        }

        /// <summary>
        /// Sets a player active or inactive.
        /// You will not receive any input from inactive players.
        /// </summary>
        /// <remarks>If the player lost connection or is waiting for an advertisement to complete, the state change will
        /// be delayed.</remarks>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="value">Activate (<c>true</c>) or deactivate (<c>false</c>) a player.</param>
        public void SetActive(int playerId, bool value)
        {
            SetActive(GetPlayer(playerId), value);
        }

        /// <summary>
        /// Sets a player active or inactive.
        /// You will not receive any input from inactive players.
        /// </summary>
        /// <remarks>If the player lost connection or is waiting for an advertisement to complete, the state change will
        /// be delayed.</remarks>
        /// <param name="playerId">Player object.</param>
        /// <param name="value">Activate (<c>true</c>) or deactivate (<c>false</c>) a player.</param>
        public void SetActive(VPlayer player, bool value)
        {
            if(player != null)
                player.SetActive(value);
        }

        /// <summary>
        /// Gets the current view name of a specific controller.
        /// </summary>
        /// <returns>The current view.</returns>
        /// <param name="playerId">Player identifier.</param>
        public string GetCurrentView(int playerId)
        {
            return GetCurrentView(GetPlayer(playerId));
        }

        /// <summary>
        /// Gets the current view name of a specific controller.
        /// </summary>
        /// <returns>The current view.</returns>
        /// <param name="player">Player object.</param>
        public string GetCurrentView(VPlayer player)
        {
            if(player != null)
                return VolplaneAgent.CustomState["views"][player.DeviceId];

            return null;
        }

        /// <summary>
        /// Changes the controller view of a specific player.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="viewName">View name.</param>
        public void ChangeView(int playerId, string viewName)
        {
            ChangeView(GetPlayer(playerId), viewName);
        }

        /// <summary>
        /// Changes the controller view of a specific player.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="viewName">View name.</param>
        public void ChangeView(VPlayer player, string viewName)
        {
            if(player != null)
            {
                VolplaneAgent.CustomState["views"][player.DeviceId] = viewName;
                VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
            }
        }

        /// <summary>
        /// Changes the controller view for all players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAll(string viewName)
        {
            // Iterate through all players
            for(int i = 0; i < VolplaneAgent.Players.Count; i++)
            {
                // Change all views
                VolplaneAgent.CustomState["views"][VolplaneAgent.Players[i].DeviceId] = viewName;
            }

            // Set views
            VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
        }

        /// <summary>
        /// Changes the controller view for all active players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAllActive(string viewName)
        {
            // Iterate through all players
            for(int i = 0; i < VolplaneAgent.Players.Count; i++)
            {
                // Change only the views from active players
                if(VolplaneAgent.Players[i].IsActive)
                    VolplaneAgent.CustomState["views"][VolplaneAgent.Players[i].DeviceId] = viewName;
            }

            // Set views
            VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
        }

        /// <summary>
        /// Changes the controller view for all inactive players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAllInactive(string viewName)
        {
            // Iterate through all players
            for(int i = 0; i < VolplaneAgent.Players.Count; i++)
            {
                // Change only the views from active players
                if(!VolplaneAgent.Players[i].IsActive)
                    VolplaneAgent.CustomState["views"][VolplaneAgent.Players[i].DeviceId] = viewName;
            }

            // Set views
            VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
        }

        /// <summary>
        /// Resets the controller view of a player to its initial state.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="viewName">View name.</param>
        public void ResetView(int playerId, string viewName)
        {
            ResetView(GetPlayer(playerId), viewName);
        }

        /// <summary>
        /// Resets the controller view of a player to its initial state.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="viewName">View name.</param>
        public void ResetView(VPlayer player, string viewName)
        {
            if(player != null)
                player.ResetView(viewName);
        }

        /// <summary>
        /// Vibrate the controller of a player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(int playerId, float time)
        {
            VibrateController(GetPlayer(playerId), time);
        }

        /// <summary>
        /// Vibrate the controller of a player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(VPlayer player, float time)
        {
            if(player != null)
                player.VibrateController(time);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Volplane.VolplaneAgent"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="Volplane.VolplaneAgent"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="Volplane.VolplaneAgent"/>
        /// so the garbage collector can reclaim the memory that the <see cref="Volplane.VolplaneAgent"/> was occupying.</remarks>
        public void Dispose()
        {
            if(VolplaneController.AirConsole != null)
                VolplaneController.AirConsole.OnConnect -= AddPlayer;
        }



        /// <summary>
        /// Gets the player identifier.
        /// </summary>
        /// <returns>The player identifier.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected int GetPlayerId(int acDeviceId)
        {
            if(VolplaneAgent.Players != null)
                return VolplaneAgent.Players.FindIndex(vp => vp.DeviceId == acDeviceId);

            return -1;
        }

        /// <summary>
        /// Allocates fields of custom device state arrays.
        /// </summary>
        /// <remarks>Device identifiers will be saved as array keys. Therefore, the array must have a size of at least the number of
        /// the greatest device identifier + 1. This method should be called everytime a new device connects to keep the arrays
        /// allocated with enough space for every device.</remarks>
        /// <param name="acDeviceId">Ac device identifier.</param>
        protected void AllocateCustomStateArrays(int acDeviceId)
        {
            // Number of fields to allocate
            int diffDeviceId = acDeviceId - VolplaneAgent.CustomState["views"].Count;

            // View management
            for(int i = 0; i <= diffDeviceId; i++)
                VolplaneAgent.CustomState["views"][-1] = VolplaneAgent.StandardView;

            // State management
            for(int i = 0; i <= diffDeviceId; i++)
                VolplaneAgent.CustomState["active"][-1] = false;
        }

        /// <summary>
        /// Adds a new player to the list.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void AddPlayer(int acDeviceId)
        {
            if(acDeviceId < 1)
                return;

            if(VolplaneAgent.Players != null)
            {
                int index = GetPlayerId(acDeviceId);

                // Player does not exist yet
                if(index == -1)
                {
                    AllocateCustomStateArrays(acDeviceId);

                    // Create new player and subscribe state change event for updating custom device state
                    VPlayer newPlayer = new VPlayer(acDeviceId);
                    Action<bool> updateState = delegate(bool active) {
                        VolplaneAgent.CustomState["active"][acDeviceId] = active;
                        VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
                    };
                    newPlayer.OnStateChange += updateState;

                    // Invoke 'updateState' delegate for initialization
                    updateState(newPlayer.State == VPlayer.PlayerState.Active);

                    // Add player to player list
                    VolplaneAgent.Players.Add(newPlayer);
                }
            }
            else
            {
                AllocateCustomStateArrays(acDeviceId);

                // Create new player and subscribe state change event for updating custom device state
                VPlayer newPlayer = new VPlayer(acDeviceId);
                Action<bool> updateState = delegate(bool active) {
                    VolplaneAgent.CustomState["active"][acDeviceId] = active;
                    VolplaneController.AirConsole.SetCustomDeviceStateProperty("volplane", VolplaneAgent.CustomState);
                };
                newPlayer.OnStateChange += updateState;

                // Invoke 'updateState' delegate for initialization
                updateState(newPlayer.State == VPlayer.PlayerState.Active);

                // Create player list and add player
                VolplaneAgent.Players = new List<VPlayer>(8);
                VolplaneAgent.Players.Add(newPlayer);
            }
        }
    }
}
