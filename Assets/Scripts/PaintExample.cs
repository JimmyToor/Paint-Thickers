using UnityEngine;
using UnityEngine.InputSystem;

public class PaintExample : MonoBehaviour
{
    public Brush brush;
    public bool RandomChannel = false;
    public bool SingleShotClick = false;
    public bool ClearOnClick = false;
    public bool IndexBrush = false;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    private bool HoldingButtonDown = false;

    //private Vector3 rotatePoint = Vector3.zero;

    private void Start()
    {
        colorTex = new Texture2D(1, 1);

        rotationX = transform.eulerAngles.y;
        rotationY = -transform.eulerAngles.x;

        if (brush.splatTexture == null)
        {
            brush.splatTexture = Resources.Load<Texture2D>("splats");
            brush.splatsX = 4;
            brush.splatsY = 4;
        }
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.isPressed) brush.splatChannel = 0;
        if (Keyboard.current.digit2Key.isPressed) brush.splatChannel = 1;
        if (Keyboard.current.digit3Key.isPressed) brush.splatChannel = 2;
        if (Keyboard.current.digit4Key.isPressed) brush.splatChannel = 3;
        if (Keyboard.current.digit5Key.isPressed) brush.splatChannel = 4;

        if (RandomChannel) brush.splatChannel = Random.Range(0, 3);

        if (Mouse.current.leftButton.isPressed)
        {
            if (!SingleShotClick || (SingleShotClick && !HoldingButtonDown))
            {
                if (ClearOnClick) PaintTarget.ClearAllPaint();
                PaintTarget.PaintCursor(brush);
                if (IndexBrush) brush.splatIndex++;
            }
            HoldingButtonDown = true;
        }
        else
        {
            HoldingButtonDown = false;
        }
    }


    private Texture2D colorTex;
    private bool ShowMenu = true;

    private void OnGUI()
    {
        ShowMenu = GUILayout.Toggle(ShowMenu,"");
        if (!ShowMenu) return;

        GUILayout.BeginVertical(GUI.skin.box);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Channel 0")) brush.splatChannel = 0;
        if (GUILayout.Button("Channel 1")) brush.splatChannel = 1;
        if (GUILayout.Button("Erase"))
        {
            brush.splatChannel = 4;
            RandomChannel = false;
            ClearOnClick = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        RandomChannel = GUILayout.Toggle(RandomChannel, "Random");
        SingleShotClick = GUILayout.Toggle(SingleShotClick, "Single Click");
        ClearOnClick = GUILayout.Toggle(ClearOnClick, "Clear on Click");
        IndexBrush = GUILayout.Toggle(IndexBrush, "Index Brush");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Paint Size");
        brush.splatScale = GUILayout.HorizontalSlider(brush.splatScale, .1f, 5f);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear ALL")) PaintTarget.ClearAllPaint();

        //Texture2D c = new Texture2D(1, 1);
        colorTex.SetPixel(0, 0, PaintTarget.CursorColor());
        colorTex.Apply();

        //GUILayout.Box(colorTex, GUILayout.Width(128), GUILayout.Height(32));
        //GUILayout.Box("CURSOR COLOR:" + PaintTarget.CursorColor());

        GUI.DrawTexture(new Rect(0, Screen.height - 32, 32, 32), colorTex);
        GUILayout.Box("CURSOR CHANNEL:" + PaintTarget.CursorChannel());

        GUILayout.EndVertical();
    }
}