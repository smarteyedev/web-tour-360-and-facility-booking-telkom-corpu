using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class ButtonInteractive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Configuration References")]
    protected EventState m_eventState = EventState.Default;
    public bool isInteractable = false;
    [SerializeField] protected List<ButtonAsset> _assetList = new List<ButtonAsset>();

    public enum EventState
    {
        Default, Hover, OnClickRightMouse, OnClickLeftMouse, OnClickMiddleMouse
    }

    [System.Serializable]
    public class ButtonAsset
    {
        public EventState eventState;
        public Sprite buttonSprite;
        public Color textColor = new Color(1f, 0f, 1f, 1f);

        public ButtonAsset(EventState _eventState, Sprite _buttonSprite, Color _textColor)
        {
            this.eventState = _eventState;
            this.buttonSprite = _buttonSprite;
            this.textColor = _textColor;
        }
    }

    [Header("Component References")]
    [SerializeField] protected Image _imageButton;
    [SerializeField] protected TextMeshProUGUI _textButton;

    [Header("Unity Event")]
    [Space(3f)]
    public UnityEvent onLeftMouseDown;
    [Space(3f)]
    public UnityEvent onRightMouseDown;
    [Space(3f)]
    public UnityEvent onMiddleMouseDown;
    [Space(3f)]
    public UnityEvent onHoverEnter;
    [Space(3f)]
    public UnityEvent onHoverExit;

    protected virtual void Start()
    {
        SetupDefaultAsset(_imageButton.sprite, _textButton.color);
        // Set initial button state to default
        m_eventState = EventState.Default;
        OnEventStateUpdate(m_eventState);
    }

    protected virtual void Enable()
    {
        // SetupDefaultAsset(_imageButton.sprite, _textButton.color);
        // Set initial button state to default
        m_eventState = EventState.Default;
        OnEventStateUpdate(m_eventState);
    }

    protected virtual void OnDisable()
    {
        if (m_eventState == EventState.OnClickLeftMouse)
        {
            m_eventState = EventState.Default;
            OnEventStateUpdate(m_eventState);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) { return; }

        // Detect mouse button clicks and update state accordingly
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_eventState = EventState.OnClickLeftMouse;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            m_eventState = EventState.OnClickRightMouse;
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            m_eventState = EventState.OnClickMiddleMouse;
        }

        OnEventStateUpdate(m_eventState);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) { return; }

        // Change state to Hover when the cursor enters the button
        m_eventState = EventState.Hover;
        OnEventStateUpdate(m_eventState);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) { return; }

        m_eventState = EventState.Default;
        OnEventStateUpdate(m_eventState);
    }

    protected virtual void OnEventStateUpdate(EventState eventState)
    {
        // Update the button's visual representation based on the event state
        ButtonAsset buttonAsset = _assetList.Find(asset => asset.eventState == eventState);

        if (buttonAsset != null)
        {
            _imageButton.sprite = buttonAsset.buttonSprite;
            _textButton.color = buttonAsset.textColor;
        }

        switch (eventState)
        {
            case EventState.Default:
                //Debug.Log($"Button State is: {eventState} | default or hover is exit");
                onHoverExit?.Invoke();
                break;
            case EventState.Hover:
                //Debug.Log($"Button State is: {eventState}");
                onHoverEnter?.Invoke();
                break;
            case EventState.OnClickRightMouse:
                //Debug.Log($"Button State is: {eventState}");
                onRightMouseDown?.Invoke();
                break;
            case EventState.OnClickLeftMouse:
                //Debug.Log($"Button State is: {eventState}");
                onLeftMouseDown?.Invoke();
                break;
            case EventState.OnClickMiddleMouse:
                //Debug.Log($"Button State is: {eventState}");
                onMiddleMouseDown?.Invoke();
                break;
        }
    }

    protected virtual void SetupDefaultAsset(Sprite buttonSprite, Color textColor)
    {
        ButtonAsset defaultAsset = new ButtonAsset(
            _eventState: EventState.Default,
            _buttonSprite: buttonSprite,
            _textColor: textColor
        );

        if (!_assetList.Any(x => x.eventState == EventState.Default))
        {
            _assetList.Add(defaultAsset);
        }
        else
        {
            var btn = _assetList.FirstOrDefault(x => x.eventState == EventState.Default);
            btn.buttonSprite = buttonSprite;
            btn.textColor = textColor;
        }
    }
}
