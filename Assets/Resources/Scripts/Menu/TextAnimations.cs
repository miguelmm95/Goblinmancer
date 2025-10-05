using DG.Tweening;
using UnityEngine;

public class TextAnimations : MonoBehaviour
{
    private Vector3 _originalLocalScale;

    private void Awake()
    {
        _originalLocalScale = transform.localScale;
    }

    private void Start()
    {
        StartFloating();
    }

    public void ScaleUp()
    {
        transform.DOScale(_originalLocalScale * 1.25f, 0.2f).SetEase(Ease.OutBack);
    }

    public void ScaleDown()
    {
        transform.DOScale(_originalLocalScale, 0.2f).SetEase(Ease.OutBack);
    }

    public void StartFloating(float amplitude = 20f, float duration = 2f)
    {
        Vector3 startPos = transform.localPosition;
        transform.DOLocalMoveY(startPos.y + amplitude, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        transform.DOLocalMoveX(startPos.x + amplitude / 2f, duration * 1.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnDestroy()
    {
        DOTween.KillAll();
    }
}
