using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModuleAnimator
{
    public IEnumerator StartModuleAnimationCoroutine(GridBlock targetBlock)
    {
        yield return null;
    }
}
