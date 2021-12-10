// Simple Zoom - https://assetstore.unity.com/packages/tools/gui/simple-zoom-143625
// Copyright (c) Daniel Lochner

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DanielLochner.Assets.SimpleZoom
{
    [AddComponentMenu("UI/Simple Zoom")]
    [RequireComponent(typeof(ScrollRect))]
    public class SimpleZoom : MonoBehaviour, IPointerClickHandler
    {
        #region Fields
        // Basic Settings
        [SerializeField] private float currentZoom = 1f;
        [SerializeField] private MinMax minMaxZoom = new MinMax(1, 3);
        [SerializeField] private ZoomTarget zoomTarget = ZoomTarget.Pointer;
        [SerializeField] private Vector2 customPosition = new Vector2(0.5f, 0.5f);
        [SerializeField] private ZoomType zoomType = ZoomType.Clamped;
        [SerializeField] private float elasticLimit = 2f;
        [SerializeField] private float elasticDamping = 0.1f;
        [SerializeField] private ZoomMode zoomMode = ZoomMode.Scale;

        // Control Settings
        [SerializeField] private Button zoomInButton = null;
        [SerializeField] private Vector2 zoomInPosition = new Vector2(0.5f, 0.5f);
        [SerializeField] private float zoomInIncrement = 0.5f;
        [SerializeField] private float zoomInSmoothing = 0.1f;
        [SerializeField] private Button zoomOutButton = null;
        [SerializeField] private Vector2 zoomOutPosition = new Vector2(0.5f, 0.5f);
        [SerializeField] private float zoomOutIncrement = 0.5f;
        [SerializeField] private float zoomOutSmoothing = 0.1f;
        [SerializeField] private Slider zoomSlider = null;
        [SerializeField] private GameObject zoomView = null;

        // Other Settings
        [SerializeField] private bool zoomMovement = true;
        [SerializeField] private bool doubleTap = true;
        [SerializeField] private float doubleTapTargetTime = 0.25f;
        [SerializeField] private float scrollWheelIncrement = 0.5f;
        [SerializeField] private float scrollWheelSmoothing = 0.1f;

        private float initialZoom = 1f, currentDistance = 1f, initialDistance = 1f, doubleTapTime, targetSmoothing, zoomViewScale;
        private bool isInitialTouch = true;
        private int taps = 0;
        private Vector2 mouseLocalPosition, originalSize, originalScale;
        private RectTransform zoomViewViewport;
        private ScrollRect scrollRect;
        private Canvas canvas;
        #endregion

        #region Properties
        public MinMax MinMaxZoom
        {
            get => minMaxZoom;
            set => minMaxZoom = value;
        }
        public ZoomTarget ZoomTarget
        {
            get => zoomTarget;
            set => zoomTarget = value;
        }
        public Vector2 CustomPosition
        {
            get => customPosition;
            set => customPosition = value;
        }
        public ZoomType ZoomType
        {
            get => zoomType;
            set => zoomType = value;
        }
        public float ElasticLimit
        {
            get => elasticLimit;
            set => elasticLimit = value;
        }
        public float ElasticDamping
        {
            get => elasticDamping;
            set => elasticDamping = value;
        }
        public ZoomMode ZoomMode
        {
            get => zoomMode;
            set => zoomMode = value;
        }
        public Button ZoomInButton
        {
            get => zoomInButton;
            set => zoomInButton = value;
        }
        public Vector2 ZoomInPosition
        {
            get => zoomInPosition;
            set => zoomInPosition = value;
        }
        public float ZoomInIncrement
        {
            get => zoomInIncrement;
            set => zoomInIncrement = value;
        }
        public float ZoomInSmoothing
        {
            get => zoomInSmoothing;
            set => zoomInSmoothing = value;
        }
        public Button ZoomOutButton
        {
            get => zoomOutButton;
            set => zoomOutButton = value;
        }
        public Vector2 ZoomOutPosition
        {
            get => zoomOutPosition;
            set => zoomOutPosition = value;
        }
        public float ZoomOutIncrement
        {
            get => zoomOutIncrement;
            set => zoomOutIncrement = value;
        }
        public float ZoomOutSmoothing
        {
            get => zoomOutSmoothing;
            set => zoomOutSmoothing = value;
        }
        public Slider ZoomSlider
        {
            get => zoomSlider;
            set => zoomSlider = value;
        }
        public GameObject ZoomView
        {
            get => zoomView;
            set => zoomView = value;
        }
        public bool ZoomMovement
        {
            get => zoomMovement;
            set => zoomMovement = value;
        }
        public bool DoubleTap
        {
            get => doubleTap;
            set => doubleTap = value;
        }
        public float DoubleTapTargetTime
        {
            get => doubleTapTargetTime;
            set => doubleTapTargetTime = value;
        }
        public float ScrollWheelIncrement
        {
            get => scrollWheelIncrement;
            set => scrollWheelIncrement = value;
        }
        public float ScrollWheelSmoothing
        {
            get => scrollWheelSmoothing;
            set => scrollWheelSmoothing = value;
        }
        
        private RectTransform Content
        {
            get { return scrollRect.content; }
        }
        private RectTransform Viewport
        {
            get { return scrollRect.viewport; }
        }

        public float TargetZoom { get; set; } = 1f;
        public float CurrentZoom
        {
            get => currentZoom;
            set => currentZoom = value;
        }

        public float ZoomProgress
        {
            get
            {
                return (currentZoom - minMaxZoom.min) / (minMaxZoom.max - minMaxZoom.min);
            }
        }
        public Vector4 ZoomMargin
        {
            get
            {
                Vector2 viewportPivot = Viewport.pivot;
                Vector2 contentPivot = Content.pivot;

                Vector2 viewportSize = Viewport.rect.size * Viewport.localScale;
                Vector2 contentSize = Content.rect.size * Content.localScale;

                float left = (Viewport.localPosition.x - viewportSize.x * viewportPivot.x) - (Content.localPosition.x - contentSize.x * contentPivot.x);
                float bottom = (Viewport.localPosition.y - viewportSize.y * viewportPivot.y) - (Content.localPosition.y - contentSize.y * contentPivot.y);

                float right = (Content.localPosition.x + contentSize.x * (1 - contentPivot.x)) - (Viewport.localPosition.x + viewportSize.x * (1 - viewportPivot.x));
                float top = (Content.localPosition.y + contentSize.y * (1 - contentPivot.y)) - (Viewport.localPosition.y + viewportSize.y * (1 - viewportPivot.y));

                return new Vector4(left, right, bottom, top);
            }
        }

        public static bool UsingUnityRemote
        {
            get
            {
                #if (UNITY_EDITOR)
                    return UnityEditor.EditorApplication.isRemoteConnected;
                #else
                    return false;
                #endif
            }
        }
        #endregion
        
        #region Methods
        private void Start()
        {
            Initialize();

            if (Validate())
            {
                Setup();
            }
            else
            {
                throw new Exception("Invalid configuration.");
            }
        }
        private void Update()
        {
            OnPivotZoomUpdate();

            OnZoomSlider();
            OnZoomView();
            OnDoubleTap();
        }
        #if UNITY_EDITOR
        private void OnValidate()
        {
            Initialize();
        }
        #endif

        public void OnPointerClick(PointerEventData eventData)
        {
            if (taps == 0) doubleTapTime = doubleTapTargetTime;
            if (Input.touchCount <= 1) taps++;
        }

        private void Initialize()
        {
            scrollRect = GetComponent<ScrollRect>();
            canvas = GetComponentInParent<Canvas>();
        }
        private bool Validate()
        {
            bool valid = true;

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                Debug.LogError("<b>[SimpleZoom]</b> The world camera must be assigned if the render mode has been set to \"Screen Space - Camera\".", gameObject);
                valid = false;
            }

            if (zoomView)
            {
                Vector2 viewportDimensions = Viewport.rect.size;
                zoomViewViewport = zoomView.transform.GetChild(0).GetComponent<RectTransform>();
                Vector2 zoomViewViewportDimensions = zoomViewViewport.rect.size;

                Vector2 contentDimensions = Content.rect.size;
                Vector2 zoomViewContentDimensions = zoomView.GetComponent<RectTransform>().rect.size;

                if (Vector2.Distance(viewportDimensions / zoomViewViewportDimensions, contentDimensions / zoomViewContentDimensions) > 0f)
                {
                    Debug.LogError("<b>[SimpleZoom]</b> The Zoom View's dimensions must be SIMILAR (i.e. the ratios of the lengths of their corresponding sides are equal) to that of the Simple Zoom's dimensions.", gameObject);
                    valid = false;
                }
            }

            return valid;
        }
        private void Setup()
        {
            // Canvas & Camera
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.planeDistance = (canvas.GetComponent<RectTransform>().rect.height / 2f) / Mathf.Tan((canvas.worldCamera.fieldOfView / 2f) * Mathf.Deg2Rad);
                if (canvas.worldCamera.farClipPlane < canvas.planeDistance)
                {
                    canvas.worldCamera.farClipPlane = Mathf.Ceil(canvas.planeDistance);
                }
            }

            // Content.
            originalSize = Content.rect.size;
            originalScale = Content.localScale;

            Content.anchorMin = new Vector2(0.5f, 0.5f);
            Content.anchorMax = new Vector2(0.5f, 0.5f);

            // Zoom Buttons.
            if (zoomInButton != null)
                zoomInButton.onClick.AddListener(delegate { ZoomIn(zoomInPosition, zoomInIncrement, zoomInSmoothing); });
            if (zoomOutButton != null)
                zoomOutButton.onClick.AddListener(delegate { ZoomOut(zoomOutPosition, zoomOutIncrement, zoomOutSmoothing); });
        }

        private void OnPivotZoomUpdate()
        {
            #region Set
            if (SystemInfo.deviceType == DeviceType.Handheld || UsingUnityRemote){
                OnHandheldUpdate();
            }else if (SystemInfo.deviceType == DeviceType.Desktop){
                OnDesktopUpdate();
            }
            #endregion

            #region Update
            if (Math.Round(currentZoom, 4) != TargetZoom)
            {
                if (targetSmoothing != 0)
                {
                    currentZoom = Mathf.Lerp(currentZoom, TargetZoom, Time.unscaledDeltaTime * (1f / targetSmoothing));
                }
                else
                {
                    currentZoom = TargetZoom;
                }
            }
            else
            {
                currentZoom = TargetZoom;
            }

            if (zoomMode == ZoomMode.Scale)
            {
                Content.localScale = originalScale * currentZoom;
            }
            else if (zoomMode == ZoomMode.Size)
            {
                Content.sizeDelta = originalSize * currentZoom;
            }
            #endregion
        }
        private void OnHandheldUpdate()
        {
            if (Input.touchCount >= 2)
            {
                #region Set Pivot
                Vector2 pos1 = Input.touches[0].position;
                Vector2 pos2 = Input.touches[1].position;

                Vector2 inputPosition = Vector2.zero;
                switch (zoomTarget)
                {
                    case ZoomTarget.Pointer:
                        inputPosition = (pos1 + pos2) / 2;
                        break;
                    case ZoomTarget.Custom:
                        inputPosition = customPosition * new Vector2(Screen.width, Screen.height);
                        break;
                }
                currentDistance = Vector2.Distance(pos1, pos2);

                if (isInitialTouch)
                {
                    Vector2 pivot = Vector2.zero;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, inputPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out mouseLocalPosition))
                    {
                        float x = Content.pivot.x + (mouseLocalPosition.x / Content.rect.width);
                        float y = Content.pivot.y + (mouseLocalPosition.y / Content.rect.height);
                        pivot = new Vector2(x, y);
                    }
                    SetPivot(pivot);

                    initialDistance = currentDistance;
                    isInitialTouch = false;
                }
                #endregion

                #region Set Zoom
                if (zoomType == ZoomType.Clamped){
                    SetZoom(Mathf.Clamp(initialZoom * (currentDistance / initialDistance), minMaxZoom.min, minMaxZoom.max));
                }else if (zoomType == ZoomType.Elastic){
                    SetZoom(ElasticClamp(initialZoom * (currentDistance / initialDistance), minMaxZoom.min, minMaxZoom.max, elasticLimit, elasticDamping));
                }
                scrollRect.horizontal = scrollRect.vertical = zoomMovement;
                #endregion
            }
            else
            {
                #region Reset Zoom
                initialZoom = currentZoom;

                if (zoomType == ZoomType.Elastic)
                {
                    if (currentZoom > minMaxZoom.max){
                        SetZoom(minMaxZoom.max, 0.1f);
                    }else if (currentZoom < minMaxZoom.min){
                        SetZoom(minMaxZoom.min, 0.1f);
                    }
                }

                scrollRect.horizontal = scrollRect.vertical = true;
                isInitialTouch = true;
                #endregion
            }
        }
        private void OnDesktopUpdate()
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0)
            {
                #region Set Pivot
                Vector2 inputPosition = Vector2.zero;
                switch (zoomTarget)
                {
                    case ZoomTarget.Pointer:
                        inputPosition = Input.mousePosition;
                        break;
                    case ZoomTarget.Custom:
                        inputPosition = customPosition * new Vector2(Screen.width, Screen.height);
                        break;
                }
                Vector2 pivot = Vector2.zero;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, inputPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out mouseLocalPosition))
                {
                    float x = Content.pivot.x + (mouseLocalPosition.x / Content.rect.width);
                    float y = Content.pivot.y + (mouseLocalPosition.y / Content.rect.height);
                    pivot = new Vector2(x, y);
                }
                SetPivot(pivot);
                #endregion

                #region Set Zoom
                SetZoom(Mathf.Clamp(currentZoom + ((scrollWheel * 10) * scrollWheelIncrement), minMaxZoom.min, minMaxZoom.max), scrollWheelSmoothing);
                #endregion
            }
        }
        private void OnDoubleTap()
        {
            if (doubleTap)
            {
                doubleTapTime -= Time.unscaledDeltaTime;
                if (doubleTapTime <= 0f)
                {
                    taps = 0; // Reset.
                }
                else if (taps >= 2)
                {
                    if (Math.Round(currentZoom, 2) > minMaxZoom.min)
                    {
                        SetPivot(new Vector2(ZoomMargin.x / (ZoomMargin.x + ZoomMargin.y), ZoomMargin.z / (ZoomMargin.z + ZoomMargin.w)));
                        SetZoom(minMaxZoom.min, 0.1f);
                    }
                    else
                    {
                        Vector2 outputPosition = Vector2.zero;
                        Vector2 pivot = Vector2.zero;

                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, Input.mousePosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out outputPosition))
                        {
                            float x = Content.pivot.x + (outputPosition.x / Content.rect.width);
                            float y = Content.pivot.y + (outputPosition.y / Content.rect.height);
                            pivot = new Vector2(x, y);
                        }
                        SetPivot(pivot);
                        SetZoom(minMaxZoom.max, 0.1f);
                    }

                    taps = 0; // Reset.
                }
            }
        }
        private void OnZoomSlider()
        {
            if (zoomSlider != null)
            {
                zoomSlider.value = ZoomProgress;
            }
        }
        private void OnZoomView()
        {
            if (zoomView != null)
            {
                zoomViewScale = zoomView.GetComponent<RectTransform>().rect.width / (Content.rect.width * Content.localScale.x);

                zoomViewViewport.offsetMin = zoomViewScale * new Vector2(ZoomMargin.x, ZoomMargin.z);
                zoomViewViewport.offsetMax = -zoomViewScale * new Vector2(ZoomMargin.y, ZoomMargin.w);
            }
        }

        private float ElasticClamp(float value, float minValue, float maxValue, float limit, float damping)
        {
            if (value > maxValue)
            {
                float y = limit * Mathf.Sin(damping * (value - maxValue)) + maxValue;

                if (value > (maxValue + ((2 * Mathf.PI / damping) / 4f)))
                {
                    return maxValue + limit;
                }
                else
                {
                    return y;
                }
            }
            else if (value < minValue)
            {
                float y = limit * Mathf.Sin(damping * (value - minValue)) + minValue;

                if (value < (minValue - ((2 * Mathf.PI / damping) / 4f)))
                {
                    return minValue - limit;
                }
                else
                {
                    return y;
                }
            }
            else
            {
                return value;
            }
        }

        public void SetZoom(float TargetZoom, float targetSmoothing = 0)
        {
            this.targetSmoothing = targetSmoothing;
            this.TargetZoom = TargetZoom;
        }
        public void SetPivot(Vector2 pivot)
        {
            Vector3 displacement = Content.pivot - pivot;
            displacement.Scale(Content.rect.size);
            displacement.Scale(Content.localScale);

            Content.pivot = pivot;
            Content.anchoredPosition -= (Vector2)displacement;
        }

        public void ZoomIn(Vector2 pivot, float increment, float smoothing)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, pivot * new Vector2(Screen.width, Screen.height), canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out mouseLocalPosition))
            {
                float x = Content.pivot.x + (mouseLocalPosition.x / Content.rect.width);
                float y = Content.pivot.y + (mouseLocalPosition.y / Content.rect.height);
                pivot = new Vector2(x, y);
            }
            SetPivot(pivot);
            SetZoom(Mathf.Clamp(currentZoom + increment, minMaxZoom.min, minMaxZoom.max), smoothing);
        }
        public void ZoomOut(Vector2 pivot, float increment, float smoothing)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, pivot * new Vector2(Screen.width, Screen.height), canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out mouseLocalPosition))
            {
                float x = Content.pivot.x + (mouseLocalPosition.x / Content.rect.width);
                float y = Content.pivot.y + (mouseLocalPosition.y / Content.rect.height);
                pivot = new Vector2(x, y);
            }
            SetPivot(pivot);
            SetZoom(Mathf.Clamp(currentZoom - increment, minMaxZoom.min, minMaxZoom.max), smoothing);
        }
        #endregion
    }
}