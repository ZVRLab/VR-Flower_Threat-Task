using UnityEngine;

public static class GameTimer
{
    public static float SceneStartTime { get; private set; }
    
    public static void MarkSceneStart()
    {
        SceneStartTime = Time.time;
    }
    
    public static float Elapsed => Time.time - SceneStartTime;
}