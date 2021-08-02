using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    //총 쏠때 변하는 크로스헤어
    [SerializeField] private CrossHairController crossHairController;

    //현재 총
    public Gun crtGun;

    [SerializeField] private GameObject bulletHolePrefab;

    public bool isFire = false;
    public bool isReload = false;

    private float reCoilAngle = 0.0f;
    private float reCoil = 0.0f;

    private RaycastHit hitInfo;

    private Coroutine fireCoroutine = null;
    private Coroutine reCoilCoroutine = null;

    private PlayerController pC;

    void Start()
    {
        pC = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        TryReload();
        TryFire();
        TryGunDump();
    }

    private void TryFire()
    {
        if (Input.GetButtonDown("Fire1") && !isReload)
        {
            if (fireCoroutine != null)
                return;
            fireCoroutine = StartCoroutine(Trigger());
        }
        if (Input.GetButtonUp("Fire1") && crtGun.fireMode == Gun.gunFireMode.automatic_fire)
        {
            isFire = false;
        }
    }

    IEnumerator Trigger()
    {
        isFire = true;

        if (crtGun.fireMode != Gun.gunFireMode.automatic_fire)
        {
            for (int i = 0; i < (int)crtGun.fireMode; i++)
            {
                Fire();
                yield return new WaitForSeconds(crtGun.fireRate);
            }
        }
        else if (crtGun.fireMode == Gun.gunFireMode.automatic_fire)
        {
            while (isFire && !isReload)
            {
                Fire();
                yield return new WaitForSeconds(crtGun.fireRate);
            }
        }

        isFire = false;
        fireCoroutine = null;

    }

    private void Fire()
    {

        crossHairController.Ch_Fire();
        crtGun.gunAnim.CrossFadeInFixedTime(crtGun.gunName + "_Fire", crtGun.fireRate);

        Vector3 recoil = new Vector3(Random.Range(0f, reCoil) + Random.Range(0f, -reCoil), Random.Range(0f, reCoil) + Random.Range(0f, -reCoil), 0f);

        //총레이어와 이그노어 레이어를 제외한 레이어의 물체들만 충돌
        int layerMask = (-1) - (((1 << LayerMask.NameToLayer("Weapon")) | (1 << LayerMask.NameToLayer("Ignore Raycast"))));

        if (Physics.Raycast(transform.position, transform.forward + recoil, out hitInfo, Mathf.Infinity, layerMask))
        {
            GameObject hole = Instantiate(bulletHolePrefab, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            Destroy(hole, 3f);
            Debug.DrawRay(transform.position, (transform.forward + recoil) * 100f, Color.red, 1f);
        }

        //어차피 반동시간(fireRate)안에 끝나는 코루틴이라 딱히 스탑코루틴 안함.
        StartCoroutine(camReCoil());

        //반동 코루틴
        if (reCoilCoroutine != null)
            StopCoroutine(reCoilCoroutine);
        reCoilCoroutine = StartCoroutine(ReCoil());
    }

    //에임 반동
    IEnumerator ReCoil()
    {
        
        while (isFire)
        {
            _ = reCoilAngle + 0.5f >= (crtGun.recoil + crossHairController.addRecoil) ? reCoilAngle = crtGun.recoil + crossHairController.addRecoil : reCoilAngle += 0.5f;
            reCoil = Mathf.Sin(reCoilAngle * Mathf.Deg2Rad);

            yield return null;
        }
        while(reCoilAngle > 0.0f)
        {
            if(reCoilAngle - 0.2f <= 0.0f)
                break;
            else
                reCoilAngle -= 0.2f;

            reCoil = Mathf.Sin(reCoilAngle * Mathf.Deg2Rad);

            yield return null;
        }
        reCoilAngle = reCoil = 0.0f;
        reCoilCoroutine = null;
    }

    //카메라 반동
    IEnumerator camReCoil()
    {
        float v = 120.0f;

        while(true)
        {
            v -= (120.0f * Time.deltaTime * (1 / crtGun.fireRate));
            if (v <= 0.0f) break;

            pC.cam.transform.eulerAngles -= new Vector3(Mathf.Sin(v * Mathf.Deg2Rad), 0.0f, 0.0f);            
            yield return null;
        }
    }

    private void TryReload()
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReload)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        //현재 장전된 총알이 모두 남은 총알과 합쳐짐
        isReload = true;
        crtGun.gunAnim.SetTrigger("Reload");

        yield return new WaitForSeconds(crtGun.reloadAllFrame/60f);

        isReload = false;

    }

    private void TryGunDump()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GunDump();
        }
    }

    public void GunDump()
    {
            //버릴 총 생성
        GameObject gun = Instantiate
           (
              crtGun.gunPrefab, pC.capsuleCollider.transform.position, 
              Quaternion.LookRotation(-pC.transform.forward, Vector3.up)
           );

        gun.GetComponent<Rigidbody>().AddForce(pC.capsuleCollider.transform.forward * 100);
        gun.GetComponent<Rigidbody>().AddTorque(Vector3.forward * 100);
    }
}