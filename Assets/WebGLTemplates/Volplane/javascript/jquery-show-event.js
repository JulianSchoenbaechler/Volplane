/**
 * jQuery 'show' event extension
 * @copyright 2017 by Julian Schoenbaechler (http://julian-s.ch/). All rights reserved.
 * @version 1.0.0
 * @license GPL v3
 *
 * @external jQuery
 * @see {@link http://api.jquery.com/jQuery/}
 */

(function($) {

    var e = $.Event('show');
    var mirror = $.fn.show;

    $.fn.show = function() {
        mirror.apply(this, arguments);
        $(this).trigger(e);
    };

})(jQuery);
