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
    using System.Buffers;

    public class JSONArrayPool : IArrayPool<char>
    {
        public static readonly JSONArrayPool Instance = new JSONArrayPool();

        /// <summary>
        /// Gets a char array from System.Buffers shared pool.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array.</param>
        /// <returns>The char array.</returns>
        public char[] Rent(int minimumLength)
        {
            return ArrayPool<char>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// Return a char array to System.Buffers shared pool.
        /// </summary>
        /// <param name="array">The char array.</param>
        public void Return(char[] array)
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }
}
