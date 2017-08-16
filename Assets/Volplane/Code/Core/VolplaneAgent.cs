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
    using Volplane.AirConsole;

    public class VolplaneAgent
    {
        // Main player list
        // This list indices can be hardcoded
        protected static List<VPlayer> Players;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.PlayerManager"/> class.
        /// </summary>
        public VolplaneAgent()
        {
        }

        /// <summary>
        /// Get a player by its identifier.
        /// </summary>
        /// <returns>The player object.</returns>
        /// <param name="playerId">Player identifier.</param>
        public static VPlayer GetPlayer(int playerId)
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
        /// <param name="player">The player object.</param>
        public static int GetPlayerId(VPlayer player)
        {
            return VolplaneAgent.Players.FindIndex(vp => vp.DeviceId == player.DeviceId);
        }



        /// <summary>
        /// Adds a new player to the list.
        /// </summary>
        /// <returns>The player identifier or -1 on failure.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected static int AddPlayer(int acDeviceId)
        {
            if(acDeviceId < 1)
                return -1;

            if(VolplaneAgent.Players != null)
            {
                int index = VolplaneAgent.GetPlayerId(acDeviceId);

                if(index == -1)
                {
                    VolplaneAgent.Players.Add(new VPlayer(acDeviceId));
                    return VolplaneAgent.Players.Count - 1;
                }

                return index;
            }
            else
            {
                VolplaneAgent.Players = new List<VPlayer>(8);
                VolplaneAgent.Players.Add(new VPlayer(acDeviceId));
                return 0;
            }
        }

        /// <summary>
        /// Gets the player identifier.
        /// </summary>
        /// <returns>The player identifier.</returns>
        /// <param name="acDeviceId">AirConsole device identifier.</param>
        protected static int GetPlayerId(int acDeviceId)
        {
            return VolplaneAgent.Players.FindIndex(vp => vp.DeviceId == acDeviceId);
        }
    }
}
