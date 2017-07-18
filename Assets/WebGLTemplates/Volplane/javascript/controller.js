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
function VolplaneController(standardView, controllerData) {
    this.init(standardView, controllerData);
}


/**
 * @chapter
 * PROPERTIES
 * -------------------------------------------------------------------------
 */
VolplaneController.prototype.controllerObject = {};
VolplaneController.prototype.debug = {};


/**
 * @chapter
 * PUBLIC FUNCTIONS
 * -------------------------------------------------------------------------
 */

/**
 * Creates a new button.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newButton = function(elementObject, viewName, $viewSelector) {
    
    var $elementSelector = $('<div/>', {
        'id': 'volplane-' + viewName + '-' + elementObject.name,
        'class': 'volplane-controller-element volplane-button'
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
        $('<div/>')
        .css({
            'background-image': "url('" + (elementObject.highlightImage || 'img/transparent.png') + "')"
        }),
        $('<p>' + (elementObject.text || '') + '</p>')
        .css({
            'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
            'font-size': (elementObject.fontSize || 12).toString() + 'pt',
            'font-color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);
    
    new Button($elementSelector.attr('id'), {
        'down': function() {
        },
        'up': function() {
        }
    });
};

/**
 * Creates a new dpad.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newDPad = function(elementObject, viewName, $viewSelector) {
    
    var $elementSelector = $('<div/>', {
        'id': 'volplane-' + viewName + '-' + elementObject.name,
        'class': 'volplane-controller-element volplane-dpad'
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
        $('<div/>')
        .css({
            'background-image': elementObject.relative ? "url('" + (elementObject.handlerImage || 'img/transparent.png') + "')" : 'none'
        })
        .append(
            $('<div/>', {
                'class': 'dpad-arrow-up'
            })
            .css({
                'background-image': "url('" + (elementObject.upHighlightImage || 'img/transparent.png') + "')"
            }),
            $('<div/>', {
                'class': 'dpad-arrow-down'
            })
            .css({
                'background-image': "url('" + (elementObject.downHighlightImage || 'img/transparent.png') + "')"
            }),
            $('<div/>', {
                'class': 'dpad-arrow-left'
            })
            .css({
                'background-image': "url('" + (elementObject.leftHighlightImage || 'img/transparent.png') + "')"
            }),
            $('<div/>', {
                'class': 'dpad-arrow-right'
            })
            .css({
                'background-image': "url('" + (elementObject.rightHighlightImage || 'img/transparent.png') + "')"
            }),
            $('<p>' + (elementObject.text || '') + '</p>')
            .css({
                'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
                'font-size': (elementObject.fontSize || 12).toString() + 'pt',
                'font-color': elementObject.fontColor || 'rgb(248, 248, 236)'
            })
        )
    )
    .appendTo($viewSelector);
    
    new DPad($elementSelector.attr('id'), {
        'relative': elementObject.relative || false,
        'distance': elementObject.distance || 10,
        'diagonal': elementObject.diagonal || false,
        'directionchange': function(key, pressed) {},
        'touchstart': function() {},
        'touchend': function(hadDirections) {}
    });
    
};

/**
 * Creates a new joystick.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newJoystick = function(elementObject, viewName, $viewSelector) {
    
    if(!(elementObject.relative || false)) {
        
        var $elementSelector = $('<div/>', {
            'id': 'volplane-' + viewName + '-' + elementObject.name,
            'class': 'volplane-controller-element volplane-joystick'
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
            $('<div/>', {
                'class': 'joystick-relative'
            })
            .css({
                'background-image': "url('" + (elementObject.stickImage || 'img/transparent.png') + "')"
            })
        )
        .appendTo($viewSelector);
        
        new Joystick($elementSelector.attr('id'), {
            'distance': elementObject.distance || 10,
            'touchstart': function() {
            },
            'touchmove': function(position) {
                
            },
            'touchend': function() {
            }
        });
    
    // Relative joystick
    } else {
        
        var $elementSelector = $('<div/>', {
            'id': 'volplane-' + viewName + '-' + elementObject.name,
            'class': 'volplane-controller-element volplane-joystick-relative'
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
            $('<div/>', {
                'class': 'joystick-inner-radius'
            })
            .css({
                'pointer-events': 'none'
            }),
            $('<div/>', {
                'class': 'joystick-relative-base-stick'
            })
            .css({
                'background-image': "url('" + (elementObject.stickImage || 'img/transparent.png') + "')"
            }),
            $('<div/>', {
                'class': 'joystick-relative-stick'
            })
            .css({
                'background-image': "url('" + (elementObject.thumbImage || 'img/transparent.png') + "')"
            })
        )
        .appendTo($viewSelector);
        
        new JoystickRelative($elementSelector.attr('id'), {
            'base_stick_size_percent': elementObject.stickSize || 50,
            'stick_size_percent': elementObject.thumbSize || 20,
            'touchstart': function() {
            },
            'touchmove': function(position) {
                
            },
            'touchend': function(hadDirections) {
            }
        });
        
    }
};

/**
 * Creates a new swipe field.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newSwipe = function(elementObject, viewName, $viewSelector) {
    
    var $elementSelector = $('<div/>', {
        'id': 'volplane-' + viewName + '-' + elementObject.name,
        'class': 'volplane-controller-element volplane-swipe'
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
        $('<div/>')
        .css({
            'background-image': "url('" + (elementObject.highlightImage || 'img/transparent.png') + "')"
        }),
        $('<p>' + (elementObject.text || '') + '</p>')
        .css({
            'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
            'font-size': (elementObject.fontSize || 12).toString() + 'pt',
            'font-color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);
    
    if((elementObject.analog || true) === true) {
        
        new SwipeAnalog($elementSelector.attr('id'), {
            'min_swipe_distance': elementObject.distance || 30,
            'onTrigger': function(directionVector) {},
            'touchstart': function() {},
            'touchend': function(e, hadDirections) {}
        });
        
    } else {
        
        new SwipeDigital($elementSelector.attr('id'), {
            'min_swipe_distance': elementObject.distance || 30,
            'allowed_directions': (elementObject.diagonal || false) ?
                                  SwipeDigital.ALLOWED_DIRECTIONS.FOURWAY :
                                  SwipeDigital.ALLOWED_DIRECTIONS.EIGHTWAY,
            'onTrigger': function(directionMap) {},
            'touchstart': function(e) {},
            'touchend': function(e, hadDirections) {}
        });
        
    }
};

/**
 * Creates a new touch area.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newTouch = function(elementObject, viewName, $viewSelector) {
    
    var $elementSelector = $('<div/>', {
        'id': 'volplane-' + viewName + '-' + elementObject.name,
        'class': 'volplane-controller-element volplane-touch'
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
        $('<p>' + (elementObject.text || '') + '</p>')
        .css({
            'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
            'font-size': (elementObject.fontSize || 12).toString() + 'pt',
            'font-color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);
    
    new TouchArea($elementSelector.attr('id'), {
        'touchstart': function(position) {},
        'touchmove': function(position) {},
        'touchend': function(position) {}
    });
};

/**
 * Creates a new text field.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newText = function(elementObject, viewName, $viewSelector) {
    
    var $elementSelector = $('<div/>', {
        'id': 'volplane-' + viewName + '-' + elementObject.name,
        'class': 'volplane-controller-element volplane-text'
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
        $('<p class="no-vertical-align">' + (elementObject.text || '') + '</p>')
        .css({
            'padding': (elementObject.paddingVertical || 2).toString() + 'pt ' +
                       (elementObject.paddingHorizontal || 2).toString() + 'pt',
            'text-align': elementObject.textAlign || 'center',
            'font-family': elementObject.font || 'Helvetica, Arial, sans-serif',
            'font-size': (elementObject.fontSize || 12).toString() + 'pt',
            'font-color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);
    
};


/**
 * PRIVATE FUNCTIONS
 * -------------------------------------------------------------------------
 */

/**
 * Load controller data.
 * @private
 */
VolplaneController.prototype.loadController = function() {
    
    instance = this;
    
    var i, j;
    var views = Object.getOwnPropertyNames(instance.controllerObject.views);
    var viewObject, elementObject;
    var $viewSelector;
    
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
                    instance.newButton(elementObject, views[i], $viewSelector);
                    break;
                
                case 'dpad':
                    instance.newDPad(elementObject, views[i], $viewSelector);
                    break;
                
                case 'joystick':
                    instance.newJoystick(elementObject, views[i], $viewSelector);
                    break;
                
                case 'swipe':
                    instance.newSwipe(elementObject, views[i], $viewSelector);
                    break;
                
                case 'touch':
                    instance.newTouch(elementObject, views[i], $viewSelector);
                    break;
                
                case 'text':
                    instance.newText(elementObject, views[i], $viewSelector);
                    break;
                
                default:
                    break;
            }
            
        }
        
        // Hide view
        if(instance.standardView !== views[i])
            $viewSelector.hide();
        
    }
    
};

/**
 * Initializes the controller.
 * @private
 */
VolplaneController.prototype.init = function(standardView, controllerData) {
    
    var instance = this;
    
    instance.standardView = standardView || '';
    instance.controllerData = controllerData || '../controller';
    
    // Proxy object functions
    $.proxy(instance.loadController, instance);
    
    // Load controller data
    $.get(instance.controllerData + '.json?t=' + (Date.now() / 1000 | 0).toString(), function(data) {
        
        instance.controllerObject = data;
        
        instance.airconsole = new AirConsole({
            'orientation': instance.controllerObject.layout || 'portrait',
            'synchronize_time': instance.controllerObject.synchronizeTime || true,
            'device_motion': instance.controllerObject.deviceMotion === true ?
                             instance.controllerObject.deviceMotionInterval :
                             undefined
        });
        
        instance.rateLimiter = new RateLimiter(instance.airconsole);
        
        instance.loadController();
        
    }, 'json').fail(function() {
        
        console.log('Volplane Controller: Failed to load controller data.');
        
        instance.airconsole = new AirConsole({
            'orientation': window.config.orientation || 'portrait',
            'synchronize_time': window.config.synchronizeTime || true,
            'device_motion': window.config.deviceMotion || undefined
        });
        
        instance.rateLimiter = new RateLimiter(instance.airconsole);
        
    });
    
};
