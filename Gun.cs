using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public enum gunFireMode
    {
        single_fire = 1,    //단발
        point_fire = 3,     //점사
        automatic_fire  //연사
    };

    public string gunName; //총이름
    public int crtBullet; //현재 총알
    public int maxBullet; //최대로 들 수 있는 총알
    public float fireRate; //발사 속도
    public float recoil; //반동강도
    public float reloadAllFrame; //장전 총 프레임 수

    public gunFireMode fireMode; //발사 모드
    public AudioClip sound_Fire; //총소리
    public Animator gunAnim; //총 애니메이션
    public GameObject gunPrefab; //총 프리팹

}
