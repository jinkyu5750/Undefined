using UnityEngine;
using DG.Tweening;

public class Tree : ObjectBase
{

    public override void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {
        base.OnPropertyInjected_Dynamic(property);

        if(property == DynamicPropertyType.thigmotropism)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(2.5f, 4f));
            seq.Join(transform.DOScaleY(8.5f, 4f));
            seq.Play();
        }
    }
}
