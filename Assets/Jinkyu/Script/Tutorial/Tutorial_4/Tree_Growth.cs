using DG.Tweening;
using Dynamite3D.RealIvy;
using System.Collections;
using UnityEngine;

public class Tree_Growth : ObjectBase
{
    private IvyController ivy;
    [SerializeField] private bool isGrown = false;
    public bool IsGrown => isGrown;
    public override void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {
        base.OnPropertyInjected_Dynamic(property);

        if (property == DynamicPropertyType.thigmotropism && !isGrown)
        {
            ivy = transform.GetComponentInChildren<IvyController>();
            StartCoroutine(Grow(ivy));

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(3.5f, 1f));
            seq.Join(transform.DOScaleY(11f, 1f)).OnComplete(() => isGrown = true);

            seq.Play();


        }
    }

    public IEnumerator Grow(IvyController ivy)
    {
        if (ivy == null) yield break;

        yield return new WaitForSeconds(1.3f);
        ivy.StartGrowth();
    }
}
