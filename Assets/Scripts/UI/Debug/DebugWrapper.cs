using UnityEngine;
using System;
using System.Collections.Generic;

public class DebugWrapper : Singleton<DebugWrapper>
{
    private List<DebugMessageDisplayer> displayers;
    
    protected override void Awake()
    {
        base.Awake();
        displayers = new ();
    }

    public void RegisterDisplayer(DebugMessageDisplayer displayer)
    {
        if (!displayers.Contains(displayer))
            displayers.Add(displayer);
    }

    public void UnregisterDisplayer(DebugMessageDisplayer displayer)
    {
        displayers.Remove(displayer);
    }

    public void Log(string message, string color = "black")
    {
        foreach (var displayer in displayers)
        {
            displayer.EnqueueMessage(message, color); // 分发给每个 Displayer
        }
    }

    public void Log(string message, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        Log(message, $"#{colorHex}");
    }
}