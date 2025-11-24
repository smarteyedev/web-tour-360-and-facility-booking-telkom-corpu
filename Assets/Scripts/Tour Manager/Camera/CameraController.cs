using UnityEngine;
using UnityEngine.UI;

namespace Tour360TelkomCorpu.TourManager
{
    using Tour360TelkomCorpu.CanvasManager;
    public class CameraController : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private Transform horizontal;   // Rotasi Y
        [SerializeField] private Transform vertical;     // Rotasi X
        [SerializeField] private float sensitivity = 0.05f;

        [Header("Vertical Clamp")]
        [SerializeField] private bool limitVertical = true;
        [SerializeField] private float minVertical = -85f;
        [SerializeField] private float maxVertical = 85f;

        [Header("Inertia")]
        [SerializeField] private float minDamping = 0.01f;        // Slow drag
        [SerializeField] private float maxDamping = 0.1f;        // Fast drag
        [SerializeField] private float maxSpeed = 2f;          // Patokan kecepatan max

        private Vector2 inertiaVelocity;       // Kecepatan sisa
        private float damping;                 // Durasi inertia
        private bool applyingInertia = false;

        private Vector3 lastMousePos;
        private float verticalAngle = 0f;

        [Header("Auto Rotate")]
        [SerializeField] private bool autoRotate = false;
        [SerializeField] private bool autoRotateClockwise = true;
        [SerializeField] private float autoRotateSpeed = 5f;

        [Header("Zoom")]
        [SerializeField] private float minFOV = 20f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float zoomSmooth = 6f;
        private float targetFOV = 60f;

        [Header("Component References")]
        public Camera cam;
        [SerializeField] private Slider zoomSlider;
        [SerializeField] private CanvasManager _canvasManager;

        void Start()
        {
            if (cam == null) cam = Camera.main;

            if (zoomSlider != null)
            {
                zoomSlider.minValue = 0;
                zoomSlider.maxValue = 1;
                targetFOV = Mathf.Lerp(minFOV, maxFOV, zoomSlider.value);
                zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
            }

            _canvasManager.SetupButtonAutoRotation(AutoRotationToggle);
        }

        void Update()
        {
            if (_canvasManager.AnyPanelOpenNow()) return;

            HandleAutoRotate();
            HandleManualRotation();
            HandleInertia();
            HandleZoom();
        }

        // ============================================================
        //  AUTO ROTATION
        // ============================================================
        void HandleAutoRotate()
        {
            if (!autoRotate || horizontal == null) return;

            float dir = autoRotateClockwise ? -1f : 1f;
            horizontal.Rotate(0f, autoRotateSpeed * Time.deltaTime * dir, 0f, Space.Self);
        }

        // ============================================================
        //  MANUAL ROTATION
        // ============================================================

        private float m_holdTime = 0f;
        private float m_holdThreshold = .5f;
        private float m_moveThreshold = 0.1f;
        void HandleManualRotation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition;
                m_holdTime = 0f;
            }

            if (Input.GetMouseButton(0))
            {
                // hitung durasi hold
                m_holdTime += Time.deltaTime;

                // hitung pergerakan mouse
                Vector3 deltaHold = Input.mousePosition - lastMousePos;

                bool isMoving = deltaHold.sqrMagnitude > m_moveThreshold * m_moveThreshold;
                bool holdLongEnough = m_holdTime >= m_holdThreshold;

                // ---------------------------
                // CONDITION YANG ANDA MINTA
                // ---------------------------
                if (holdLongEnough && isMoving)
                {
                    autoRotate = false;
                    applyingInertia = false;
                    inertiaVelocity = Vector2.zero;
                    return; // <<==== KELUAR!
                }

                // rotasi normal
                Vector3 current = Input.mousePosition;
                Vector2 delta = (current - lastMousePos) * sensitivity;
                lastMousePos = current;

                ApplyRotation(delta);

                inertiaVelocity = delta;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (inertiaVelocity.sqrMagnitude > 0.01f)
                {
                    float speed = inertiaVelocity.magnitude;
                    float t = Mathf.Clamp01(speed / maxSpeed);
                    damping = Mathf.Lerp(minDamping, maxDamping, t);
                    applyingInertia = true;
                }
            }
        }

        // ============================================================
        //  INERTIA
        // ============================================================
        void HandleInertia()
        {
            if (!applyingInertia) return;

            // apply inertia movement
            ApplyRotation(inertiaVelocity);

            // decay inertia using exponential damping
            inertiaVelocity = Vector2.Lerp(inertiaVelocity, Vector2.zero, Time.deltaTime / damping);

            if (inertiaVelocity.sqrMagnitude < 0.01f)
            {
                applyingInertia = false;
            }
        }

        // ============================================================
        //  ROTATION CORE
        // ============================================================
        void ApplyRotation(Vector2 delta)
        {
            if (horizontal != null)
                horizontal.Rotate(0f, -delta.x, 0f, Space.Self);

            if (vertical != null)
            {
                verticalAngle += delta.y;
                if (limitVertical)
                    verticalAngle = Mathf.Clamp(verticalAngle, minVertical, maxVertical);

                vertical.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
            }
        }

        // ============================================================
        //  ZOOM
        // ============================================================
        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetFOV -= scroll * 20f;
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

                if (zoomSlider != null)
                    zoomSlider.SetValueWithoutNotify(Mathf.InverseLerp(minFOV, maxFOV, targetFOV));
            }

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmooth);
        }

        void OnZoomSliderChanged(float v)
        {
            targetFOV = Mathf.Lerp(minFOV, maxFOV, v);
            autoRotate = false;
        }

        public void AutoRotationToggle()
        {
            autoRotate = !autoRotate;
        }
    }
}