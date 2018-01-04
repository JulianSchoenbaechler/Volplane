/**
 * Volplane Controller
 * @copyright 2017 by Julian Schoenbaechler (http://julian-s.ch/). All rights reserved.
 * @version 1.0.0
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
 * @param {String|undefined} standardView - The name of the view that should initially be loaded.
 * @param {String|undefined} controllerData - The path to the controller data (created by the Volplane Controller Editor).
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
 * ELEMENT CREATION
 * -------------------------------------------------------------------------
 */

/**
 * Creates a new button.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newButton = function(elementObject, viewName, $viewSelector) {

    var instance = this;

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
            'color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);

    new Button($elementSelector.attr('id'), {
        'down': function() {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'button',
                data: {
                    state: true,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.airconsole.message(AirConsole.SCREEN, data);
        },
        'up': function() {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'button',
                data: {
                    state: false,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.airconsole.message(AirConsole.SCREEN, data);
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

    var instance = this;

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
                'color': elementObject.fontColor || 'rgb(248, 248, 236)'
            })
        )
    )
    .appendTo($viewSelector);

    new DPad($elementSelector.attr('id'), {
        'relative': elementObject.relative || false,
        'distance': elementObject.distance || 10,
        'diagonal': elementObject.diagonal || false,
        'touchstart': function() {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'dpad',
                data: {
                    state: true,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        },
        'directionchange': function() {
            if(!instance.active) return;    // Disable input?
            var directions = {
                x: this.state[DPad.RIGHT] ? 1 : (this.state[DPad.LEFT] ? -1 : 0),
                y: this.state[DPad.UP] ? 1 : (this.state[DPad.DOWN] ? -1 : 0)
            };
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'dpad',
                data: {
                    x: directions.x,
                    y: directions.y,
                    state: (directions.x == 0) && (directions.y == 0) ? false : true,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        },
        'touchend': function(hadDirections) {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'dpad',
                data: {
                    x: 0,
                    y: 0,
                    state: false,
                    hadDirections: (elementObject.relative || false) ? hadDirections : false,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        }
    });

};

/**
 * Creates a new joystick.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newJoystick = function(elementObject, viewName, $viewSelector) {

    var instance = this;

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
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        state: true,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
            },
            'touchmove': function(position) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        x: position.x,
                        y: position.y * -1,
                        state: true
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
            },
            'touchend': function() {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        x: 0,
                        y: 0,
                        state: false,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
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

        var joystick = new JoystickRelative($elementSelector.attr('id'), {
            'base_stick_size_percent': elementObject.stickSize || 50,
            'stick_size_percent': elementObject.thumbSize || 20,
            'touchstart': function() {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        state: true,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
            },
            'touchmove': function(position) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        x: position.x,
                        y: position.y * -1,
                        state: true
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
            },
            'touchend': function(hadDirections) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'joystick',
                    data: {
                        x: 0,
                        y: 0,
                        state: false,
                        hadDirections: hadDirections,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.rateLimiter.message(AirConsole.SCREEN, data);
            }
        });

        // Recalculate element sizes when containing view is shown
        $viewSelector.on('show', function() {
            joystick.initElementSizes();
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

    var instance = this;

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
            'color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);

    if((elementObject.analog || true) === true) {

        new SwipeAnalog($elementSelector.attr('id'), {
            'min_swipe_distance': elementObject.distance || 30,
            'onTrigger': function(directionVector) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: true,
                        x: directionVector.x,
                        y: directionVector.y * -1,
                        distance: directionVector.distance,
                        angle: directionVector.angle,
                        degree: directionVector.degree,
                        speed: directionVector.speed,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            },
            'touchstart': function() {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: false,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            },
            'touchend': function(e, hadDirections) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: false,
                        hadDirections: hadDirections,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            }
        });

    } else {

        new SwipeDigital($elementSelector.attr('id'), {
            'min_swipe_distance': elementObject.distance || 30,
            'allowed_directions': (elementObject.diagonal || false) ?
                                  SwipeDigital.ALLOWED_DIRECTIONS.FOURWAY :
                                  SwipeDigital.ALLOWED_DIRECTIONS.EIGHTWAY,
            'onTrigger': function(directionMap) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: true,
                        x: directionMap[SwipeDigital.RIGHT] ? 1 : directionMap[SwipeDigital.LEFT] ? -1 : 0,
                        y: directionMap[SwipeDigital.UP] ? 1 : directionMap[SwipeDigital.DOWN] ? -1 : 0,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            },
            'touchstart': function(e) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: false,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            },
            'touchend': function(e, hadDirections) {
                if(!instance.active) return;    // Disable input?
                var data = {};
                data.volplane = {
                    action: 'input',
                    name: elementObject.name,
                    type: 'swipe',
                    data: {
                        state: false,
                        hadDirections: hadDirections,
                        timeStamp: instance.airconsole.getServerTime()
                    }
                };
                instance.airconsole.message(AirConsole.SCREEN, data);
            }
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

    var instance = this;

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
            'color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);

    new TouchArea($elementSelector.attr('id'), {
        'touchstart': function(position) {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'touch',
                data: {
                    state: true,
                    move: false,
                    x: position.x,
                    y: 1 - position.y,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.airconsole.message(AirConsole.SCREEN, data);
        },
        'touchmove': function(position) {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'touch',
                data: {
                    state: true,
                    move: true,
                    x: position.x,
                    y: 1 - position.y
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        },
        'touchend': function(position) {
            if(!instance.active) return;    // Disable input?
            var data = {};
            data.volplane = {
                action: 'input',
                name: elementObject.name,
                type: 'touch',
                data: {
                    state: false,
                    move: false,
                    x: position.x,
                    y: 1 - position.y,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        }
    });
};

/**
 * Creates a new text field.
 * @param {Object} elementObject - Element data.
 * @param {String} viewName - The name of the view the element is in.
 * @param {jQuery} $viewSelector - jQuery selector of the view container.
 */
VolplaneController.prototype.newText = function(elementObject, viewName, $viewSelector) {

    var instance = this;

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
            'color': elementObject.fontColor || 'rgb(248, 248, 236)'
        })
    )
    .appendTo($viewSelector);

};


/**
 * @chapter
 * ELEMENT MANIPULATION
 * -------------------------------------------------------------------------
 */

/**
 * Edit element properties.
 * @param {String} name - The name of the element to edit.
 * @param {String|undefined} view - The name of the view on which the element is placed.
 * @param {Object|undefined} properties - An object with element properties to overwrite.
 */
VolplaneController.prototype.editElement = function(name, view, properties) {

    if(typeof name == 'undefined')
        return;

    var instance = this;

    properties = properties || {};

    var propertyNames = Object.getOwnPropertyNames(properties);
    var $selector;

    // Manipulate current view or specified view?
    if(typeof view == 'undefined')
        $selector = $('#volplane-' + instance.getActiveView() + '-' + name + '.volplane-controller-element');
    else
        $selector = $('#volplane-' + view + '-' + name + '.volplane-controller-element');

    // This element does not exist
    if($selector.length == 0)
        return;

    // Iterate through all properties
    for(var i = 0; i < propertyNames.length; i++) {

        // What to change / edit
        switch(propertyNames[i]) {

            case 'hidden':

                if(properties.hidden)
                    $selector.hide();
                else
                    $selector.show();

                break;

            case 'toggle':

                $selector.toggle();

                break;

            case 'image':
                $selector.css('background-image', "url('" + (properties.image || 'img/transparent.png') + "')");
                break;

            case 'text':
                $selector.find('p').html(properties.text);
                break;

            case 'textAlign':
                $selector.find('p').css('text-align', properties.textAlign);
                break;

            case 'paddingVertical':
                $selector.find('p').css('padding', properties.paddingVertical.toString() + 'pt ' +
                                                   (properties.paddingHorizontal || 2).toString() + 'pt');
                break;

            case 'paddingHorizontal':
                $selector.find('p').css('padding', (properties.paddingVertical || 2).toString() + 'pt ' +
                                                   properties.paddingHorizontal.toString() + 'pt');
                break;

            case 'font':
                $selector.find('p').css('font-family', properties.font);
                break;

            case 'fontSize':
                $selector.find('p').css('font-size', properties.fontSize.toString() + 'pt');
                break;

            case 'fontColor':
                $selector.find('p').css('color', properties.fontColor);
                break;

            case 'highlightImage':
                $selector.children('div').css('background-image', "url('" + properties.highlightImage + "')");
                break;

            case 'handlerImage':
                $selector.children('div:not(.dpad-arrow-up):not(.dpad-arrow-down):not(.dpad-arrow-left):not(.dpad-arrow-right)')
                .css('background-image', "url('" + properties.handlerImage + "')");
                break;

            case 'upHighlightImage':
                $selector.children('div.dpad-arrow-up').css('background-image', "url('" + properties.upHighlightImage + "')");
                break;

            case 'downHighlightImage':
                $selector.children('div.dpad-arrow-down').css('background-image', "url('" + properties.downHighlightImage + "')");
                break;

            case 'leftHighlightImage':
                $selector.children('div.dpad-arrow-left').css('background-image', "url('" + properties.leftHighlightImage + "')");
                break;

            case 'rightHighlightImage':
                $selector.children('div.dpad-arrow-right').css('background-image', "url('" + properties.rightHighlightImage + "')");
                break;

            case 'stickImage':
                $selector.children('div.joystick-relative').css('background-image', "url('" + properties.stickImage + "')");
                $selector.children('div.joystick-relative-base-stick').css('background-image', "url('" + properties.stickImage + "')");
                break;

            case 'thumbImage':
                $selector.children('div.joystick-relative-stick').css('background-image', "url('" + properties.thumbImage + "')");
                break;

            default:
                break;
        }

    }

};

/**
 * Edit view properties.
 * @param {String} name - The name of the view to edit.
 * @param {Object|undefined} properties - An object with view properties to overwrite.
 */
VolplaneController.prototype.editView = function(name, properties) {

    if(typeof name == 'undefined')
        return;

    var instance = this;

    properties = properties || {};

    var propertyNames = Object.getOwnPropertyNames(properties);
    var $selector = $('#volplane-view-' + name + '.volplane-view');

    // This view does not exist
    if($selector.length == 0)
        return;

    // Iterate through all properties
    for(var i = 0; i < propertyNames.length; i++) {

        // What to change / edit
        switch(propertyNames[i]) {

            case 'image':
                $selector.css('background-image', "url('" + (properties.image || 'img/transparent.png') + "')");
                break;

            case 'color':
                $selector.find('p').css('background-color', properties.color || 'transparent');
                break;

            default:
                break;
        }

    }

};


/**
 * @chapter
 * VIEW HANDLING
 * -------------------------------------------------------------------------
 */

/**
* Get the name of the current active view.
* @returns {String} The name the currently active view.
*/
VolplaneController.prototype.getActiveView = function() {

    if($('.volplane-view:visible').length == 0)
        return;

    var id = $('.volplane-view:visible')[0].getAttribute('id');

    return id.replace('volplane-view-', '');

};

/**
 * Clears the view and reload its content.
 * @param {String} name - The name of the view to reset.
 */
VolplaneController.prototype.resetView = function(name) {

    if(typeof name == 'undefined')
        return;

    var viewObject = instance.controllerObject.views[name];
    var elementObject = {};

    if(typeof viewObject == 'undefined')
        return;


    // View properties
    $viewSelector = $('#volplane-view-' + name + '.volplane-view')
    .css({
        'background-color': viewObject.color || 'rgb(31, 29, 42)',
        'background-image': "url('" + (viewObject.image || 'img/transparent.png') + "')"
    });

    if(typeof viewObject.format != 'undefined') {

        if(viewObject.format.lastIndexOf('repeat', 0) === 0) {

            $viewSelector.css('background-size', 'auto auto');
            $viewSelector.css('background-repeat', viewObject.format);

        } else {

            $viewSelector.css('background-size', viewObject.format);
            $viewSelector.css('background-repeat', 'no-repeat');

        }

    }

    // Empty view content
    $viewSelector.empty();

    // Load controller elements
    for(j = 0; j < viewObject.content.length; j++) {

        elementObject = viewObject.content[j];

        switch(elementObject.type) {

            case 'button':
                instance.newButton(elementObject, name, $viewSelector);
                break;

            case 'dpad':
                instance.newDPad(elementObject, name, $viewSelector);
                break;

            case 'joystick':
                instance.newJoystick(elementObject, name, $viewSelector);
                break;

            case 'swipe':
                instance.newSwipe(elementObject, name, $viewSelector);
                break;

            case 'touch':
                instance.newTouch(elementObject, name, $viewSelector);
                break;

            case 'text':
                instance.newText(elementObject, name, $viewSelector);
                break;

            default:
                break;
        }

    }

};

/**
 * Switches to another view (hide/show)
 * @param {String} name - The name of the view switching to.
 */
VolplaneController.prototype.changeView = function(name) {

    if(typeof name == 'undefined')
        return;

    var instance = this;

    if(name == instance.getActiveView())
        return;

    // View name or index number?
    if(typeof name == 'string') {

        $viewSelector = $('#volplane-view-' + name + '.volplane-view')

        if($viewSelector.length == 0)
            return;

    } else {

        $viewSelector = $('.volplane-view:nth-child(' + Math.max(1, parseInt(name, 10) + 1) + ')');

        if($viewSelector.length == 0)
            return;

    }

    // Hide visible views and show specified one
    $('.volplane-view:visible').hide();
    $viewSelector.show();
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

    var instance = this;

    var i, j;
    var views = Object.getOwnPropertyNames(instance.controllerObject.views);
    var viewObject, elementObject;
    var $viewSelector;


    // Loading font-faces...

    // Cut path and extension -> plain file name
    var filename = function(path) {
        if(typeof path == 'undefined') return;
        path = path.includes('/') ? path.substr(path.lastIndexOf('/') + 1) : path;
        return path.includes('.') ? path.substr(0, path.lastIndexOf('.')) : path;
    };

    var fontFaceStyle = '';
    var fontFaceCreated = [];

    // Iterate through font list
    for(var i = 0; i < instance.controllerObject.fontList.length; i++) {

        var fontName = filename(instance.controllerObject.fontList[i]);
        var fontFolder = instance.controllerObject.fontList[i];
        fontFolder = fontFolder.substring(0, fontFolder.search(fontName));

        // No font-face for this font created?
        if($.inArray(fontName, fontFaceCreated) == -1) {

            var fontFace = "@font-face {\nfont-family: '" + fontName + "';\n";

            // EOT (used as default) exists?
            if($.inArray(fontFolder + fontName + '.eot', instance.controllerObject.fontList)) {
                fontFace += "src: url('" + fontFolder + fontName + ".eot');\n";
                fontFace += "src: url('" + fontFolder + fontName + ".eot') format('embedded-opentype'),";
            } else {
                fontFace += "src: ";
            }

            // TTF exists?
            if($.inArray(fontFolder + fontName + '.ttf', instance.controllerObject.fontList)) {
                fontFace += "url('" + fontFolder + fontName + ".ttf') format('truetype'),";
            }

            // OTF exists?
            if($.inArray(fontFolder + fontName + '.otf', instance.controllerObject.fontList)) {
                fontFace += "url('" + fontFolder + fontName + ".otf') format('opentype'),";
            }

            // WOFF exists?
            if($.inArray(fontFolder + fontName + '.woff', instance.controllerObject.fontList)) {
                fontFace += "url('" + fontFolder + fontName + ".woff') format('woff'),";
            }

            // WOFF2 exists?
            if($.inArray(fontFolder + fontName + '.woff2', instance.controllerObject.fontList)) {
                fontFace += "url('" + fontFolder + fontName + ".woff2') format('woff2'),";
            }

            // SVG exists?
            if($.inArray(fontFolder + fontName + '.svg', instance.controllerObject.fontList)) {
                fontFace += "url('" + fontFolder + fontName + ".svg') format('svg'),";
            }

            // Slice last comma
            fontFace = fontFace.slice(0, -1);

            fontFace += ";\nfont-weight: normal;\nfont-style: normal;\n}\n";

            // Add font-face
            fontFaceStyle += fontFace;
            fontFaceCreated.push(fontName);

        }

    }

    // Add font-face to css style sheet
    $('<style/>').append(fontFaceStyle).appendTo($('head'));


    // Loading views...

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
 * @param {String|undefined} standardView - The name of the view that should initially be loaded.
 * @param {String|undefined} controllerData - The path to the controller data (created by the Volplane Controller Editor).
 * @private
 */
VolplaneController.prototype.init = function(standardView, controllerData) {

    var instance = this;

    // Vibration support
    navigator.vibrate = navigator.vibrate ||
                        navigator.webkitVibrate ||
                        navigator.mozVibrate ||
                        navigator.msVibrate;

    // Variables
    instance.standardView = standardView || '';
    instance.controllerData = controllerData || 'controller';
    instance.active = true;
    instance.deviceMotion = false;

    // Proxy object functions
    $.proxy(instance.newButton, instance);
    $.proxy(instance.newDPad, instance);
    $.proxy(instance.newJoystick, instance);
    $.proxy(instance.newSwipe, instance);
    $.proxy(instance.newTouch, instance);
    $.proxy(instance.newText, instance);
    $.proxy(instance.loadController, instance);
    $.proxy(instance.editElement, instance);
    $.proxy(instance.editView, instance);
    $.proxy(instance.getActiveView, instance);
    $.proxy(instance.resetView, instance);
    $.proxy(instance.changeView, instance);

    // Load controller data
    $.get(instance.controllerData + '.json?t=' + (Date.now() / 1000 | 0).toString(), function(data) {

        instance.controllerObject = data;

        // Initialize AirConsole with controller settings
        instance.airconsole = new AirConsole({
            'orientation': instance.controllerObject.layout || 'portrait',
            'synchronize_time': instance.controllerObject.synchronizeTime || true,
            'device_motion': instance.controllerObject.deviceMotion === true ?
                             instance.controllerObject.deviceMotionInterval :
                             undefined
        });

        instance.rateLimiter = new RateLimiter(instance.airconsole);

        // Load controller
        instance.loadController();


        // Callback function - onDeviceMotion event
        instance.airconsole.onDeviceMotion = function(motionData) {

            if(!instance.deviceMotion || !instance.active)
                return;

            var data = {};
            data.volplane = {
                action: 'input',
                name: 'volplane-device-motion',
                type: 'motion',
                data: {
                    x: motionData.x,
                    y: motionData.y,
                    z: motionData.z,
                    alpha: motionData.alpha,
                    beta: motionData.beta,
                    gamma: motionData.gamma,
                    timeStamp: instance.airconsole.getServerTime()
                }
            };
            instance.rateLimiter.message(AirConsole.SCREEN, data);
        };

        // Callback function - onCustomDeviceStateChange event
        instance.airconsole.onCustomDeviceStateChange = function(deviceId, data) {

            if(typeof data.volplane == 'undefined')
                return;

            if(deviceId != AirConsole.SCREEN)
                return;

            // Controller view
            var view = data.volplane.views[instance.airconsole.getDeviceId()];

            // Change view (if needed)
            instance.changeView(view);

            // Set active or inactive
            instance.active = data.volplane.active[instance.airconsole.getDeviceId()];

        };

        // Callback function - onMessage event
        instance.airconsole.onMessage = function(deviceId, data) {

            if(typeof data.volplane == 'undefined')
                return;

            if(deviceId != AirConsole.SCREEN)
                return;

            // Action
            switch(data.volplane.action) {

                // Vibrate controller
                case 'vibrate':

                    if(typeof data.volplane.time == 'number') {
                        if('vibrate' in navigator)
                            navigator.vibrate(data.volplane.time);
                        else
                            instance.airconsole.vibrate(data.volplane.time);
                    }

                    break;

                // Enable or disable device motion input
                case 'deviceMotion':

                    if(typeof data.volplane.enable == 'boolean') {

                        instance.deviceMotion = data.volplane.enable;

                        // Send reset data on disable
                        if(!instance.deviceMotion) {

                            var data = {};
                            data.volplane = {
                                action: 'input',
                                name: 'volplane-device-motion',
                                type: 'motion',
                                data: {
                                    x: 0,
                                    y: 0,
                                    z: 0,
                                    alpha: 0,
                                    beta: 0,
                                    gamma: 0,
                                    timeStamp: instance.airconsole.getServerTime()
                                }
                            };
                            instance.rateLimiter.message(AirConsole.SCREEN, data);

                        }
                    }

                    break;

                // Edit element
                case 'element':

                    instance.editElement(data.volplane.name, data.volplane.view, data.volplane.properties);

                    break;

                // Edit view
                case 'view':

                    instance.editView(data.volplane.name, data.volplane.properties);

                    break;

                default:
                    break;
            }

        };


    // Controller data could not be loaded
    }, 'json').fail(function() {

        console.log('Volplane Controller: Failed to load controller data.');

        // Initialize AirConsole
        instance.airconsole = new AirConsole({
            'orientation': window.config.orientation || 'portrait',
            'synchronize_time': window.config.synchronizeTime || true,
            'device_motion': window.config.deviceMotion || undefined
        });

        instance.rateLimiter = new RateLimiter(instance.airconsole);

    });

};
