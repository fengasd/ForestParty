using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody mRigidBody;
    private CapsuleCollider mCapsCol;
    private bool mIsInited = false;
    [SerializeField]
    private TextMesh mUserName;
    [SerializeField]
    private TextMesh mDisplayMsg;
    [SerializeField] GameObject mSelfIndicator;
    protected float turnSmoothVelocity;
    public float Speed;
    string mPlayerID;
    public string PlayerID { get => mPlayerID; set => mPlayerID = value; }
    private string mPlayerName;
    public string PlayerName { get => mPlayerName; set => mPlayerName = value;}
    private bool mIsLocal = false;
    public bool IsLocal { get => mIsLocal; set => mIsLocal = value; }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if(mIsLocal)SetMovement(GetCurrentInputAxis());
    }
    void Init()
    {
        if (!mIsInited)
        {
            mRigidBody = GetComponent<Rigidbody>();
            mCapsCol = GetComponent<CapsuleCollider>();
            mIsInited = true;
        }
    }

    private Vector2 GetCurrentInputAxis()
    {
        Vector2 returnVar = new Vector2();
        returnVar.y = Input.GetAxisRaw("Horizontal");
        returnVar.x = Input.GetAxisRaw("Vertical");
        return returnVar;
    }
    public void SetMovement(Vector2 inputAxisValue)
    {
        Vector3 direction = new Vector3(inputAxisValue.y, 0f, inputAxisValue.x).normalized;
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, AgoraConst.TURN_SMOOTH_TIME);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.localPosition += moveDir * Speed * Time.fixedDeltaTime;
            DemoGameManager.Instance().SendActionMessage(MessageGenerator.MessageType.Action, inputAxisValue);
        }
    }

    public void SetName(string name)
    {
        mUserName.text = name;
        mSelfIndicator.SetActive(IsLocal);
    }

    public void SetDisplayMessage(string message)
    {
        mDisplayMsg.text = message;
    }
}