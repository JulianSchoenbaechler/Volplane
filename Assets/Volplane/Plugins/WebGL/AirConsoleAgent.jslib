/**
 * Volplane AirConsole Agent
 * JSLib Plugin
 * @copyright 2017 by Julian Schoenbaechler (http://julian-s.ch/). All rights reserved.
 * @version 0.1.0
 * @license GPL v3
 */
var AirConsoleAgent = {
    SendData: function(data)
    {
        window.volplane.processData(Pointer_stringify(data));
    },
    UnityIsReady: function(autoScale, objectName)
    {
        window.volplane.unityIsReady(autoScale ? true : false, Pointer_stringify(objectName));
    }
};

mergeInto(LibraryManager.library, AirConsoleAgent);
