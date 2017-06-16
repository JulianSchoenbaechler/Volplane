/*
 * Copyright - Julian Schoenbaechler
 * https://github.com/JulianSchoenbaechler/*
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
    using UnityEngine;
    using SimpleJSON;


    public class VolplaneController : MonoBehaviour
    {
        public static VolplaneController VolplaneSingleton;

        private void Awake()
        {
            if((VolplaneSingleton != null) && (VolplaneSingleton != this))
                Destroy(this.gameObject);

            VolplaneSingleton = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            Application.runInBackground = true;
        }

        public void ProcessData(string data)
        {
            JSON.Parse(data);
        }
    }
}
