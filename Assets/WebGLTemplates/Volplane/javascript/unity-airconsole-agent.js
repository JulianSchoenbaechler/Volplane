/**
 * Unity-AirConsole Agent.
 * @copyright 2017 by Julian Schoenbaechler. All rights reserved.
 * @version 0.0.1
 * @see https://github.com/JulianSchoenbaechler/* for the project source code.
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
 * @param {Object} loadingBar - Properties of the loading bar.
 */
function Agent(gameContainer, screenRatio, loadingBar) {
    
    this.isStandalone = this.getUrlParameter('unity-editor-websocket-port') != null ? true : false;
    this.isUnityReady = false;
    this.dataQueue = [];
    
    if(this.isStandalone) {
        
        this.screenRatio = screenRatio || { width: 16, height: 9 };
        
        // Setup UnityLoader
        this.game = UnityLoader.instantiate(gameContainer, 'Build/game.json', this.screenRatio);
        
        if(typeof loadingBar != 'undefined')
            console.log('loading bar stuff...');
        
        this.resizeCanvas();
        this.initAirConsole();
        
    } else {
        
        this.setupWebsocket();
        
    }
}

/*
 * Initialize AirConsole API and register events.
 */
Agent.prototype.initAirConsole = function() {
    
    if(typeof this.airconsole != 'undefined')
        return;
    
    this.airconsole = new AirConsole({ 'synchronize_time': true });
    
    this.airconsole.onConnect = function(device_id) {
        this.sendToUnity({
            'action': 'onConnect',
            'device_id': device_id
        });
    };
    
    this.airconsole.onDisconnect = function(device_id) {
        this.sendToUnity({
            'action': 'onDisconnect',
            'device_id': device_id
        });
    };

    this.airconsole.onReady = function(code) {
        this.sendToUnity({
            'action': 'onReady',
            'code': code,
            'device_id': this.airconsole.device_id,
            'devices': this.airconsole.devices,
            'server_time_offset': this.airconsole.server_time_offset,
            'location': window.location.href
        });
    };
    
    this.airconsole.onMessage = function(from, data) {
        this.sendToUnity({
            'action': 'onMessage',
            'from': from,
            'data': data
        });
    };

    this.airconsole.onDeviceStateChange = function(device_id, device_data) {
        this.sendToUnity({
            'action': 'onDeviceStateChange',
            'device_id': device_id,
            'device_data': device_data
        });
    };
    
    this.airconsole.onCustomDeviceStateChange = function(device_id) {
        this.sendToUnity({
            'action': 'onCustomDeviceStateChange',
            'device_id': device_id
        });
    };
    
    this.airconsole.onDeviceProfileChange = function(device_id) {
        this.sendToUnity({
            'action': 'onDeviceProfileChange',
            'device_id': device_id
        });
    };
    
    this.airconsole.onEmailAddress = function(email_address) {
        this.sendToUnity({
            'action': 'onEmailAddress',
            'email_address': email_address
        });
    };
    
    this.airconsole.onAdShow = function() {
        this.sendToUnity({
            'action': 'onAdShow'
        });
    };
    
    this.airconsole.onAdComplete = function(ad_was_shown) {
        this.sendToUnity({
            'action': 'onAdComplete',
            'ad_was_shown': ad_was_shown
        });
    };

    this.airconsole.onPremium = function(device_id) {
        this.sendToUnity({
            'action': 'onPremium',
            'device_id': device_id
        });
    };

    this.airconsole.onPersistentDataLoaded = function(data) {
        this.sendToUnity({
            'action': 'onPersistentDataLoaded',
            'data': data
        });
    };

    this.airconsole.onPersistentDataStored = function(uid) {
        this.sendToUnity({
            'action': 'onPersistentDataStored',
            'uid': uid
        });
    };

    this.airconsole.onHighScores = function(highscores) {
        this.sendToUnity({
            'action': 'onHighScores',
            'highscores': highscores
        });
    };

    this.airconsole.onHighScoreStored = function(highscore) {
        this.sendToUnity({
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
    
    var port = this.getUrlParameter('unity-editor-websocket-port');
    
    this.socket = new WebSocket('ws://localhost:' + port + '/Volplane');
    
    this.socket.onopen = function() {
        
        this.isUnityReady = true;
        this.initAirConsole();
        
    }
    
    this.socket.onclose = function() {
        
        console.log('socket closed...');
        
    }
    
    this.socket.onmessage = function(msg) {
        
        this.processData(msg.data);
        
    }
    
}

/*
 * Send data to Unity as JSON object.
 * Enqueue data if Unity is not ready.
 * @param {Object} data - Data.
 */
Agent.prototype.sendToUnity = function(data) {
    
    if(this.isUnityReady) {
        
        if(this.isStandalone)
            this.game.SendMessage('Volplane', 'ProcessData', JSON.stringify(data));
        else
            this.socket.send(JSON.stringify(data));
        
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
    
    if((this.dataQueue.length != 0) || (data.action != 'onReady'))
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
    
    switch(data) {
        
        case 'message':
            this.airconsole.message(data.from, data.data);
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
        
        case 'storePersistentData':
            this.airconsole.storePersistentData(data.key, data.value, data.uid);
            break;
        
        case 'requestPersistentData':
            this.airconsole.requestPersistentData(data.uids);
            break;
        
        case 'storeHighScore':
            this.airconsole.storePersistentData(data.key, data.value, data.uid);
            break;
        
        case 'requestHighScores':
            this.airconsole.requestPersistentData(data.uids);
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
    var width, height;
    
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
    
    this.game.container.style.width = width.toString() + 'px';
    this.game.container.style.height = height.toString() + 'px';
    
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
