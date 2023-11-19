using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public struct SpeedTimer
{
    Stopwatch stopwatch;
    string title;

    public SpeedTimer(string title)
    {
        this.title = title;

        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    public void StopAndLog()
    {
        stopwatch.Stop();
        UnityEngine.Debug.Log($"{title} Use Time: {stopwatch.ElapsedTicks / 10_000} ms");
    }
}