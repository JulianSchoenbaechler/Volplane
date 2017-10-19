/**
 * WebGL template progress bar for Unity 5.5 and older.
 * @copyright 2017 by Julian Schoenbaechler. All rights reserved.
 * @version 1.0.0
 * @see https://github.com/JulianSchoenbaechler/Volplane for the project source code.
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
 * @see the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with the Volplane project.
 * If not, see http://www.gnu.org/licenses/.
 *
 */

/**
 * The UnityProgress object will be instantiated by the 'UnityLoader.js' of the WebGL build.
 * @constructor
 * @param {string} gameContainer - The element id of the game container.
 * @param {Object} dom - The dom object of the canvas.
 */
function UnityProgress(dom) {

    this.progress = 0.0;
    this.message = 'Loading...';
    this.dom = dom;

    this.main = document.getElementById('screen-progress');
    this.bar = null;
    this.info = this.main.getElementsByClassName('info')[0];

    var node = this.main.getElementsByClassName('bar')[0];

    for(var i = 0; i < node.childNodes.length; i++) {
        if(node.childNodes[i].nodeName.toLowerCase() == 'span') {
          this.bar = node.childNodes[i];
          break;
        }
    }

    this.Update();

}

/**
 * Setting the percentage of the progress.
 * @param {number} progress - A number from 0.0 to 1.0.
 */
UnityProgress.prototype.SetProgress = function(progress) {

    if(this.progress < progress)
        this.progress = progress;

    if(this.progress >= 1)
        this.message = 'Preparing...';

    this.Update();

};

/**
 * Setting the loading message.
 * @param {string} message - Useful loading information.
 */
UnityProgress.prototype.SetMessage = function(message) {

    this.message = message;
    this.Update();

};

/**
 * Clears the loading progress view.
 */
UnityProgress.prototype.Clear = function() {

    this.main.style.display = 'none';

};

/**
 * Updates all used DOM objects.
 */
UnityProgress.prototype.Update = function() {

    if(this.info != null)
        this.info.innerHTML = this.message;

    if(this.bar != null)
        this.bar.style.width = Math.min(100, this.progress * 100).toString() + '%';

};
