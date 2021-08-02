using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("마우스속도")]
    //시리얼라이즈
    [SerializeField] private float sensitivity;
    [Header("이동속도")]
    [SerializeField] private float moveSpd;
    [Header("점프력")]
    [SerializeField] private float jumpPower;
    [Header("크로스헤어컨트롤러")]
    [SerializeField] private CrossHairController crossHairController;
    [Header("건컨트롤러")]
    [SerializeField] private GunController gunController;
    [Header("메인카메라")]
    public Camera cam;

    //변수
    //마우스관련 변수
    public float mx = 0, my = 0;
    //캐릭터 상태 bool 변수
    private bool isIdle = false;
    private bool isWalk = false;
    private bool isGround = false;
    public bool isSit = false;
    //실제 캐릭터에 대한 움직임제어에 대한 변수
    private float sitPrevYpos;
    private float sitYpos = 0.0f;
    private float applyYpos;
    private float applySpd;

    //컴포넌트
    public CapsuleCollider capsuleCollider;
    public Rigidbody rigid;

    private RaycastHit hitInfo;

    void Start()
    {

        //변수 초기화
        sitPrevYpos = cam.transform.localPosition.y;
        applyYpos = sitPrevYpos;
        applySpd = moveSpd;

        //플레이어 오브젝트에 붙어있는 캡슐콜라이더
        capsuleCollider = GetComponent<CapsuleCollider>();

        //리지드 바디
        rigid = GetComponent<Rigidbody>();

        //커서
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;   

    }

    void Update()
    {
        InteractLook();
        Idle();
        IsGround();
        TrySit();
        Move();
        TryJump();
        XcameraMove();
        YcameraMove();
    }

    //카메라 좌우 움직임
    private void XcameraMove()
    {

        float move = Input.GetAxis("Mouse X");

        mx += move * sensitivity;

        transform.eulerAngles = new Vector3(0f, mx, 0f);

    }

    //카메라 상하 움직임
    private void YcameraMove()
    {

        float move = Input.GetAxis("Mouse Y");

        my -= move * sensitivity;

        my = Mathf.Clamp(my, -90f, 90f);

        //컴포넌트로 불러온 cam, Player의 실린더가 상하로 움직이면 안되니까.
        //상속되어 있으므로 로컬좌표
        cam.transform.localEulerAngles = new Vector3(my, 0f, 0f);
    }

    //position은 월드(절대) 좌표를 현재의 스크립트를 적용한 오브젝트의 기준이 아닌 무조건 월드좌표기준
    //그래서 Player 오브젝트의 Z좌표가 변함에도 불구하고 바라보는 시점으로 이동을 안 한 것.
    //Translate는 상대좌표

    private void Idle()
    {
        isIdle = (!isWalk && !isSit && isGround);
        crossHairController.Ch_Idle(isIdle);
    }

    private void Move()
    {

        float ws = Input.GetAxisRaw("Vertical");
        float ad = Input.GetAxisRaw("Horizontal");

        //크로스헤어 크기제어나 기타 상황을 위해 만듦
        //절대값으로 만들어서 음수와 양수가 더해질때 0이 되는 상황방지
        float walkCheck = Mathf.Abs(ws) + Mathf.Abs(ad);

        //무빙중인지 true, false 체크
        if (walkCheck != 0 && isGround)
            isWalk = true;
        else
            isWalk = false;

        crossHairController.Ch_Walk(isWalk);

        transform.Translate(new Vector3(ad, 0f, ws).normalized * applySpd * Time.deltaTime);
    }

    //점프
    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }
  
    //캐릭터의 밑바닥 + 0.1f 부분에 레이저를 쐈을때 무언가 있는가 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        crossHairController.Ch_Jump(!isGround);
    }

    private void Jump()
    {
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    //앉기
    private void TrySit()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl) && isGround)
        {
            isSit = true;
            applySpd *= 0.5f;
            applyYpos = sitYpos;
            StopAllCoroutines();
            StartCoroutine(Sit());
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || !isGround)
        {
            isSit = false;
            applySpd = moveSpd;
            applyYpos = sitPrevYpos;
            StopAllCoroutines();
            StartCoroutine(Sit());
        }
        crossHairController.Ch_Sit(isSit);
    }

    //러프가 키를 눌렀을때 한번만 실행되었기때문에 코루틴하고 while문을 씀.
    IEnumerator Sit()
    {
        while(!(cam.transform.localPosition.y < applyYpos + 0.01f && cam.transform.localPosition.y > applyYpos - 0.01f))
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0f, applyYpos, 0f), 0.03f);
            yield return null;
        }
        cam.transform.localPosition = new Vector3(0f, applyYpos, 0f);
    }

    //둘러보면서 상호작용가능한 오브젝트를 찾는것.
    private void InteractLook()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 3f))
        {
            ItemGet();
        }
        Debug.DrawRay(cam.transform.position, cam.transform.forward * 2f, Color.red);
    }

    //아이템 줍줍
    private void ItemGet()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            //앞에 총이 있을 때
            if(hitInfo.transform.tag == "Weapon")
            {
                //이쪽은 gunController.cs쪽에서 왠지 코루틴함수를 불러와서 처리해야할것같다.
                //교체할때 버려질 총
                gunController.GunDump();
                //교체할때 적용될 총
                //gunController.crtGun = hitInfo.transform.GetComponent<Gun>();
               
                WeaponManager.Instance.WeaponChange(hitInfo.transform.GetComponent<Gun>());

                //Destroy(hitInfo.transform.gameObject); //주워진 총 필드에서 삭제
            }
        }
    }
}
