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

    public class VPlayer : IDisposable
    {
        public VPlayer(int acDeviceId)
        {
            this.State = PlayerState.Inactive;
            this.DeviceId = acDeviceId;
            this.IsConnected = true;
            this.IsHero = VolplaneController.AirConsole.IsPremium(acDeviceId);


            // Subscribe events
            VolplaneController.AirConsole.onConnect += Connect;
            VolplaneController.AirConsole.onDisconnect += Disconnect;
            VolplaneController.AirConsole.onPremium += Hero;
            VolplaneController.AirConsole.onDeviceStateChange += UpdateSettings;
            //VolplaneController.AirConsole.onCustomDeviceStateChange;
            //VolplaneController.AirConsole.onDeviceProfileChange;
            //VolplaneController.AirConsole.onAdShow;
            //VolplaneController.AirConsole.onAdComplete;
        }

        public enum PlayerState
        {
            Inactive,
            Active,
            Pending
        }

        public PlayerState State { get; protected set; }
        public int DeviceId { get; protected set; }
        public bool IsConnected { get; protected set; }
        public bool IsHero { get; protected set; }



        public void Dispose()
        {
            // Unsubscribe all events
            /*
            VolplaneController.AirConsole.onConnect;
            VolplaneController.AirConsole.onDisconnect;
            VolplaneController.AirConsole.onPremium;
            VolplaneController.AirConsole.onDeviceStateChange;
            VolplaneController.AirConsole.onCustomDeviceStateChange;
            VolplaneController.AirConsole.onDeviceProfileChange;
            VolplaneController.AirConsole.onAdShow;
            VolplaneController.AirConsole.onAdComplete;
            */
        }


        protected void Connect(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsConnected = true;
            }
        }

        protected void Disconnect(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsConnected = false;
            }
        }

        protected void Hero(int acDeviceId)
        {
            if(acDeviceId == DeviceId)
            {
                IsHero = true;
            }
        }

        protected void UpdateSettings(int acDeviceId, JSONNode data)
        {
            if(data == null)
                return;
            
            if(acDeviceId == DeviceId)
            {
            }
        }
    }
}
