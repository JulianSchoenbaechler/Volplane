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
            // Standard values
            this.oldPlayerState = VolplaneController.AirConsole.GetMasterControllerDeviceId() == acDeviceId ? 
                PlayerState.Active :
                PlayerState.Inactive;
            this.currentPlayerState = this.oldPlayerState;

            this.DeviceId = acDeviceId;
            this.IsUsingBrowser = true;
            this.IsConnected = true;
            this.IsHero = VolplaneController.AirConsole.IsPremium(acDeviceId);
            this.IsLoggedIn = VolplaneController.AirConsole.IsUserLoggedIn(acDeviceId);
            this.HasSlowConnection = false;
            this.UID = VolplaneController.AirConsole.GetUID(acDeviceId);
            this.Nickname = VolplaneController.AirConsole.GetNickname(acDeviceId);
            this.ProfilePicture = null;

            // Change to standard view
            this.ChangeView(VolplaneAgent.StandardView);

            // Subscribe events
            VolplaneController.AirConsole.OnConnect += Connect;
            VolplaneController.AirConsole.OnDisconnect += Disconnect;
            VolplaneController.AirConsole.OnPremium += Hero;
            VolplaneController.AirConsole.OnDeviceStateChange += UpdateSettings;
            VolplaneController.AirConsole.OnDeviceProfileChange += UpdateProfile;
            VolplaneController.AirConsole.OnAdShow += WaitForAd;
            VolplaneController.AirConsole.OnAdComplete += AdCompleted;
        }

        #region Player Events

        /// <summary>
        /// Occurs when player state changes.
        /// </summary>
        public event Action<bool> OnStateChange;

        #endregion

        /// <summary>
        /// Player state.
        /// </summary>
        public enum PlayerState
        {
            Inactive,
            Active,
            Pending,
            WaitingForAd
        }

        #region Player Properties

        /// <summary>
        /// Gets the player identifier.
        /// </summary>
        /// <value>The player identifier.</value>
        public int PlayerId
        {
            get { return VolplaneController.Main.GetPlayerId(this); }
        }

        /// <summary>
        /// Gets the current player state.
        /// </summary>
        /// <value>The current player state.</value>
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
                if(OnStateChange != null)
                    OnStateChange(value == PlayerState.Active);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this player is active.
        /// </summary>
        /// <value><c>true</c> if this player is active; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets the current view displayed on this players controller.
        /// </summary>
        /// <value>The current view.</value>
        public string CurrentView
        {
            get { return VolplaneController.Main.GetCurrentView(this); }
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        public int DeviceId { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this player is connected through a browser.
        /// </summary>
        /// <value><c>true</c> if this player is connected through a browser; otherwise, <c>false</c>.</value>
        public bool IsUsingBrowser { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this player is connected.
        /// </summary>
        /// <value><c>true</c> if this player is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this player purchased AirConsole Hero.
        /// </summary>
        /// <value><c>true</c> if this player is Hero; otherwise, <c>false</c>.</value>
        public bool IsHero { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this player is logged in.
        /// </summary>
        /// <value><c>true</c> if this player is logged in; otherwise, <c>false</c>.</value>
        public bool IsLoggedIn { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this player has s slow connection.
        /// </summary>
        /// <value><c>true</c> if this player has a slow connection; otherwise, <c>false</c>.</value>
        public bool HasSlowConnection { get; protected set; }

        /// <summary>
        /// Gets the players unique identifier.
        /// </summary>
        /// <value>The players unique identifier.</value>
        public string UID { get; protected set; }

        /// <summary>
        /// Gets the players nickname.
        /// </summary>
        /// <value>The nickname.</value>
        public string Nickname { get; protected set; }

        /// <summary>
        /// Gets the profile picture. Make sure you have previously loaded the profile picture with
        /// <see cref="Volplane.VPlayer.LoadProfilePicture(int)"/>.
        /// </summary>
        /// <value>The profile picture.</value>
        public Texture2D ProfilePicture { get; protected set; }

        #endregion

        #region Player Specific Methods

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

        #endregion

        #region Player Controller View Management

        /// <summary>
        /// Changes the controller view of this player.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ChangeView(string viewName)
        {
            VolplaneController.Main.ChangeView(this, viewName);
        }

        /// <summary>
        /// Resets the controller view of this player to its initial state.
        /// </summary>
        /// <param name="viewName">View name.</param>
        public void ResetView(string viewName)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "reset";
            data["volplane"]["name"] = viewName;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        #endregion

        #region Player Controller Elements / Function Management

        /// <summary>
        /// Hides and disables an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void HideElement(string elementName)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"]["hidden"] = true;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Shows and enables a hidden element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void ShowElement(string elementName)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"]["hidden"] = false;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Toggles the visibility of an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void ToggleElement(string elementName)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"]["toggle"] = true;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Changes the text of an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="text">New text.</param>
        public void ChangeElementText(string elementName, string text)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"]["text"] = text;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Changes the background image of an element on the current view.
        /// Specified image must exist in the 'img' folder in the WebGL template:
        /// 'Assets/WebGLTemplates/Volplane/img/'
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="image">The image name (including extension).</param>
        public void ChangeElementImage(string elementName, string image)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"]["image"] = String.Format("img/{0:G}", image);

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        /// <summary>
        /// Changes multiple properties from an element on the current view at the same time.
        /// Use this method for more advanced element manipulation.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="properties">Properties.</param>
        public void ChangeElementProperties(string elementName, ElementProperties properties)
        {
            JSONNode data = new JSONObject();
            data["volplane"]["action"] = "element";
            data["volplane"]["name"] = elementName;
            data["volplane"]["properties"] = properties.Data;

            VolplaneController.AirConsole.Message(DeviceId, data);
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
            data["volplane"]["action"] = "vibrate";
            data["volplane"]["time"] = milliseconds;

            VolplaneController.AirConsole.Message(DeviceId, data);
        }

        #endregion

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
            VolplaneController.AirConsole.OnConnect -= Connect;
            VolplaneController.AirConsole.OnDisconnect -= Disconnect;
            VolplaneController.AirConsole.OnPremium -= Hero;
            VolplaneController.AirConsole.OnDeviceStateChange -= UpdateSettings;
            VolplaneController.AirConsole.OnDeviceProfileChange -= UpdateProfile;
            VolplaneController.AirConsole.OnAdShow -= WaitForAd;
            VolplaneController.AirConsole.OnAdComplete -= AdCompleted;

            // Unassign won events
            OnStateChange = null;
        }

        #region Player Data Processing

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

        #endregion
    }
}
