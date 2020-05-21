// Simple Zoom - https://assetstore.unity.com/packages/tools/gui/simple-zoom-143625
// Version: 1.0.2
// Author: Daniel Lochner

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleZoom
{
    [CustomEditor(typeof(SimpleZoom))]
    public class SimpleZoomEditor : Editor
    {
        #region Fields
        private SimpleZoom simpleZoom;
        private SerializedProperty currentZoom, minZoom, maxZoom, zoomTarget, customPosition, zoomType, elasticLimit, elasticDamping, zoomMode, zoomIn, zoomInPosition, zoomInIncrement, zoomInSmoothing, zoomOut, zoomOutPosition, zoomOutIncrement, zoomOutSmoothing, zoomSlider, zoomView, zoomMovement, doubleTap, doubleTapTargetTime, scrollWheelIncrement, scrollWheelSmoothing;
        private bool showBasic = true, showControl = true, showOther = true, showScrollWheel = false;
        #endregion

        #region Methods
        private void OnEnable()
        {
            simpleZoom = target as SimpleZoom;

            // Basic Settings.
            currentZoom = serializedObject.FindProperty("currentZoom");
            minZoom = serializedObject.FindProperty("minZoom");
            maxZoom = serializedObject.FindProperty("maxZoom");
            zoomTarget = serializedObject.FindProperty("zoomTarget");
            customPosition = serializedObject.FindProperty("customPosition");
            zoomType = serializedObject.FindProperty("zoomType");
            elasticLimit = serializedObject.FindProperty("elasticLimit");
            elasticDamping = serializedObject.FindProperty("elasticDamping");
            zoomMode = serializedObject.FindProperty("zoomMode");

            // Control Settings.
            zoomIn = serializedObject.FindProperty("zoomIn");
            zoomInPosition = serializedObject.FindProperty("zoomInPosition");
            zoomInIncrement = serializedObject.FindProperty("zoomInIncrement");
            zoomInSmoothing = serializedObject.FindProperty("zoomInSmoothing");
            zoomOut = serializedObject.FindProperty("zoomOut");
            zoomOutPosition = serializedObject.FindProperty("zoomOutPosition");
            zoomOutIncrement = serializedObject.FindProperty("zoomOutIncrement");
            zoomOutSmoothing = serializedObject.FindProperty("zoomOutSmoothing");
            zoomSlider = serializedObject.FindProperty("zoomSlider");
            zoomView = serializedObject.FindProperty("zoomView");

            // Other Settings.
            zoomMovement = serializedObject.FindProperty("zoomMovement");
            doubleTap = serializedObject.FindProperty("doubleTap");
            doubleTapTargetTime = serializedObject.FindProperty("doubleTapTargetTime");
            scrollWheelIncrement = serializedObject.FindProperty("scrollWheelIncrement");
            scrollWheelSmoothing = serializedObject.FindProperty("scrollWheelSmoothing");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HeaderInformation();
            BasicSettings();
            ControlSettings();
            OtherSettings();

            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(simpleZoom);
        }

        private void HeaderInformation()
        {
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Simple Zoom", new GUIStyle() { fontSize = 30, alignment = TextAnchor.MiddleCenter });
            GUILayout.Label("Version: 1.0.2", new GUIStyle() { fontSize = 14, alignment = TextAnchor.MiddleCenter });
            GUILayout.Label("Author: Daniel Lochner", new GUIStyle() { fontSize = 14, alignment = TextAnchor.MiddleCenter });
            GUILayout.EndVertical();
        }

        private void BasicSettings()
        {
            EditorGUILayout.Space();

            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            showBasic = EditorGUILayout.Foldout(showBasic, "Basic Settings", true);
            EditorStyles.foldout.fontStyle = FontStyle.Normal;

            if (showBasic)
            {
                EditorGUILayout.Slider(new GUIContent("Current Zoom", "The current value by which the scale/size is multiplied by when zooming."), simpleZoom.currentZoom, simpleZoom.minZoom, simpleZoom.maxZoom);
                EditorGUILayout.PropertyField(minZoom, new GUIContent("Min Zoom", "The minimum value by which the scale/size could be multiplied by when zooming."));
                EditorGUILayout.PropertyField(maxZoom, new GUIContent("Max Zoom", "The maximum value by which the scale/size could be multiplied by when zooming."));
                EditorGUILayout.PropertyField(zoomTarget, new GUIContent("Zoom Target", "Determines what value the content's pivot will be set to when zooming."));
                if (simpleZoom.zoomTarget == SimpleZoom.ZoomTarget.Custom)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(customPosition, new GUIContent("Position", "The custom value the pivot will be set to when zooming."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(zoomType, new GUIContent("Zoom Type", "Determines whether zooming may exceed the minimum/maximum zoom values and how it should be handled."));
                if (simpleZoom.zoomType == SimpleZoom.ZoomType.Elastic)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(elasticLimit, new GUIContent("Limit", "The amount by which the zoom can exceed the minimum/maximum zoom values."));
                    EditorGUILayout.PropertyField(elasticDamping, new GUIContent("Damping", "The amount by which the zoom is dampened when exceeding the minimum/maximum zoom values."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(zoomMode, new GUIContent("Zoom Mode", "Determines whether zooming transforms the scale or size (width/height) when zooming."));
            }

            EditorGUILayout.Space();
        }
        private void ControlSettings()
        {
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            showControl = EditorGUILayout.Foldout(showControl, "Control Settings", true);
            EditorStyles.foldout.fontStyle = FontStyle.Normal;

            if (showControl)
            {
                EditorGUILayout.ObjectField(zoomIn, typeof(Button), new GUIContent("Zoom In", "The button whose OnClick event listener will be assigned the \"ZoomIn\" method invocation on setup with values specified below."));
                if (simpleZoom.zoomIn != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(zoomInPosition, new GUIContent("Position", "The custom value the pivot will be set to when zooming in."));
                    EditorGUILayout.PropertyField(zoomInIncrement, new GUIContent("Increment", "The amount by which the current zoom will be transformed by when zooming in."));
                    EditorGUILayout.PropertyField(zoomInSmoothing, new GUIContent("Smoothing", "The smoothing by which the current zoom will be transformed into the incremented zoom when zooming in."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.ObjectField(zoomOut, typeof(Button), new GUIContent("Zoom Out", "The button whose OnClick event listener will be assigned the \"ZoomOut\" method invocation on setup with values specified below."));
                if (simpleZoom.zoomOut != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(zoomOutPosition, new GUIContent("Position", "The custom value the pivot will be set to when zooming out."));
                    EditorGUILayout.PropertyField(zoomOutIncrement, new GUIContent("Increment", "The amount by which the current zoom will be transformed by when zooming out."));
                    EditorGUILayout.PropertyField(zoomOutSmoothing, new GUIContent("Smoothing", "The smoothing by which the current zoom will be transformed into the incremented zoom when zooming out."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.ObjectField(zoomSlider, typeof(Slider), new GUIContent("Zoom Slider", "A slider that displays the progress of the zoom represented by a number between 0 (minimum) and 1 (maximum)."));
                EditorGUILayout.ObjectField(zoomView, typeof(GameObject), new GUIContent("Zoom View", "A small scale representation of the zoom."));
            }

            EditorGUILayout.Space();
        }
        private void OtherSettings()
        {
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            showOther = EditorGUILayout.Foldout(showOther, "Other Settings", true);
            EditorStyles.foldout.fontStyle = FontStyle.Normal;

            if (showOther)
            {
                EditorGUILayout.PropertyField(zoomMovement, new GUIContent("Zoom Movement", "Should the ScrollRect's movement be restricted when zooming?"));
                EditorGUILayout.PropertyField(doubleTap, new GUIContent("Double Tap", "Should users be able to double-tap to fully zoom in/out depending on the current zoom?"));
                if (simpleZoom.doubleTap)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(doubleTapTargetTime, new GUIContent("Target Time", "The amount of time users have in order for a double-tap to register."));
                    EditorGUI.indentLevel--;
                }
                showScrollWheel = EditorGUILayout.Foldout(showScrollWheel, "Scroll Wheel", true);
                if (showScrollWheel)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(scrollWheelIncrement, new GUIContent("Increment", "The amount by which the current zoom will be transformed by when scrolling in/out."));
                    EditorGUILayout.PropertyField(scrollWheelSmoothing, new GUIContent("Smoothing", "The smoothing by which the current zoom will be transformed into the incremented zoom when scrolling in/out."));
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();
        }

        [MenuItem("GameObject/UI/Simple Zoom/Zoom")]
        private static void CreateZoom()
        {
            // Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create " + canvasObject.name);
            }

            // Zoom
            GameObject zoom = new GameObject("Zoom");
            RectTransform zoomRectTransform = zoom.AddComponent<RectTransform>();
            zoomRectTransform.sizeDelta = new Vector2(400, 400);
            MultiTouchScrollRect zoomScrollRect = zoom.AddComponent<MultiTouchScrollRect>();
            zoomScrollRect.horizontal = true;
            zoomScrollRect.vertical = true;
            zoomScrollRect.scrollSensitivity = 0f;
            zoomScrollRect.decelerationRate = 0.01f;
            GameObjectUtility.SetParentAndAlign(zoom, canvas.gameObject);
            zoom.AddComponent<SimpleZoom>();

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            RectTransform viewportRectTransform = viewport.AddComponent<RectTransform>();
            viewportRectTransform.anchorMin = new Vector2(0, 0);
            viewportRectTransform.anchorMax = new Vector2(1, 1);
            viewportRectTransform.offsetMin = Vector2.zero;
            viewportRectTransform.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>();
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.5f);
            zoomScrollRect.viewport = viewportRectTransform;
            GameObjectUtility.SetParentAndAlign(viewport, zoom.gameObject);

            // Content
            GameObject content = new GameObject("Content");
            RectTransform contentRectTransform = content.AddComponent<RectTransform>();
            contentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            contentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            contentRectTransform.sizeDelta = new Vector2(400, 400);
            contentRectTransform.pivot = new Vector2(0.5f, 0.5f);
            Image contentImage = content.AddComponent<Image>();
            contentImage.color = new Color(1, 1, 1, 1f);
            zoomScrollRect.content = contentRectTransform;
            GameObjectUtility.SetParentAndAlign(content, viewport.gameObject);

            // Event System
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventObject.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
            }

            // Editor
            Selection.activeGameObject = zoom;
            Undo.RegisterCreatedObjectUndo(zoom, "Create " + zoom.name);
        }
        [MenuItem("GameObject/UI/Simple Zoom/Zoom View")]
        private static void CreateZoomView()
        {
            // Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create " + canvasObject.name);
            }

            // Zoom View
            GameObject zoom = new GameObject("Zoom View");
            RectTransform zoomRectTransform = zoom.AddComponent<RectTransform>();
            zoomRectTransform.sizeDelta = new Vector2(100, 100);
            GameObjectUtility.SetParentAndAlign(zoom, canvas.gameObject);
            Image zoomImage = zoom.AddComponent<Image>();
            zoomImage.color = new Color(1, 1, 1, 0.5f);

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            RectTransform viewportRectTransform = viewport.AddComponent<RectTransform>();
            viewportRectTransform.anchorMin = new Vector2(0, 0);
            viewportRectTransform.anchorMax = new Vector2(1, 1);
            viewportRectTransform.offsetMin = Vector2.zero;
            viewportRectTransform.offsetMax = Vector2.zero;
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 1);
            GameObjectUtility.SetParentAndAlign(viewport, zoom.gameObject);

            // Event System
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventObject.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
            }

            // Editor
            Selection.activeGameObject = zoom;
            Undo.RegisterCreatedObjectUndo(zoom, "Create " + zoom.name);
        }
        #endregion
    }
}