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

    public void ResetAndStart()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    public void Stop()
    {
        stopwatch.Stop();
        UnityEngine.Debug.LogFormat("Stopwatch: {0}ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
    }
}
