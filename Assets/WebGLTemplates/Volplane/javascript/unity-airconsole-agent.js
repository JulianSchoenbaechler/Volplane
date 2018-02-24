/**
 * Unity-AirConsole Agent.
 * @copyright 2017 by Julian Schoenbaechler. All rights reserved.
 * @version 1.0.6
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
 * Represents an agent interacting between the AirConsole API and Unity.
 * Do not access properties of this object directly.
 * @constructor
 * @param {string} gameContainer - The element id of the game container.
 * @param {Object} screenRatio - Object storing the ratio the game should be displayed.
 * @param {number} screenRatio.width - Ratio width.
 * @param {number} screenRatio.height - Ratio height.
 * @param {Object} loadingScreen - Properties of the loading bar.
 */
function Agent(gameContainer, screenRatio, loadingScreen) {

    this.accessObject = 'Volplane';
    this.compatibilityMode = false;
    this.isStandalone = this.getUrlParameter('unity-editor-websocket-port') != null ? false : true;
    this.isUnityReady = false;
    this.dataQueue = [];

    if(this.isStandalone) {

        this.screenRatio = screenRatio || { width: 16, height: 9 };

        // UnityLoader
        if(document.getElementById(gameContainer).nodeName.toLowerCase() == 'canvas') {

            // Unity 5.5 and older
            this.game = document.getElementById(gameContainer);
            this.compatibilityMode = true;

        } else {

            this.progress = new UnityProgress(gameContainer);

            // Setup UnityLoader
            this.game = UnityLoader.instantiate(gameContainer, 'Build/game.json', {
                onProgress: (function(instance) {
                    return function(game, progress) {
                        if(progress < 1)
                            instance.progress.SetProgress(progress);
                        else
                            instance.progress.Clear();

                        instance.progress.SetMessage(Math.round(Math.min(100, progress * 100)).toString() + '%');
                    };
                })(this)
            });

        }

        // Loading screen styles
        if(typeof loadingScreen != 'undefined') {

            var loadingContainer = document.getElementById('screen-progress');
            var image = loadingContainer.getElementsByTagName('IMG')[0];
            var infoText = loadingContainer.getElementsByClassName('info')[0];
            var barContainer = loadingContainer.getElementsByClassName('bar')[0];
            var progress = barContainer.getElementsByTagName('SPAN')[0];

            if(loadingScreen.fullScreenImage === true) {

                loadingContainer.className = 'full-screen';
                loadingContainer.style.backgroundImage = 'url("img/' + (loadingScreen.image || 'loading.png') + '")';
                loadingContainer.style.backgroundSize = (loadingScreen.backgroundSize || 'cover');
                barContainer.style.top = (loadingScreen.barTop || 60).toString() + '%';
                barContainer.style.left = (loadingScreen.barLeft || 35).toString() + '%';
                barContainer.style.width = (loadingScreen.barWidth || 30).toString() + '%';
                barContainer.style.height = (loadingScreen.barHeight || 6).toString() + '%';
                infoText.style.top = (loadingScreen.textTop || 70).toString() + '%';
                infoText.style.left = (loadingScreen.textLeft || 35).toString() + '%';
                infoText.style.width = (loadingScreen.textWidth || 30).toString() + '%';
                infoText.style.height = (loadingScreen.textHeight || 'auto').toString() + '%';

            } else {

                loadingContainer.style.backgroundImage = 'none';
                barContainer.style.width = (loadingScreen.barWidth || 300).toString() + 'px';
                barContainer.style.height = (loadingScreen.barHeight || 6).toString() + 'px';

            }

            loadingContainer.style.backgroundColor = (loadingScreen.background || '#1F1D2A');
            image.src = 'img/' + (loadingScreen.image || 'loading.png');
            infoText.style.color = (loadingScreen.fontColor || '#F8F8EC');
            progress.style.backgroundColor = (loadingScreen.barColor || '#7A2F34');
            progress.style.borderRadius = ((loadingScreen.barBorderRadius || 3) - 2).toString() + 'px';
            barContainer.style.borderColor = (loadingScreen.barBorderColor || '#87383C');
            barContainer.style.borderRadius = (loadingScreen.barBorderRadius || 3).toString() + 'px';

        }

        this.setupErrorHandler();
        this.resizeCanvas();
        this.initAirConsole();

    } else {

        this.setupWebsocket();

        document.body.innerHTML = '<div class="full-screen">' +
            '<p id="editor-message">You can see the game scene in the Unity Editor.<br />' +
            'Keep this window open in the background.</p>' +
            '</div>';

    }

}

/*
 * Setup an error-handler which reacts to specific errors and informs the user.
 * Proper error handling and stack tracing should be covered by the AirConsole API.
 */
Agent.prototype.setupErrorHandler = function() {

    // Override window onerror event
    window.onerror = function(msg) {

        if((message.indexOf('UnknownError') != -1) ||
           (message.indexOf('Program terminated with exit(0)') != -1) ||
           (message.indexOf('DISABLE_EXCEPTION_CATCHING') != -1)) {

            alert('An unknown error occured! Check your WebGL build.');

        } else if(message.indexOf('Cannot enlarge memory arrays') != -1) {

            window.setTimeout(function() {
                throw new Error('[Volplane] Not enough memory. Allocate more memory in the WebGL player settings.');
            }, 200);

            return false;

        } else if((message.indexOf('Invalid array buffer length') != -1) ||
                  (message.indexOf('out of memory') != -1) ||
                  (message.indexOf('Array buffer allocation failed') != -1)) {

            alert('Your browser ran out of memory. Try restarting your browser and close other applications running on your computer.');
            return true;

        }

        var container = document.createElement('div');
        var message = document.createElement('p');

        container.className = 'full-screen';
        message.innerHTML = 'An <span style="color: red;">error</span> has occured, the AirConsole team was informed.';

        container.appendChild(message);
        document.body.appendChild(container);

        // Navigate to AirConsole home in 5 seconds...
        window.setTimeout(function() {

           if(window.volplane && window.volplane.airconsole)
               window.volplane.airconsole.navigateHome();

        }, 5000);

        return true;

    }

};

/*
 * Initialize AirConsole API and register events.
 */
Agent.prototype.initAirConsole = function() {

    if(typeof this.airconsole != 'undefined')
        return;

    var instance = this;

    instance.airconsole = new AirConsole({ 'synchronize_time': true });

    instance.airconsole.onConnect = function(device_id) {
        instance.sendToUnity({
            'action': 'onConnect',
            'device_id': device_id
        });
    };

    instance.airconsole.onDisconnect = function(device_id) {
        instance.sendToUnity({
            'action': 'onDisconnect',
            'device_id': device_id
        });
    };

    instance.airconsole.onReady = function(code) {
        instance.sendToUnity({
            'action': 'onReady',
            'device_id': instance.airconsole.device_id,
            'data': {
                'code': code,
                'devices': instance.airconsole.devices,
                'server_time_offset': instance.airconsole.server_time_offset,
                'location': window.location.href
            }
        });
    };

    instance.airconsole.onMessage = function(from, data) {
        instance.sendToUnity({
            'action': 'onMessage',
            'from': from,
            'data': data
        });
    };

    instance.airconsole.onDeviceStateChange = function(device_id, device_data) {
        instance.sendToUnity({
            'action': 'onDeviceStateChange',
            'device_id': device_id,
            'device_data': device_data
        });
    };

    instance.airconsole.onCustomDeviceStateChange = function(device_id, custom_data) {
        instance.sendToUnity({
            'action': 'onCustomDeviceStateChange',
            'device_id': device_id,
            'custom_data': custom_data
        });
    };

    instance.airconsole.onDeviceProfileChange = function(device_id) {
        instance.sendToUnity({
            'action': 'onDeviceProfileChange',
            'device_id': device_id
        });
    };

    instance.airconsole.onAdShow = function() {
        instance.sendToUnity({
            'action': 'onAdShow'
        });
    };

    instance.airconsole.onAdComplete = function(ad_was_shown) {
        instance.sendToUnity({
            'action': 'onAdComplete',
            'ad_was_shown': ad_was_shown
        });
    };

    instance.airconsole.onPremium = function(device_id) {
        instance.sendToUnity({
            'action': 'onPremium',
            'device_id': device_id
        });
    };

    instance.airconsole.onPersistentDataLoaded = function(data) {
        instance.sendToUnity({
            'action': 'onPersistentDataLoaded',
            'data': data
        });
    };

    instance.airconsole.onPersistentDataStored = function(uid) {
        instance.sendToUnity({
            'action': 'onPersistentDataStored',
            'uid': uid
        });
    };

    instance.airconsole.onHighScores = function(highscores) {
        instance.sendToUnity({
            'action': 'onHighScores',
            'highscores': highscores
        });
    };

    instance.airconsole.onHighScoreStored = function(highscore) {
        instance.sendToUnity({
            'action': 'onHighScoreStored',
            'highscore': highscore
        });
    };

};

/*
 * Setup websocket connection. Initializes AirConsole API when connection opens.
 */
Agent.prototype.setupWebsocket = function() {

    if(typeof this.socket != 'undefined')
        return;

    var instance = this;
    var port = instance.getUrlParameter('unity-editor-websocket-port');

    instance.socket = new WebSocket('ws://localhost:' + port + '/Volplane');

    instance.socket.onopen = function() {

        instance.unityIsReady(false);
        instance.initAirConsole();

    };

    instance.socket.onclose = function() {

        document.getElementById('editor-message').innerHTML = 'Game <span style="color: red;">stopped</span> in Unity. Please close this tab.';

    };

    instance.socket.onmessage = function(msg) {

        instance.processData(msg.data);

    };

};

/*
 * Send data to Unity as JSON object.
 * Enqueue data if Unity is not ready.
 * @param {Object} data - Data.
 */
Agent.prototype.sendToUnity = function(data) {

    if(this.isUnityReady) {

        if(this.isStandalone) {

            if(!this.compatibilityMode)
                this.game.SendMessage(this.accessObject, 'ProcessData', JSON.stringify(data));
            else
                SendMessage(this.accessObject, 'ProcessData', JSON.stringify(data));

        } else {

            this.socket.send(JSON.stringify(data));

        }

    } else {

        // Enqueue data
        this.enqueueData(data);

    }

};

/*
 * Enqueue data for later use.
 * @param {Object} data - Data.
 */
Agent.prototype.enqueueData = function(data) {

    if((data.action == 'onReady') && (this.dataQueue.length > 0))
        this.dataQueue.unshift(data);
    else
        this.dataQueue.push(data);

};

/*
 * Send enqueued data to Unity.
 */
Agent.prototype.dequeueToUnity = function() {

    for(var i = 0; i < this.dataQueue.length; i++) {

        this.sendToUnity(this.dataQueue[i]);

    }

    this.dataQueue = [];

};

/*
 * Process data sent from Unity as JSON string.
 * @param {string} jsonData - Data as JSON string.
 */
Agent.prototype.processData = function(jsonData) {

    var data = JSON.parse(jsonData);

    switch(data.action) {

        case 'message':
            this.airconsole.message(data.to, data.data);
            break;

        case 'broadcast':
            this.airconsole.broadcast(data.data);
            break;

        case 'setCustomDeviceState':
            this.airconsole.setCustomDeviceState(data.data);
            break;

        case 'setCustomDeviceStateProperty':
            this.airconsole.setCustomDeviceStateProperty(data.key, data.value);
            break;

        case 'setActivePlayers':
            this.airconsole.setActivePlayers(data.max_players);
            break;

        case 'showAd':
            this.airconsole.showAd();
            break;

        case 'storePersistentData':
            this.airconsole.storePersistentData(data.key, data.value, data.uid);
            break;

        case 'requestPersistentData':
            this.airconsole.requestPersistentData(data.uids);
            break;

        case 'storeHighScore':
            this.airconsole.storeHighScore(data.level_name, data.level_version, data.score, data.uid, data.data, data.score_string);
            break;

        case 'requestHighScores':
            this.airconsole.requestHighScores(data.level_name, data.level_version, data.uids, data.ranks, data.total, data.top);
            break;

        case 'navigateHome':
            this.airconsole.navigateHome();
            break;

        case 'navigateTo':
            this.airconsole.navigateTo(data.data);
            break;

        case 'showDefaultUI':
            this.airconsole.showDefaultUI(data.data);
            break;

        default:
            console.log('[Volplane] Debug: ', data.data);
            break;

    }

};

/*
 * Resize game container element to fit screen with specified ratio.
 */
Agent.prototype.resizeCanvas = function() {

    if((typeof this.screenRatio.width == 'undefined') ||
        (typeof this.screenRatio.height == 'undefined'))
        return;

    var aspectRatio = this.screenRatio.width / this.screenRatio.height;

    if(!this.compatibilityMode) {

        var width, height;

        if(this.screenRatio.stretch) {

            // Stretch to full screen
            width = window.innerWidth;
            height = window.innerHeight;

        } else {

            // Fill window
            if((window.innerWidth / aspectRatio) > window.innerHeight) {

                // Stretch by height
                width = window.innerHeight * aspectRatio;
                height = window.innerHeight;

            } else {

                // Stretch by width
                width = window.innerWidth;
                height = window.innerWidth / aspectRatio;

            }

        }

        if(this.game.Module && this.game.Module.setCanvasSize)
            this.game.Module.setCanvasSize(width, height);

        this.game.container.style.width = width.toString() + 'px';
        this.game.container.style.height = height.toString() + 'px';

    } else {

        document.body.style.height = '100%';
        document.body.style.width = '100%';
        document.body.style.margin = 0;
        document.body.style.overflow = 'hidden';

        if(this.screenRatio.stretch) {

            this.game.style.width = '100vw';
            this.game.style.height = '100vh';
            this.game.style.maxWidth = '100vw';
            this.game.style.maxHeight = '100vh';

        } else {

            this.game.style.width = '100vw';
            this.game.style.height = (100 / aspectRatio).toString() + 'vw';
            this.game.style.maxWidth = (100 * aspectRatio).toString() + 'vh';
            this.game.style.maxHeight = '100vh';

        }

        this.game.style.margin = '0 auto';
        this.game.style.top = 0;
        this.game.style.bottom = 0;
        this.game.style.left = 0;
        this.game.style.right = 0;

    }

};

/*
 * Resolves single parameters from GET arguments in the URL.
 * @param {string} name - The name of the parameter.
 * @returns {string} - The resolved parameter.
 */
Agent.prototype.getUrlParameter = function(name) {

    var result = null;
    var tmp = [];

    window.location.search
        .substr(1)
        .split('&')
        .forEach(function(item) {
            tmp = item.split('=');

            if(tmp[0] === name)
                result = decodeURIComponent(tmp[1]);

        });

    return result;

};

/*
 * Setter function for external call from Unity.
 * Indicates that Unity is ready.
 * @param {boolean} autoScaleCanvas - true when canvas should scale automatically. Otherwise false.
 */
Agent.prototype.unityIsReady = function(autoScaleCanvas, accessObject = 'Volplane') {

    var instance = this;

    instance.accessObject = accessObject;
    instance.isUnityReady = true;
    instance.dequeueToUnity();

    if(autoScaleCanvas === true) {

        window.addEventListener('resize', function() { instance.resizeCanvas() });
        instance.resizeCanvas();

    }

};
