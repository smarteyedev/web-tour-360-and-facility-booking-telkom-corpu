using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class SelectionCard : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private Image _imageBackgroundCard;
        [SerializeField] private TextMeshProUGUI _textCardName;
        [SerializeField] private Button _buttonSelectCorpuArea;

        [Header("Corpu Selection | Additional Component References")]
        [SerializeField] private TextMeshProUGUI _textAddress;
        [SerializeField] private GameObject _iconLocker;
        [SerializeField] private GameObject _iconExplor;


        public void SetupCard(Sprite bgCard, string cardName, string address, bool isOpenForVisitor, Action onClickAction)
        {
            _imageBackgroundCard.sprite = bgCard;
            _textCardName.text = $"{cardName}";
            _textAddress.text = $"{address}";

            _iconLocker.SetActive(!isOpenForVisitor);
            _iconExplor.gameObject.SetActive(isOpenForVisitor);
            _buttonSelectCorpuArea.interactable = isOpenForVisitor;

            if (isOpenForVisitor)
            {
                _buttonSelectCorpuArea.onClick.RemoveAllListeners();
                _buttonSelectCorpuArea.onClick.AddListener(() => onClickAction?.Invoke());
            }
        }
    }
}