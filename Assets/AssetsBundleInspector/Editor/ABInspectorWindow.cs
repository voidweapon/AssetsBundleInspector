using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ABInspector
{
    public class ABInspectorWindow : EditorWindow
    {
        private Texture2D Background = null;
        public static ABInspectorWindow WindowInstance;
        private const string editorPath = "Assets/AssetsBundleInspector/Editor/EditorAssets/";
        private Vector2 editorScrollPos = Vector2.zero;
        private Vector2 editorScrollSize = new Vector2(500, 500);
        private Rect VisualWindowRect;
        private Rect LayoutPosition = Rect.zero;
        private ABInspectorEditor m_abEditor = null;

        private TreeViewState OrganizationTreeViewState = null;
        private ABInspectorOrganizationTreeView OrganizationTreeView = null;
        private Rect OrganizationTreeViewRect = Rect.zero;

        private Object selectObj = null;

        [MenuItem("Tools/ABInspector")]
        public static void ShowWindow()
        {
            WindowInstance = EditorWindow.GetWindow<ABInspectorWindow>(false, "ABInspectorWindow");
            WindowInstance.minSize = new Vector2(1200.0f, 600.0f);
            WindowInstance.wantsMouseMove = true;
            WindowInstance.titleContent = new GUIContent("ABInspector");
            WindowInstance.Show();
        }
        private void Awake()
        {
            m_abEditor = new ABInspectorEditor();
            m_abEditor.Init();

            OrganizationTreeViewState = new TreeViewState();
            var headerState = ABInspectorOrganizationTreeView.CreateDefaultMultiColumnHeaderState();
            OrganizationTreeView = new ABInspectorOrganizationTreeView(OrganizationTreeViewState, headerState);
            OrganizationTreeView.GetABInspectorItemDataByGUID = m_abEditor.GetItemByGUID;
            OrganizationTreeView.Reload();

            Debug.Log("Awake");
        }
        private void OnDestroy()
        {
            m_abEditor.OnDestory();
            m_abEditor = null;
            WindowInstance = null;
            selectObj = null;
            OrganizationTreeViewState = null;
            OrganizationTreeView = null;

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
            VisualWindowRect = new Rect(0, 0, position.width - 300, position.height);
            _zoomArea = VisualWindowRect;
            EditorZoomArea.Begin(_zoom, _zoomArea);


            // You can also use GUILayout inside the zoomed area.
            //GUILayout.BeginArea(new Rect(300.0f - _zoomCoordsOrigin.x, 70.0f - _zoomCoordsOrigin.y, 130.0f, 50.0f));
            //GUILayout.Button("Zoomed Button 1");
            //GUILayout.Button("Zoomed Button 2");
            //GUILayout.EndArea();

            GUI.BeginGroup(new Rect(0.0f - _zoomCoordsOrigin.x, 0.0f - _zoomCoordsOrigin.y, 10000f, 10000f));

            if (m_abEditor != null && m_abEditor.Ready)
            {

                BeginWindows();
                m_abEditor.DrawNode();
                EndWindows();

                m_abEditor.DrawLink();
            }
            GUI.EndGroup();

            EditorZoomArea.End();
        }

        private void DrawNonZoomArea()
        {
            if (OrganizationTreeView != null)
            {
                OrganizationTreeViewRect = new Rect(position.width - 300, 0, 300, position.height);
                OrganizationTreeView.OnGUI(OrganizationTreeViewRect);
            }
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
            DrawNonZoomArea();

        }


        private void DrawBackgroundUI()
        {
            LayoutPosition = VisualWindowRect;

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

        private void OnSelectionChange()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (Selection.objects.Length > 0 
                && Selection.objects[0]  != null
                && Equals(selectObj, Selection.objects[0]) == false)
            {
                selectObj = Selection.objects[0];
                string guid;
                long localid;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(selectObj, out guid, out localid))
                {
                    m_abEditor.SelectNode(guid);
                    OrganizationTreeView.ShowSelectNodeOrganization(guid);
                    OrganizationTreeView.Reload();
                    Repaint();
                }
            }
            stopwatch.Stop();
            System.TimeSpan timeSpan = stopwatch.Elapsed;
            Debug.Log(timeSpan.TotalMilliseconds);
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