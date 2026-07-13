using UnityEngine;
using DG.Tweening;
using Dynamite3D.RealIvy;
using System.Collections;

public class Tree : ObjectBase
{
    private IvyController ivy;
    public override void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {
        base.OnPropertyInjected_Dynamic(property);

        if(property == DynamicPropertyType.thigmotropism)
        {
            ivy = transform.GetComponentInChildren<IvyController>();
            StartCoroutine(Growth(ivy));

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(2.5f, 1.5f));
            seq.Join(transform.DOScaleY(8.5f, 1.5f));

            seq.Play();

           
        }
    }

    public IEnumerator Growth(IvyController ivy)
    {
        if (ivy == null) yield break;

        yield return new WaitForSeconds(1f);
        ivy.StartGrowth();
    }
}
