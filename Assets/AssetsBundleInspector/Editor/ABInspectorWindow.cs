using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ABInspector
{
    public class ABInspectorWindow : EditorWindow
    {
        private Texture2D Background = null;
        public static ABInspectorWindow WindowInstance;
        private const string editorPath = "Assets/Constellation/Editor/EditorAssets/";
        private Vector2 editorScrollPos = Vector2.zero;
        private Vector2 editorScrollSize = new Vector2(500, 500);
        private Rect LayoutPosition = Rect.zero;
        private ABInspectorEditor m_abEditor = null;

        [MenuItem("Tools/ABInspector")]
        public static void ShowWindow()
        {
            WindowInstance = EditorWindow.GetWindow<ABInspectorWindow>(false, "ABInspectorWindow");
            WindowInstance.minSize = new Vector2(600.0f, 300.0f);
            WindowInstance.wantsMouseMove = true;
            WindowInstance.Show();
        }
        private void Awake()
        {
            m_abEditor = new ABInspectorEditor();
            Debug.Log("Awake");
        }
        private void OnDestroy()
        {
            m_abEditor = null;
            WindowInstance = null;
            Debug.Log("OnDestroy");
        }
        private const float kZoomMin = 0.1f;
        private const float kZoomMax = 10.0f;

        private Rect _zoomArea = new Rect(0.0f, 75.0f, 600.0f, 300.0f - 100.0f);
        private float _zoom = 1.0f;
        private Vector2 _zoomCoordsOrigin = Vector2.zero;

        private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
        {
            return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
        }

        private void DrawZoomArea()
        {
            // Within the zoom area all coordinates are relative to the top left corner of the zoom area
            // with the width and height being scaled versions of the original/unzoomed area's width and height.
            _zoomArea = new Rect(0, 0, position.width - 35, position.height - 35);
            EditorZoomArea.Begin(_zoom, _zoomArea);

            GUI.Box(new Rect(0.0f - _zoomCoordsOrigin.x, 0.0f - _zoomCoordsOrigin.y, 100.0f, 25.0f), "Zoomed Box");

            // You can also use GUILayout inside the zoomed area.
            GUILayout.BeginArea(new Rect(300.0f - _zoomCoordsOrigin.x, 70.0f - _zoomCoordsOrigin.y, 130.0f, 50.0f));
            GUILayout.Button("Zoomed Button 1");
            GUILayout.Button("Zoomed Button 2");
            GUILayout.EndArea();

            GUI.BeginGroup(new Rect(0.0f - _zoomCoordsOrigin.x, 0.0f - _zoomCoordsOrigin.y, 10000f, 10000f));
            //GUI.Button(new Rect(0, 0, 50, 50), "aaa");
            DrawNode();
            DrawLink();
            if (m_abEditor != null)
                m_abEditor.OnGUI();
            GUI.EndGroup();

            EditorZoomArea.End();
        }

        private void DrawNonZoomArea()
        {
            GUI.Box(new Rect(0.0f, 0.0f, 600.0f, 50.0f), "Adjust zoom of middle box with slider or mouse wheel.\nMove zoom area dragging with middle mouse button or Alt+left mouse button.");
            _zoom = EditorGUI.Slider(new Rect(0.0f, 50.0f, 600.0f, 25.0f), _zoom, kZoomMin, kZoomMax);
            GUI.Box(new Rect(0.0f, 300.0f - 25.0f, 600.0f, 25.0f), "Unzoomed Box");
        }

        private void HandleEvents()
        {
            // Allow adjusting the zoom with the mouse wheel as well. In this case, use the mouse coordinates
            // as the zoom center instead of the top left corner of the zoom area. This is achieved by
            // maintaining an origin that is used as offset when drawing any GUI elements in the zoom area.
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 screenCoordsMousePos = Event.current.mousePosition;
                Vector2 delta = Event.current.delta;
                Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
                float zoomDelta = -delta.y / 150.0f;
                float oldZoom = _zoom;
                _zoom += zoomDelta;
                _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
                _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);

                Event.current.Use();
            }

            // Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
            // with the left mouse button with Alt pressed.
            if (Event.current.type == EventType.MouseDrag &&
                (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
                Event.current.button == 2)
            {
                Vector2 delta = Event.current.delta;
                delta /= _zoom;
                _zoomCoordsOrigin += delta;

                Event.current.Use();
            }
        }

        public void OnGUI()
        {
            HandleEvents();
            DrawBackgroundUI();

            // The zoom area clipping is sometimes not fully confined to the passed in rectangle. At certain
            // zoom levels you will get a line of pixels rendered outside of the passed in area because of
            // floating point imprecision in the scaling. Therefore, it is recommended to draw the zoom
            // area first and then draw everything else so that there is no undesired overlap.
            DrawZoomArea();
            //DrawNonZoomArea();

        }


        private void DrawBackgroundUI()
        {
            LayoutPosition = new Rect(0, 35, position.width - 35, position.height - 35);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos, false, false, GUILayout.Width(LayoutPosition.width), GUILayout.Height(LayoutPosition.height));
            GUILayoutOption[] options = { GUILayout.Width(editorScrollSize.x), GUILayout.Height(editorScrollSize.y) };
            EditorGUILayout.LabelField("", options);
            DrawBackground(LayoutPosition.width, LayoutPosition.height, editorScrollSize.x, editorScrollSize.y, Color.white);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBackground(float _width, float _height, float offsetX, float offsetY, Color tint)
        {
            if (Background == null)
            {
                Background = AssetDatabase.LoadAssetAtPath(editorPath + "background.png", typeof(Texture2D)) as Texture2D;
            }

            //Background location based of current location allowing unlimited background
            //How many background are needed to fill the background
            var xCount = Mathf.Round(_width / Background.width) + 2;
            var yCount = Mathf.Round(_height / Background.height) + 2;
            //Current scroll offset for background
            var xOffset = Mathf.Round(offsetX / Background.width) - 1;
            var yOffset = Mathf.Round(offsetY / Background.height) - 1;
            var texRect = new Rect(0, 0, Background.width, Background.height);
            // if (isInstance && constellationScript.IsDifferentThanSource)
            GUI.color = (tint);
            for (var i = xOffset; i < xOffset + xCount; i++)
            {
                for (var j = yOffset; j < yOffset + yCount; j++)
                {
                    texRect.x = i * Background.width;
                    texRect.y = j * Background.height;
                    GUI.DrawTexture(texRect, Background);
                }
            }
            GUI.color = Color.white;
        }


        Rect nodeRect = new Rect(20, 20, 300, 300);
        private void DrawNode()
        {
            BeginWindows();
            int i = 0;

            GUIStyle NoteStyle = GUI.skin.GetStyle("VCS_StickyNote");
            nodeRect = GUI.Window(i, nodeRect, DrawNodeContent, "node", NoteStyle);
            EndWindows();
        }
        private void DrawNodeContent(int id)
        {
            GUI.Label(new Rect(10, 20, 100, 20), "1212");
        }

        private void DrawLink()
        {
            Color color = Color.red;

            Vector3 startPos = new Vector3(300, 300, 0);
            Vector3 endPos = new Vector3(400, 520, 0);

            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;

            var distance = Vector3.Distance(startPos, endPos);
            if (distance < 100)
            {
                startTan = startPos + Vector3.right * (distance * 0.5f);
                endTan = endPos + Vector3.left * (distance * 0.5f);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 5);
        }
    }


    public class EditorZoomArea
    {
        private const float kEditorWindowTabHeight = 21.0f;
        private static Matrix4x4 _prevGuiMatrix;

        public static Rect Begin(float zoomScale, Rect screenCoordsArea)
        {
            GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.

            Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());
            clippedArea.y += kEditorWindowTabHeight;
            GUI.BeginGroup(clippedArea);

            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

            return clippedArea;
        }

        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
        }
    }

    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }
}