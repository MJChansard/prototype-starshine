using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorAutoCannon : MonoBehaviour, IModuleAnimator
{
    public IEnumerator StartModuleAnimationCoroutine(GridBlock targetBlock)
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        for (int i = 0; i < targetBlock.objectsOnBlock.Count; i++)
        {
            if (targetBlock.objectsOnBlock[i].TryGetComponent<GridObject>(out GridObject gridObject))
            {
                var trigger = ps.trigger;
                trigger.enabled = true;

                trigger.SetCollider(0, gridObject.GetComponent<Collider>());
                trigger.radiusScale = 0.5f;

                ps.Play();
                yield return new WaitForSeconds(2.0f);
                ps.Stop();

                break;
            }
        }
    }
}
