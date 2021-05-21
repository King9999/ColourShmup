using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used to handle animations in place of Unity's animator, which is designed for 3D models.
public class AnimationController : MonoBehaviour
{
    string CurrentState { get; set; }            //used to hold the name of the current animation playing
   

    //IMPORTANT: This method only works within a coroutine!
    public void ChangeAnimationState(Animator anim, string animState)
    {
        if (CurrentState == animState)
            return;

        anim.Play(animState);

        CurrentState = animState;
    }
}
