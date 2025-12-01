using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class ButtonToggle : ButtonInteractive, IPointerDownHandler
    {
        [Header("Toggle State")]
        [SerializeField] private bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value) return;
                isActive = value;
                UpdateToggleVisual();
                onToggleChanged?.Invoke(isActive);
            }
        }

        [Header("Toggle Sprites - Background")]
        [SerializeField] private Sprite backgroundActive;
        [SerializeField] private Sprite backgroundInactive;

        [Header("Toggle Sprites - Icon")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite iconActive;
        [SerializeField] private Sprite iconInactive;

        [Header("Toggle Events")]
        public UnityEvent<bool> onToggleChanged;

        protected override void Start()
        {
            // base.Start();
            // Pastikan visual sesuai state awal
            UpdateToggleVisual();
        }

        /// <summary>
        /// Gantikan handler klik kiri bawaan jadi toggle ON/OFF,
        /// tapi tetap pakai base untuk trigger UnityEvent klik.
        /// </summary>
        public new void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) return;

            // Toggle hanya pada klik kiri
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                IsActive = !IsActive;
            }

            // Tetap jalankan logika klik dari base (event, state, dll)
            base.OnPointerDown(eventData);
        }

        /// <summary>
        /// Override supaya setiap perubahan event state (hover, click, dll)
        /// tetap update visual toggle.
        /// </summary>
        protected override void OnEventStateUpdate(EventState eventState)
        {
            base.OnEventStateUpdate(eventState);
            UpdateToggleVisual();
        }

        private void UpdateToggleVisual()
        {
            // Background dari toggle state (ON/OFF)
            if (_imageButton != null)
            {
                _imageButton.sprite = isActive ? backgroundActive : backgroundInactive;

                Color currentColor = _imageButton.color;
                currentColor.a = isActive ? 1f : .7f;
                _imageButton.color = currentColor;
            }

            // Icon dari toggle state (ON/OFF)
            if (iconImage != null)
            {
                iconImage.sprite = isActive ? iconActive : iconInactive;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Biar di inspector langsung kelihatan
            UpdateToggleVisual();
        }
#endif
    }
}