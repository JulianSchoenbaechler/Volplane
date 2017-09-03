/**
 * Volplane AirConsole Agent
 * JSLib Plugin
 * @copyright 2017 by Julian Schoenbaechler (http://julian-s.ch/). All rights reserved.
 * @version 0.1.0
 * @license GPL v3
 */
 var AirConsoleAgent = {
    Hello: function(aBool)
    {
        console.log(aBool);
    }
};

mergeInto(LibraryManager.library, AirConsoleAgent);
