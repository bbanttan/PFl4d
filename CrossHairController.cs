using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairController : MonoBehaviour
{
    [SerializeField]
    private Animator anim;

    public float addRecoil = 0.0f; //추가 반동

    public void Ch_Idle(bool _flag)
    {
        anim.SetBool("Idle", _flag);
    }

    public void Ch_Walk(bool _flag)
    {
        anim.SetBool("Walk", _flag);
    }

    public void Ch_Sit(bool _flag)
    {
        anim.SetBool("Sit", _flag);
    }

    public void Ch_Jump(bool _flag)
    {
        anim.SetBool("Jump", _flag);
    }

    public void Ch_Fire()
    {
        if (anim.GetBool("Idle"))
        {
            addRecoil = 0.0f;
            anim.SetTrigger("IdleFire");
        }
        else if (anim.GetBool("Walk"))
        {
            addRecoil = 0.5f;
            anim.SetTrigger("WalkFire");
        }
        else if (anim.GetBool("Sit"))
        {
            addRecoil = -1.0f;
            anim.SetTrigger("SitFire");
        }
        else if (anim.GetBool("Jump"))
        {
            addRecoil = 1.0f;
            anim.SetTrigger("JumpFire");
        }
    }

}
