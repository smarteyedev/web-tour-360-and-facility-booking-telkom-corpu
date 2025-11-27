using UnityEngine;
using UnityEngine.UI;

namespace Tour360TelkomCorpu.TourManager
{
    using System;
    using DG.Tweening;
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
        [SerializeField] private bool autoRotate = true;

        [SerializeField] private float _holdTimeForDisableAutoRotate = .3f;
        [SerializeField] private bool autoRotateClockwise = true;
        [SerializeField] private float autoRotateSpeed = 5f;

        [Header("Zoom")]
        [SerializeField] private float minFOV;
        [SerializeField] private float maxFOV;
        [SerializeField] private float zoomSmooth = 6f;
        [SerializeField] private float targetFOV;

        [Header("Component References")]
        public Camera cam;

        [SerializeField] private CanvasManager _canvasManager;

        void Start()
        {
            if (cam == null) cam = Camera.main;
           
            StartAnimDrone(2f, 
                () => Debug.Log("Start Animation Drone"),
                () => Debug.Log("End Animation Drone"));


            //if (zoomSlider != null)
            //{
            //    zoomSlider.minValue = 0;
            //    zoomSlider.maxValue = 1;
            //    targetFOV = Mathf.Lerp(minFOV, maxFOV, zoomSlider.value);
            //    zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
            //}

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

        void HandleAutoRotate()
        {
            if (!autoRotate || horizontal == null) return;

            float dir = autoRotateClockwise ? -1f : 1f;
            horizontal.Rotate(0f, autoRotateSpeed * Time.deltaTime * dir, 0f, Space.Self);
        }

        void HandleManualRotation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition;
                _holdTimeForDisableAutoRotate = 0f;
            }

            if (Input.GetMouseButton(0))
            {
                // hitung durasi hold
                _holdTimeForDisableAutoRotate += Time.deltaTime;

                // hitung pergerakan mouse
                Vector3 deltaHold = Input.mousePosition - lastMousePos;

                bool isMoving = deltaHold.sqrMagnitude > 0.1f * 0.1f;
                bool holdLongEnough = _holdTimeForDisableAutoRotate >= .3f;

                if (holdLongEnough && isMoving)
                {
                    autoRotate = false;
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

        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetFOV -= scroll * 20f;
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

                //if (zoomSlider != null)
                //    zoomSlider.SetValueWithoutNotify(Mathf.InverseLerp(minFOV, maxFOV, targetFOV));
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

        public void StartAnimDrone(float cameraY, Action startAnimation, Action endAnimation )
        {
            startAnimation?.Invoke();
            // Convert 0–1 to 0–360

            float targetYaw = (cameraY >= 0f && cameraY <= 1f) //harus ada kondisi jika cameraY diluar 0-1 dan default nya dijadikan 0 
                ? cameraY * 360f 
                : 0f;
           
            float startAngle = 80f;
            float endAngle = 0f;
            float startHeight = 0.6f;
            float endHeight = -0.08f;
            float startAnimDuration = 3f;

            float t = Mathf.InverseLerp(startHeight, endHeight, 0.3f);
            t = Mathf.Clamp01(t);
            float thresholdTime = t * startAnimDuration;

            cam.fieldOfView = Mathf.Lerp(57f, targetFOV, 0f);
            cam.transform.localPosition = new Vector3(0f, startHeight, 0f);

            horizontal.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
            cam.transform.localRotation = Quaternion.Euler(startAngle, 0f, 0f);

            Sequence seq = DOTween.Sequence();

            seq.Join(
                horizontal.DOLocalRotate(
                    new Vector3(0f, targetYaw, 0f),
                    startAnimDuration
                )
                .From()                    // animate from current localRotation
                .SetEase(Ease.InOutSine)
            );

            seq.Join(
                cam.transform.DOLocalMoveY(endHeight, startAnimDuration)
                .SetEase(Ease.InOutQuad)
            );

            float rotDuration = Mathf.Max(0.0001f, startAnimDuration - thresholdTime);

            seq.Insert(
                thresholdTime,
                cam.transform.DOLocalRotate(
                    new Vector3(0f, 0f, 0f), rotDuration
                )
                .SetEase(Ease.InOutSine)
            );

            seq.OnComplete(() =>
            {
                verticalAngle = endAngle;
                seq.Kill();
                endAnimation?.Invoke();
                Debug.Log("Drone animation finished");
            });
        }

    }
}