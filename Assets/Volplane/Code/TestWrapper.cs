using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Volplane;

public class TestWrapper : MonoBehaviour
{
    public int deviceId = 1;

    void Start()
    {
        VolplaneController.AirConsole.OnReady += delegate(string code) {
            Debug.Log("OnReady with game code: " + code);
        };

        VolplaneController.AirConsole.OnConnect += delegate(int acDeviceId) {
            Debug.Log("OnConnect with device id: " + acDeviceId.ToString("D"));
        };

        VolplaneController.AirConsole.OnDisconnect += delegate(int acDeviceId) {
            Debug.Log("OnDisconnect with device id: " + acDeviceId.ToString("D"));
        };

        VolplaneController.AirConsole.OnMessage += delegate(int acDeviceId, string data) {
            Debug.LogFormat("OnMessage (device/message): {0:D} / {1:G}", acDeviceId, data);
        };

        VolplaneController.AirConsole.OnDeviceStateChange += delegate(int acDeviceId, Volplane.AirConsole.AirConsoleAgent.Device data) {
            Debug.LogFormat("OnDeviceStateChange (device/data): {0:D} / {1:G}", acDeviceId, data.CustomData.ToString());
        };

        VolplaneController.AirConsole.OnCustomDeviceStateChange += delegate(int acDeviceId, string data) {
            Debug.LogFormat("OnCustomDeviceStateChange (device/data): {0:D} / {1:G}", acDeviceId, data);
        };

        VolplaneController.AirConsole.OnDeviceProfileChange += delegate(int acDeviceId) {
            Debug.LogFormat("OnDeviceProfileChange with device id: {0:D}", acDeviceId);
        };

        VolplaneController.AirConsole.OnAdShow += delegate {
            Debug.Log("OnAdShow");
        };

        VolplaneController.AirConsole.OnAdComplete += delegate(bool state) {
            Debug.LogFormat("OnAdComplete with state: {0:F}", state);
        };

        VolplaneController.AirConsole.OnPersistentDataLoaded += delegate(string data) {
            Debug.LogFormat("OnPersistentDataLoaded: {0:G}", data);
        };

        VolplaneController.AirConsole.OnPersistentDataStored += delegate(string uid) {
            Debug.LogFormat("OnPersistentDataStored: {0:G}", uid);
        };
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            VolplaneController.AirConsole.Message(deviceId, @"{""key"":""value""}");
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            VolplaneController.AirConsole.Broadcast(@"{""key"":""to all devices!""}");
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            VolplaneController.AirConsole.SetCustomDeviceState(@"{""randomData"":[0,4,3,2,65],""property"":""test-data""}");
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            VolplaneController.AirConsole.SetCustomDeviceStateProperty("randomData", @"[1,4,44,2,9]");
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("GetControllerDeviceIds:");

            foreach(var id in VolplaneController.AirConsole.GetControllerDeviceIds())
                Debug.Log(id);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            Debug.LogFormat("GetMasterControllerDeviceId: {0:D}", VolplaneController.AirConsole.GetMasterControllerDeviceId());
        }

        if(Input.GetKeyDown(KeyCode.U))
        {
            Debug.LogFormat("GetServerTime: {0:D}", VolplaneController.AirConsole.GetServerTime());
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            Debug.LogFormat("GetCustomDeviceState (device {0:D}): {1:G}", deviceId, VolplaneController.AirConsole.GetCustomDeviceState(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.O))
        {
            Debug.LogFormat("GetNickname (device {0:D}): {1:G}", deviceId, VolplaneController.AirConsole.GetNickname(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            Debug.LogFormat("GetProfilePicture (device {0:D}): {1:G}", deviceId, VolplaneController.AirConsole.GetProfilePicture(
                VolplaneController.AirConsole.GetUID(deviceId)
            ));
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.LogFormat("GetUID (device {0:D}): {1:G}", deviceId, VolplaneController.AirConsole.GetUID(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            Debug.LogFormat("IsUserLoggedIn (device {0:D}): {1:F}", deviceId, VolplaneController.AirConsole.IsUserLoggedIn(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.Y))
        {
            Debug.LogFormat("ConvertDeviceIdToPlayerNumber (device {0:D}): {1:D}", deviceId, VolplaneController.AirConsole.ConvertDeviceIdToPlayerNumber(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            Debug.LogFormat("ConvertPlayerNumberToDeviceId (player {0:D}): {1:D}",
                            VolplaneController.AirConsole.ConvertDeviceIdToPlayerNumber(deviceId),
                            VolplaneController.AirConsole.ConvertPlayerNumberToDeviceId(
                                VolplaneController.AirConsole.ConvertDeviceIdToPlayerNumber(deviceId)));
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("GetActivePlayerDeviceIds:");

            foreach(var id in VolplaneController.AirConsole.GetActivePlayerDeviceIds())
                Debug.Log(id);
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("SetActivePlayers (2)");
            VolplaneController.AirConsole.SetActivePlayers(2);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("SetActivePlayers (0)");
            VolplaneController.AirConsole.SetActivePlayers(0);
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("GetPremiumDeviceIds:");

            foreach(var id in VolplaneController.AirConsole.GetPremiumDeviceIds())
                Debug.Log(id);
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.LogFormat("IsPremium (device {0:D}): {1:F}", deviceId, VolplaneController.AirConsole.IsPremium(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("StorePersistentData");
            VolplaneController.AirConsole.StorePersistentData("persistent", @"{""dataProperty1"":""aString"",""dataProperty2"":42}", VolplaneController.AirConsole.GetUID(deviceId));
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("RequestPersistentData device " + deviceId);
            VolplaneController.AirConsole.RequestPersistentData(VolplaneController.AirConsole.GetUID(deviceId));
        }
	}
}
