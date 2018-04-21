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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class VPlayer : IDisposable
    {
        protected PlayerState oldPlayerState, currentPlayerState;
        protected StringBuilder sendData;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.VPlayer"/> class.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        public VPlayer(int acDeviceId)
        {
            // Allocate memory for JSON data string
            sendData = new StringBuilder(512);

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
            this.UserData = null;

            // Change to standard view
            this.ChangeView(VolplaneAgent.StandardView);

            // Subscribe events
            VolplaneController.AirConsole.OnConnect += Connect;
            VolplaneController.AirConsole.OnDisconnect += Disconnect;
            VolplaneController.AirConsole.OnPremium += Hero;
            VolplaneController.AirConsole.OnDeviceStateChange += UpdateSettings;
            VolplaneController.AirConsole.OnAdShow += WaitForAd;
            VolplaneController.AirConsole.OnAdComplete += AdCompleted;
            VolplaneController.AirConsole.OnPersistentDataLoaded += GetUserData;

            // Request persistent data
            if(VolplaneAgent.SyncUserData)
                VolplaneController.AirConsole.RequestPersistentData(UID);
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
            get
            {
                string view = VolplaneController.Main.GetCurrentView(this);

                return view.Length > 0 ? view : null;
            }
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

        /// <summary>
        /// Gets the persistent user data loaded from the AirConsole servers.
        /// </summary>
        /// <value>The user data.</value>
        public JObject UserData { get; protected set; }

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

        /// <summary>
        /// Save user data. This method tries to persistently store the data on the AirConsole servers.
        /// When complete, <see cref="Volplane.VolplaneBehaviour.OnUserDataSaved"/> fires for this player.
        /// </summary>
        /// <param name="data">New user data as JObject.</param>
        public void SaveUserData(JObject data)
        {
            if(data == null)
                return;

            UserData = data;

            if(VolplaneAgent.SyncUserData)
            {
                foreach(var kvp in data)
                {
                    VolplaneController.AirConsole.StorePersistentData(kvp.Key, kvp.Value.ToString(), UID);
                }
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
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("reset");
                writer.WritePropertyName("name");
                writer.WriteValue(viewName);
                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Changes the color of a controller view.
        /// </summary>
        /// <param name="viewName">View name.</param>
        /// <param name="color">New color.</param>
        public void ChangeViewColor(string viewName, Color color)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("view");
                writer.WritePropertyName("name");
                writer.WriteValue(viewName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("color");
                writer.WriteValue(String.Format(
                    "rgb({0:F0}, {1:F0}, {2:F0})",
                    color.r * 255f,
                    color.g * 255f,
                    color.b * 255f
                ));
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Changes the background image of a controller view.
        /// Specified image must exist in the 'img' folder in the WebGL template:
        /// 'Assets/WebGLTemplates/Volplane/img/'
        /// </summary>
        /// <param name="viewName">View name.</param>
        /// <param name="image">The image name (including extension).</param>
        public void ChangeViewImage(string viewName, string image)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("view");
                writer.WritePropertyName("name");
                writer.WriteValue(viewName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("image");
                writer.WriteValue(String.Format("img/{0:G}", image));
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        #endregion

        #region Player Controller Elements / Function Management

        /// <summary>
        /// Hides and disables an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void HideElement(string elementName)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("hidden");
                writer.WriteValue(true);
                writer.WriteEndObject();

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Shows and enables a hidden element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void ShowElement(string elementName)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("hidden");
                writer.WriteValue(false);
                writer.WriteEndObject();

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Toggles the visibility of an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        public void ToggleElement(string elementName)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("toggle");
                writer.WriteValue(true);
                writer.WriteEndObject();

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Changes the text of an element on the current view.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="text">New text.</param>
        public void ChangeElementText(string elementName, string text)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("text");
                writer.WriteValue(text);
                writer.WriteEndObject();

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
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
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteStartObject();
                writer.WritePropertyName("image");
                writer.WriteValue(String.Format("img/{0:G}", image));
                writer.WriteEndObject();

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Changes multiple properties from an element on the current view at the same time.
        /// Use this method for more advanced element manipulation.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="properties">Properties.</param>
        public void ChangeElementProperties(string elementName, ElementProperties properties)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("element");
                writer.WritePropertyName("name");
                writer.WriteValue(elementName);
                writer.WritePropertyName("properties");
                writer.WriteRawValue(properties.Data);

                if(CurrentView != null)
                {
                    writer.WritePropertyName("view");
                    writer.WriteValue(CurrentView);
                }

                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Enable or disable the tracking of physical motion data of the controller from this
        /// player (acceleration and rotation).
        /// Calling this method has no effect if the 'Track Device Motion' flag is not set for
        /// this players controller.
        /// </summary>
        /// <param name="value">If set to <c>true</c> motion data will be tracked.</param>
        public void TrackingControllerMotion(bool value)
        {
            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("deviceMotion");
                writer.WritePropertyName("enable");
                writer.WriteValue(value);
                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
        }

        /// <summary>
        /// Vibrate the controller of this player for a specified amount of time.
        /// Maximum time is 10 seconds.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        public void VibrateController(float time)
        {
            int milliseconds = time > 10f ? 10000 : (int)(time * 1000f);

            sendData.Length = 0;

            using(var sw = new StringWriter(sendData))
            using(var writer = new JsonTextWriter(sw))
            {
                // Use buffer
                writer.ArrayPool = JSONArrayPool.Instance;

                writer.WriteStartObject();
                writer.WritePropertyName("volplane");
                writer.WriteStartObject();
                writer.WritePropertyName("action");
                writer.WriteValue("vibrate");
                writer.WritePropertyName("time");
                writer.WriteValue(milliseconds);
                writer.WriteEndObject();
                writer.WriteEnd();
            }

            VolplaneController.AirConsole.Message(DeviceId, sendData.ToString());
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
            VolplaneController.AirConsole.OnAdShow -= WaitForAd;
            VolplaneController.AirConsole.OnAdComplete -= AdCompleted;

            // Unassign events
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
                IsHero = true;
        }

        /// <summary>
        /// When this player device state changes.
        /// </summary>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        /// <param name="data">Device state data.</param>
        protected void UpdateSettings(int acDeviceId, Volplane.AirConsole.AirConsoleAgent.Device data)
        {
            if(data == null)
                return;

            if(acDeviceId == DeviceId)
            {
                UID = data.UID;
                IsUsingBrowser = data.IsUsingBrowser;
                HasSlowConnection = data.HasSlowConnection;
                Nickname = data.Nickname;
                IsLoggedIn = data.IsLoggedIn;
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

        /// <summary>
        /// When requested persistent data was received.
        /// </summary>
        /// <param name="data">Persistent user data.</param>
        protected void GetUserData(string data)
        {
            JObject globalData = JObject.Parse(data);

            if(globalData[UID] != null)
                UserData = globalData[UID] as JObject;
        }

        #endregion
    }
}
