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

namespace Volplane
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public abstract class VolplaneBehaviour : MonoBehaviour
    {
        private const int EventCount = 15;

        private bool initialized = false;

        private Type objectType;
        private MethodInfo methodInfo;
        private EventInfo eventInfo;
        private Delegate handler;

        private List<ReflectionEvent> detectedEvents;

        #region Volplane Property Wrappers

        /// <summary>
        /// Gets the AirConsole game code.
        /// </summary>
        /// <value>The game code.</value>
        public string GameCode { get { return VolplaneAgent.GameCode; } }

        /// <summary>
        /// Gets the name of the controller standard view.
        /// </summary>
        /// <value>The standard view.</value>
        public string StandardView { get { return VolplaneAgent.StandardView; } }

        /// <summary>
        /// Gets the number of all currently connected players.
        /// </summary>
        /// <value>The player count.</value>
        public int PlayerCount { get { return VolplaneAgent.PlayerCount; } }

        /// <summary>
        /// Gets the number of active while connected players.
        /// </summary>
        /// <value>The number of active players.</value>
        public static int ActivePlayerCount { get { return VolplaneAgent.ActivePlayerCount; } }

        /// <summary>
        /// Gets the number of inactive while connected players.
        /// </summary>
        /// <value>The number of inactive players.</value>
        public static int InactivePlayerCount { get { return VolplaneAgent.InactivePlayerCount; } }

        #endregion

        #region Volplane Method Wrappers

        /// <summary>
        /// Get a player by its identifier.
        /// </summary>
        /// <returns>The player object.</returns>
        /// <param name="playerId">Player identifier.</param>
        public VPlayer GetPlayer(int playerId)
        {
            return VolplaneController.Main.GetPlayer(playerId);
        }

        /// <summary>
        /// Gets the player identifier.
        /// </summary>
        /// <returns>The player identifier.</returns>
        /// <param name="player">Player object.</param>
        public int GetPlayerId(VPlayer player)
        {
            return VolplaneController.Main.GetPlayerId(player);
        }

        /// <summary>
        /// Gets all connected players.
        /// </summary>
        /// <returns>All players.</returns>
        public IEnumerable<VPlayer> GetAllPlayers()
        {
            return VolplaneController.Main.GetAllPlayers();
        }

        /// <summary>
        /// Gets all active players.
        /// </summary>
        /// <returns>All active players.</returns>
        public IEnumerable<VPlayer> GetAllActivePlayers()
        {
            return VolplaneController.Main.GetAllActivePlayers();
        }

        /// <summary>
        /// Gets all inactive players.
        /// </summary>
        /// <returns>All inactive players.</returns>
        public IEnumerable<VPlayer> GetAllInactivePlayers()
        {
            return VolplaneController.Main.GetAllInactivePlayers();
        }

        /// <summary>
        /// Gets the master player.
        /// </summary>
        /// <returns>A player.</returns>
        public VPlayer GetMaster()
        {
            return VolplaneController.Main.GetMaster();
        }

        /// <summary>
        /// Gets the master player identifier.
        /// </summary>
        /// <returns>The player identifier.</returns>
        public int GetMasterId()
        {
            return VolplaneController.Main.GetMasterId();
        }

        /// <summary>
        /// Sets the specified amount of players active.
        /// This method will pick the earliest connected players.
        /// </summary>
        /// <param name="count">Number of players to pick.</param>
        public void SetPlayersActive(int count)
        {
            VolplaneController.Main.SetPlayersActive(count);
        }

        /// <summary>
        /// Sets all connected players active.
        /// </summary>
        public void SetAllPlayersActive()
        {
            VolplaneController.Main.SetAllPlayersActive();
        }

        /// <summary>
        /// Sets all connected players inactive.
        /// </summary>
        public void SetAllPlayersInactive()
        {
            VolplaneController.Main.SetAllPlayersInactive();
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
            VolplaneController.Main.SetActive(playerId, value);
        }

        /// <summary>
        /// Sets a player active or inactive.
        /// You will not receive any input from inactive players.
        /// </summary>
        /// <remarks>If the player lost connection or is waiting for an advertisement to complete, the state change will
        /// be delayed.</remarks>
        /// <param name="player">Player object.</param>
        /// <param name="value">Activate (<c>true</c>) or deactivate (<c>false</c>) a player.</param>
        public void SetActive(VPlayer player, bool value)
        {
            VolplaneController.Main.SetActive(player, value);
        }

        /// <summary>
        /// Save user data. This method tries to persistently store the data on the AirConsole servers.
        /// When complete, <see cref="Volplane.VolplaneBehaviour.OnUserDataSaved"/> fires for this player.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="data">JSON data.</param>
        public void SaveUserData(int playerId, JObject data)
        {
            VolplaneController.Main.SaveUserData(playerId, data);
        }

        /// <summary>
        /// Save user data. This method tries to persistently store the data on the AirConsole servers.
        /// When complete, <see cref="Volplane.VolplaneBehaviour.OnUserDataSaved"/> fires for this player.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="data">JSON data.</param>
        public void SaveUserData(VPlayer player, JObject data)
        {
            VolplaneController.Main.SaveUserData(player, data);
        }

        /// <summary>
        /// Sets the standard controller view of the players.
        /// </summary>
        /// <param name="viewName">Controller view name.</param>
        public void SetStandardView(string viewName)
        {
            VolplaneController.Main.SetStandardView(viewName);
        }

        /// <summary>
        /// Gets the current view name of a specific controller.
        /// </summary>
        /// <returns>The current view.</returns>
        /// <param name="playerId">Player identifier.</param>
        public string GetCurrentView(int playerId)
        {
            return VolplaneController.Main.GetCurrentView(playerId);
        }

        /// <summary>
        /// Gets the current view name of a specific controller.
        /// </summary>
        /// <returns>The current view.</returns>
        /// <param name="player">Player object.</param>
        public string GetCurrentView(VPlayer player)
        {
            return VolplaneController.Main.GetCurrentView(player);
        }

        /// <summary>
        /// Changes the controller view of a specific player.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="viewName">View name.</param>
        public void ChangeView(int playerId, string viewName)
        {
            VolplaneController.Main.ChangeView(playerId, viewName);
        }

        /// <summary>
        /// Changes the controller view of a specific player.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="viewName">View name.</param>
        public void ChangeView(VPlayer player, string viewName)
        {
            VolplaneController.Main.ChangeView(player, viewName);
        }

        /// <summary>
        /// Changes the controller view for all players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAll(string viewName)
        {
            VolplaneController.Main.ChangeViewAll(viewName);
        }

        /// <summary>
        /// Changes the controller view for all active players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAllActive(string viewName)
        {
            VolplaneController.Main.ChangeViewAllActive(viewName);
        }

        /// <summary>
        /// Changes the controller view for all inactive players.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeViewAllInactive(string viewName)
        {
            VolplaneController.Main.ChangeViewAllInactive(viewName);
        }

        /// <summary>
        /// Resets the controller view of a player to its initial state.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="viewName">View name.</param>
        public void ResetView(int playerId, string viewName)
        {
            VolplaneController.Main.ResetView(playerId, viewName);
        }

        /// <summary>
        /// Resets the controller view of a player to its initial state.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="viewName">View name.</param>
        public void ResetView(VPlayer player, string viewName)
        {
            VolplaneController.Main.ResetView(player, viewName);
        }

        /// <summary>
        /// Enable or disable the tracking of physical motion data of the controller from a
        /// player (acceleration and rotation).
        /// Calling this method has no effect if the 'Track Device Motion' flag is not set for
        /// specified players controller.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="value">If set to <c>true</c> motion data will be tracked.</param>
        public void TrackingControllerMotion(int playerId, bool value)
        {
            VolplaneController.Main.TrackingControllerMotion(playerId, value);
        }

        /// <summary>
        /// Enable or disable the tracking of physical motion data of the controller from a
        /// player (acceleration and rotation).
        /// Calling this method has no effect if the 'Track Device Motion' flag is not set for
        /// specified players controller.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="value">If set to <c>true</c> motion data will be tracked.</param>
        public void TrackingControllerMotion(VPlayer player, bool value)
        {
            VolplaneController.Main.TrackingControllerMotion(player, value);
        }

        /// <summary>
        /// Vibrate the controller of a player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(int playerId, float time)
        {
            VolplaneController.Main.VibrateController(playerId, time);
        }

        /// <summary>
        /// Vibrate the controller of a player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(VPlayer player, float time)
        {
            VolplaneController.Main.VibrateController(player, time);
        }

        /// <summary>
        /// Request showing a multiscreen advertisement.
        /// Call this method at reasonable places / passages in your game. AirConsole will take care on when to show an
        /// advertisement in your game.
        /// </summary>
        public void RequestAd()
        {
            VolplaneController.Main.RequestAd();
        }

        #endregion

        #region Initialization / Subscribe Children Events

        /// <summary>
        /// Unity OnEnable call. Call base method on override.
        /// </summary>
        protected virtual void OnEnable()
        {
            // Not yet initialized?
            if(!initialized)
                Initialize();

            for(int i = 0; i < detectedEvents.Count; i++)
                detectedEvents[i].Add();
        }

        /// <summary>
        /// Unity OnDisable call. Call base method on override.
        /// </summary>
        protected virtual void OnDisable()
        {
            for(int i = 0; i < detectedEvents.Count; i++)
                detectedEvents[i].Remove();
        }

        /// <summary>
        /// Initialize this behaviour.
        /// </summary>
        private void Initialize()
        {
            detectedEvents = new List<ReflectionEvent>(EventCount);
            objectType = this.GetType();

            // Subscribe input events
            SubscribeEvent(VolplaneController.InputHandling, "OnButton");
            SubscribeEvent(VolplaneController.InputHandling, "OnDPad");
            SubscribeEvent(VolplaneController.InputHandling, "OnJoystick");
            SubscribeEvent(VolplaneController.InputHandling, "OnSwipe");
            SubscribeEvent(VolplaneController.InputHandling, "OnTouch");
            SubscribeEvent(VolplaneController.InputHandling, "OnAccelerometer");
            SubscribeEvent(VolplaneController.InputHandling, "OnGyroscope");

            // Subscribe Volplane (AirConsole related) events
            SubscribeEvent(VolplaneController.Main, "OnReady");
            SubscribeEvent(VolplaneController.Main, "OnConnect", "Secondary");
            SubscribeEvent(VolplaneController.Main, "OnDisconnect", "Secondary");
            SubscribeEvent(VolplaneController.Main, "OnHero", "Secondary");
            SubscribeEvent(VolplaneController.Main, "OnAdShow");
            SubscribeEvent(VolplaneController.Main, "OnAdComplete", "Secondary");
            SubscribeEvent(VolplaneController.Main, "OnPlayerProfileChange", "Secondary");
            SubscribeEvent(VolplaneController.Main, "OnUserDataSaved", "Secondary");

            initialized = true;
        }

        /// <summary>
        /// Subscribes this instance to an event through reflection.
        /// This method should not be used on runtime. It is only intended for initialization.
        /// </summary>
        /// <param name="eventHolder">Event holder object.</param>
        /// <param name="name">Event name.</param>
        /// <param name="appendix">Bind a secondary event by its appendix.</param>
        private void SubscribeEvent(object eventHolder, string name, string appendix = null)
        {
            // Get instances event method info
            methodInfo = objectType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            // Method exists?
            if(methodInfo != null)
            {
                // Event from holder element
                eventInfo = eventHolder.GetType().GetEvent(name);

                // Create delegate from child method
                handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo, false);

                // If event delegate parameters matches with the ones from the child method -> add event to collection
                if(handler != null)
                {
                    detectedEvents.Add(new ReflectionEvent(eventHolder, eventInfo, handler));
                }
                else if(appendix != null)
                {
                    // Secondary event from holder element
                    eventInfo = eventHolder.GetType().GetEvent(name + appendix);

                    // Create delegate from child method
                    handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo, false);

                    // If event delegate parameters matches with the ones from the child method -> add event to collection
                    if(handler != null)
                        detectedEvents.Add(new ReflectionEvent(eventHolder, eventInfo, handler));
                }
            }
        }

        #endregion
    }
}
