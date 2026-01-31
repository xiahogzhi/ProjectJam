using System.Threading.Tasks;
using Azathrix.EzUI.Core;
using Azathrix.EzUI.Interfaces;
using Azathrix.GameKit.Runtime.Behaviours;
using Azathrix.GameKit.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingPanel : GameScript, ILoadingController
    {
        [SerializeField] private Image _image;

        public static LoadingPanel Instance { get; set; }

  

        public void SetProgress(float progress)
        {
        }

        public void SetText(string text)
        {
        }

        public void SetTitle(string title)
        {
        }

        public async UniTask<ILoadingController> ShowAsync()
        {
            gameObject.SetActive(true);
            _image.color= _image.color.SetAlpha(0);
            _image.DOFade(1f, 0.2f).SetUpdate(true);
            await UniTask.WaitForSeconds(0.2f, true);
            return this;
        }

        public async UniTask<ILoadingController> HideAsync()
        {
            _image.color= _image.color.SetAlpha(1f);
            _image.DOFade(0f, 0.2f).SetUpdate(true);
            await UniTask.WaitForSeconds(0.2f, true);
            gameObject.SetActive(false);
            return this;
        }
    }
}