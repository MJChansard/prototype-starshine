using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ModuleAnimator : MonoBehaviour
{
    //Ok, right now this is specifically for AutoCannon
    public IEnumerator TriggerModuleAnimationCoroutine(GridBlock targetBlock)
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        for (int i = 0; i < targetBlock.objectsOnBlock.Count; i++)
        {
            if (targetBlock.objectsOnBlock[i].TryGetComponent<Hazard>(out Hazard hazard))
            {
                var trigger = ps.trigger;
                trigger.enabled = true;

                trigger.SetCollider(0, hazard.GetComponent<Collider>());
                trigger.radiusScale = 0.5f;

                ps.Play();
                yield return new WaitForSeconds(2.0f);
                ps.Stop();

                break;
            }
        }
    }
}
