using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class AutoCapture
{
    static AutoCapture()
    {
        EditorApplication.delayCall += Capture;
    }

    [MenuItem("Tools/Event System/Capture Scene")]
    public static void Capture()
    {
        var view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            Debug.LogError("No active scene view");
            return;
        }
        var cam = view.camera;
        if (cam == null) return;
        int w = (int)view.position.width;
        int h = (int)view.position.height;
        if (w <= 0 || h <= 0) return;
        
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("/tmp/unity_scene.png", bytes);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(rt);
        Debug.Log("Scene view captured to /tmp/unity_scene.png");
    }
}
