using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    [SerializeField]
    private GameObject[] Guns;

    [SerializeField]
    private GunController gunController;

    private Dictionary<string, Gun> gunList = new Dictionary<string, Gun>();

    private static WeaponManager instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < Guns.Length; i++)
        {
            gunList.Add(Guns[i].GetComponent<Gun>().gunName, Guns[i].GetComponent<Gun>());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) //주무기
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) //보조무기
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) //근접무기
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) //투척무기
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) //회복템
        {

        }
    }

    public static WeaponManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public void WeaponChange(Gun changeGun)
    {
        gunList[gunController.crtGun.gunName].gameObject.SetActive(false);
        gunController.crtGun = changeGun;
        gunList[gunController.crtGun.gunName].gameObject.SetActive(true);
    }

}
