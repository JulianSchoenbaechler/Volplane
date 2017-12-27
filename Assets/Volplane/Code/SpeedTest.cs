using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Volplane;
using SimpleJSON;
using Newtonsoft.Json.Linq;

public class SpeedTest : MonoBehaviour
{
    public Stopwatch stopwatch;

    private void Awake()
    {
        stopwatch = Stopwatch.StartNew();
    }

    private void Start()
    {
        VolplaneController.AirConsole.OnMessage += Message;
    }

    private void Message(int device, JSONNode data)
    {
        long receiveTime = VolplaneController.AirConsole.GetServerTime();
        long sendTime = data["volplane"]["data"]["timeStamp"].AsLong;

        stopwatch.Stop();
        long processing = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.LogFormat("Connection: {0}\nProcessing: {1}", receiveTime - sendTime, processing);
    }
}
