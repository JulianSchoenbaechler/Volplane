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
    using UnityEngine;

    public class VPlayer : IDisposable
    {

        protected PlayerState oldPlayerState, currentPlayerState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.VPlayer"/> class.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public VPlayer(int acDeviceId)
        {
            this.oldPlayerState = VolplaneController.AirConsole.GetMasterControllerDeviceId() == acDeviceId ? 
                PlayerState.Active :
                PlayerState.Inactive;
            this.currentPlayerState = this.oldPlayerState;

            this.DeviceId = acDeviceId;
            this.IsUsingBrowser = true;
            this.IsConnected = true;
            this.IsHero = VolplaneController.AirConsole.IsPremium(acDeviceId);
            this.HasSlowConnection = false;
            this.UID = VolplaneController.AirConsole.GetUID(acDeviceId);
            this.Nickname = VolplaneController.AirConsole.GetNickname(acDeviceId);
            this.ProfilePicture = null;
            this.Email = null;

            // Change to standard view
            this.ChangeView(VolplaneAgent.StandardView);

            // Subscribe events
            VolplaneController.AirConsole.onConnect += Connect;
            VolplaneController.AirConsole.onDisconnect += Disconnect;
            VolplaneController.AirConsole.onPremium += Hero;
            VolplaneController.AirConsole.onDeviceStateChange += UpdateSettings;
            VolplaneController.AirConsole.onDeviceProfileChange += UpdateProfile;
            VolplaneController.AirConsole.onAdShow += WaitForAd;
            VolplaneController.AirConsole.onAdComplete += AdCompleted;
        }

        public event Action<bool> stateChangeEvent;

        public enum PlayerState
        {
            Inactive,
            Active,
            Pending,
            WaitingForAd
        }

        public int PlayerId
        {
            get { return VolplaneController.Main.GetPlayerId(this); }
        }

        public PlayerState State
        {
            get
            {
                return currentPlayerState;
            }

            protected set
            {
                // Only overwrite old state if new state is not -> pending or waiting
                if((currentPlayerState != PlayerState.Pending) &&
                   (currentPlayerState != PlayerState.WaitingForAd))
                    oldPlayerState = currentPlayerState;
                
                currentPlayerState = value;

                // Fire state change event
                if(stateChangeEvent != null)
                    stateChangeEvent(value == PlayerState.Active);
            }
        }

        public bool IsActive
        {
            get
            {
                if((currentPlayerState != PlayerState.Pending) &&
                   (currentPlayerState != PlayerState.WaitingForAd))
                {
                    return currentPlayerState == PlayerState.Active;
                }
                else
                {
                    return oldPlayerState == PlayerState.Active;
                }
            }
        }

        public string CurrentView
        {
            get { return VolplaneController.Main.GetCurrentView(this); }
        }

        public int DeviceId { get; protected set; }
        public bool IsUsingBrowser { get; protected set; }
        public bool IsConnected { get; protected set; }
        public bool IsHero { get; protected set; }
        public bool HasSlowConnection { get; protected set; }
        public string UID { get; protected set; }
        public string Nickname { get; protected set; }
        public Texture2D ProfilePicture { get; protected set; }
        public string Email { get; protected set; }


        /// <summary>
        /// Loads the profile picture of this player.
        /// This method must be used as a Unity coroutine.
        /// </summary>
        /// <param name="size">Size of the profile picture (aspect ratio 1/1).</param>
        public IEnumerator LoadProfilePicture(int size = 256)
        {
            WWW www = new WWW(VolplaneController.AirConsole.GetProfilePicture(UID, size));

            yield return www;
            ProfilePicture = www.texture;
            www.Dispose();
            www = null;
        }

        public void RequestEmailAddress()
        {
            JSONNode data = new JSONObject();
            data["volplane"] = "email";

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Sets this player active or inactive.
        /// You will not receive any input from inactive players.
        /// </summary>
        /// <remarks>If the player lost connection or is waiting for an advertisement to complete, the state change will
        /// be delayed.</remarks>
        /// <param name="value">Activate (<c>true</c>) or deactivate (<c>false</c>) this player.</param>
        public void SetActive(bool value)
        {
            if((currentPlayerState != PlayerState.Pending) &&
               (currentPlayerState != PlayerState.WaitingForAd))
            {
                // Set new player state and fire event
                State = value ? PlayerState.Active : PlayerState.Inactive;
            }
            else
            {
                // Overwrite old state
                // After 'WaitingForAd' or 'Pending', old state will be reloaded
                oldPlayerState = value ? PlayerState.Active : PlayerState.Inactive;
            }
        }

        /// <summary>
        /// Changes the controller view of this player.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeView(string viewName)
        {
            VolplaneController.Main.ChangeView(this, viewName);
        }

        /// <summary>
        /// Vibrate the controller of this player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(float time)
        {
            int milliseconds = time > 10f ? 10000 : (int)(time * 1000f);

            JSONNode data = new JSONObject();
            data["volplane"] = "vibrate";
            data["time"] = milliseconds;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Volplane.VPlayer"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="Volplane.VPlayer"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="Volplane.VPlayer"/> so the garbage
        /// collector can reclaim the memory that the <see cref="Volplane.VPlayer"/> was occupying.</remarks>
        public void Dispose()
        {
            // Unsubscribe all events
            VolplaneController.AirConsole.onConnect -= Connect;
            VolplaneController.AirConsole.onDisconnect -= Disconnect;
            VolplaneController.AirConsole.onPremium -= Hero;
            VolplaneController.AirConsole.onDeviceStateChange -= UpdateSettings;
            VolplaneController.AirConsole.onDeviceProfileChange -= UpdateProfile;
            VolplaneController.AirConsole.onAdShow -= WaitForAd;
            VolplaneController.AirConsole.onAdComplete -= AdCompleted;
        }


        /// <summary>
        /// When this player device connects.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void Connect(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsConnected = true;
                State = oldPlayerState;
            }
        }

        /// <summary>
        /// When this player device disconnects.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void Disconnect(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsConnected = false;
                State = PlayerState.Pending;
            }
        }

        /// <summary>
        /// When this player device becomes AirConsole Hero.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void Hero(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsHero = true;
            }
        }

        /// <summary>
        /// When this player device state changes.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        /// <param name="data">Device state data.</param>
        protected void UpdateSettings(int acDeviceId, JSONNode data)
        {
            if(data == null)
                return;
            Debug.Log(data.ToString(2));
            if(acDeviceId == DeviceId)
            {
                UID = data["uid"].Value;
                IsUsingBrowser = data["client"]["app"].Value == "web";
                HasSlowConnection = data["slow_connection"].AsBool;
            }
        }

        /// <summary>
        /// When this player device updates its nickname.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected void UpdateProfile(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                Nickname = VolplaneController.AirConsole.GetNickname(acDeviceId);
            }
        }

        /// <summary>
        /// When advertising is displayed.
        /// </summary>
        protected void WaitForAd()
        {
            State = PlayerState.WaitingForAd;
        }

        /// <summary>
        /// When advertising has been displayed.
        /// </summary>
        protected void AdCompleted(bool complete)
        {
            State = oldPlayerState;
        }
    }
}
