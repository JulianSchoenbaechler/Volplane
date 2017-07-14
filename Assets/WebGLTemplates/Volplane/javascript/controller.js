/**
 * Volplane Controller
 * @copyright 2017 by Julian Schoenbaechler (http://julian-s.ch/). All rights reserved.
 * @version 0.1.0
 * @license GPL v3
 * 
 * @external jQuery
 * @see {@link http://api.jquery.com/jQuery/}
 * 
 * @external AirConsole
 * @see {@link https://www.airconsole.com/}
 */

/**
 * Gateway object to the controller object.
 * @constructor
 */
function VolplaneController() {
    this.init();
}


/**
 * @chapter
 * PROPERTIES
 * -------------------------------------------------------------------------
 */
 VolplaneController.prototype.controllerObject = {};

/**
 * @chapter
 * PUBLIC FUNCTIONS
 * -------------------------------------------------------------------------
 */

/**
 * Text input prompt popup.
 * @param {String} title - The title displayed as header of the popup.
 * @param {String} text - The displayed text.
 * @param {Boolean} isName - Should the text input correlate naming conventions?
 * @param {Function} callback - A callback when user sends data.
 */


/**
 * PRIVATE FUNCTIONS
 * -------------------------------------------------------------------------
 */

/**
 * Get a new popup.
 * @param {String} name - The name displayed as header for this popup.
 * @returns {jQuery} jQuery selector of this popup div container.
 * @private
 */
VolplaneController.prototype.loadController = function() {
    
    instance = this;
    
    var i, j;
    var views = Object.getOwnPropertyNames(instance.controllerObject.views);
    var viewObject, elementObject;
    var $viewSelector, $elementSelector;
    
    // Iterate through views
    for(i = 0; i < views.length; i++) {
        
        viewObject = instance.controllerObject.views[views[i]];
        
        // View properties
        $viewSelector = $('<div/>', {
            'id': 'volplane-view-' + views[i],
            'class': 'volplane-view'
        })
        .css({
            'background-color': viewObject.color || 'rgb(31, 29, 42)',
            'background-image': "url('" + (viewObject.image || 'img/transparent.png') + "')"
        })
        .appendTo($('body'));
        
        if(typeof viewObject.format != 'undefined') {
            
            if(viewObject.format.lastIndexOf('repeat', 0) === 0) {
                
                $viewSelector.css('background-size', 'auto auto');
                $viewSelector.css('background-repeat', viewObject.format);
                
            } else {
                
                $viewSelector.css('background-size', viewObject.format);
                $viewSelector.css('background-repeat', 'no-repeat');
                
            }
            
        }
        
        // Load controller elements
        for(j = 0; j < viewObject.content.length; j++) {
            
            elementObject = viewObject.content[j];
            
            switch(elementObject.type) {
                
                case 'button':
                    
                    $elementSelector = $('<div/>', {
                        'id': 'volplane-' + views[i] + '-' + elementObject.name,
                        'class': 'volplane-button'
                    })
                    .css({
                        'left': elementObject.x.toString() + '%',
                        'top': elementObject.y.toString() + '%',
                        'width': elementObject.width.toString() + '%',
                        'height': elementObject.height.toString() + '%',
                        'background-image': "url('" + (elementObject.image || 'img/transparent.png') + "')",
                        'display': (elementObject.hidden || false) === true ? 'none' : 'block'
                    })
                    .append(
                        $('<div><p>' + (elementObject.text || '') + '</p></div>')
                        .css({
                            'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
                            'font-size': (elementObject.fontSize || 12).toString() + 'pt',
                            'font-color': elementObject.fontColor || 'rgb(248, 248, 236)',
                            'background-image': "url('" + (elementObject.highlightImage || 'img/transparent.png') + "')"
                        })
                    )
                    .appendTo($viewSelector);
                    
                    break;
                
            }
        }
        
    }
    
};

/**
 * Initializes the controller.
 * @private
 */
VolplaneController.prototype.init = function() {
    
    var instance = this;
    
    // Proxy object functions
    $.proxy(instance.loadController, instance);
    
    // Load controller data
    $.get('../controller.json?t=' + (Date.now() / 1000 | 0).toString(), function(data) {
        
        instance.controllerObject = data;
        
        instance.airconsole = new AirConsole({
            'orientation': instance.controllerObject.layout || 'portrait',
            'synchronize_time': instance.controllerObject.synchronizeTime || true,
            'device_motion': instance.controllerObject.deviceMotion === true ?
                             instance.controllerObject.deviceMotionInterval :
                             undefined
        });
        
        instance.rateLimiter = RateLimiter(instance.airconsole);
        
        instance.loadController();
        
    }, 'json').fail(function() {
        
        throw new Error('Volplane Controller Error: Failed to load controller data.');
        
    });
    
};
