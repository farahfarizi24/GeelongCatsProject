using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCatch : MonoBehaviour
{
    private Rigidbody rb = null;
    public Rigidbody mainBodyrb;
    public Animator anim;
    [SerializeField] public bool BallCatcher;
    public GameObject BallHoldPoint;
    public GameObject BallOwnership;
    public GameObject MainBody;
    public bool BallHolder; //to activate character animation script
    private IEnumerator Courutine;
    public float speed = 1.0f;
    // Start is called before the first frame update

    public GameObject BallDestination;
    public BallSensor ballSensor;
    private const string HoldBall = "HoldBall";
    void Start()
    {
        //BallCatcher = true;
        BallHolder = false; }

    // Update is called once per frame
    void Update()
    {

        if (BallHolder == true)
        {
            StartCoroutine(HoldBeforeDroppingTheBall());
        }
    }

    IEnumerator HoldBeforeDroppingTheBall()
    {
        yield return new WaitForSeconds(3);
        BallHolder = false;
        anim.SetBool(HoldBall, false);
        BallOwnership.transform.SetParent(null);
        mainBodyrb.isKinematic = true;
        rb.isKinematic = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("BallDestination")) { 
            BallCatcher = true; 
            Debug.Log("Ball Destination entered, catcher true");
            BallDestination = collider.gameObject;
            

            SetCatchingAnim();
        }

        if (collider.gameObject.CompareTag("BallContainer") && BallCatcher == true)
        {
            rb = collider.GetComponentInParent<Rigidbody>();

           
           Debug.Log(" This person is holding Ball");
      
            BallHolder = true;
            collider.transform.parent = BallOwnership.transform;
            mainBodyrb.isKinematic = true;
            rb.isKinematic = true;
          

        }

    }


    private void GrabBall()
    {

        BallOwnership.transform.parent = BallHoldPoint.transform;
        BallOwnership.transform.position = BallHoldPoint.transform.position;
        BallOwnership.transform.GetChild(0).position = BallOwnership.transform.position;

        ballSensor.SensorTrigger = false;

        anim.SetBool(HoldBall, true);
    }

    private void OnTriggerExit(Collider other)
    {
     //   if (other.gameObject.CompareTag("BallDestination")) { BallCatcher = false; Debug.Log("Ball Destination exited, catcher false"); }
       
    }
    public void SetCatchingAnim(){

        anim.SetLayerWeight(anim.GetLayerIndex("ActionLayer"), 1f);
        anim.SetLayerWeight(anim.GetLayerIndex("ArmLayer"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("BodyLayer"), 0f);

        if (BallDestination.transform.position.y > 1.5)
        {
            if (ballSensor.SensorTrigger == true)
            {

                BallOwnership.transform.GetChild(0).position = BallOwnership.transform.position;
            }
            anim.SetTrigger("HighCatch");

            Courutine = WaitForActionAnimToFinish("HighCatch");
        
            StartCoroutine(Courutine);
        }
        else
        {
            if (ballSensor.SensorTrigger == true)
            {

                BallOwnership.transform.GetChild(0).position = BallOwnership.transform.position;
            }

            anim.SetTrigger("LowCatch");
            Courutine = WaitForActionAnimToFinish("LowCatch");
            
            StartCoroutine(Courutine);
        }
    }
  
    IEnumerator WaitForActionAnimToFinish(string TriggerName)
    {
        // float animationLength = anim.GetCurrentAnimatorStateInfo(1).length + anim.GetCurrentAnimatorStateInfo(1).normalizedTime;
        //TODO on here, make ball move towards the player

        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(2.0f);
        //Ball ownership is triggered from characteranimationscript.cs


   


        anim.SetLayerWeight(anim.GetLayerIndex("BodyLayer"), 1f);
        anim.SetLayerWeight(anim.GetLayerIndex("ArmLayer"), 1f);

        BallCatcher = false; //stop from repeating

        anim.SetLayerWeight(anim.GetLayerIndex("ActionLayer"), 0f);
        anim.ResetTrigger(TriggerName);
        mainBodyrb = MainBody.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        mainBodyrb.isKinematic = true;
        BallHolder = true;
        SetBallOwnership();
    }
    public void SetBallOwnership()
    {
        Debug.Log("BALL is received");
        // turn on kinematics so player is not influenced by the ball

       
        BallOwnership.transform.parent = BallHoldPoint.transform;
        BallOwnership.transform.position = BallHoldPoint.transform.position;
        BallOwnership.transform.GetChild(0).position = BallOwnership.transform.position;

        ballSensor.SensorTrigger = false;
        
        anim.SetBool(HoldBall, true);
        rb.isKinematic = true;
        mainBodyrb.isKinematic = true;
        Debug.Log("Ball ownership completed");
     
    }
        

}
