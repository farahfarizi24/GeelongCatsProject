﻿using com.DU.CE.INT;
using com.DU.CE.LVL;
using com.DU.CE.NET.NCM;
using EPOOutline;
using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

namespace com.DU.CE.AI
{
    [RequireComponent(typeof(RealtimeView))]
    [RequireComponent(typeof(RealtimeTransform))]
    public class AI_Avatar : RealtimeComponent<NCM_AvatarModel>, INT_ILinkedPinObject
    {
        #region Private Members

        [SerializeField] private SOC_FieldBoard m_boardSock = null;
        [Space]
        [SerializeField] private AI_CustomXRInteractable m_interactable = null;
        [SerializeField] private AI_PathManager m_pathManager = null;
        [SerializeField] private NavMeshAgent m_navMeshAgent = null;
        [SerializeField] private Rigidbody m_rigidBody = null;
        [SerializeField] private SkinnedMeshRenderer m_meshRenderer = null;
        [SerializeField] private GameObject m_uiElementsParent = null;
        [Space]
        [SerializeField] private TextMeshPro m_numberText = null;
        //[SerializeField] private Outlinable m_outline = null;
        public Outlinable OutlineScript;
       // public GameObject OutlineableObject=null;
       
        public bool NavMeshCount;//to see when the navmesh is finish
        private bool NavMeshIsRunning;
        private int m_teamNumber = 0;
        private ETEAM m_team = 0;
       // public List<Vector3> Position = new List<Vector3>();
        //public List<Vector3> Rotation = new List<Vector3>();
        private float m_rotationY = 0f;
        public string state = "";
        public bool BallReceiver = false;
        public bool prevBallReceiver = false;
        public bool IsPositionReference = false;
        public bool prevPositionReference = false;

        public bool HighlightOn = false;
        public bool prevHighlightOn = false;
        //This control whether scenario or running or not, when scenario is running all outline should be hidden
        //but if scenario on review all outline can be shown again
        public bool isScenarioRunning;
        public bool isReviewRunning;
        public bool isCreatingState;
        public GameObject AIModel;
        #endregion

        
        public int M_PlayerNumber { get => m_teamNumber; }
        public ETEAM M_Team { get => m_team; }

        public NCM_AvatarModel M_NCModel { get => model; }
        public List<Vector3> AvatarPosition = new List<Vector3>();
        public List<Vector3> AvatarRotation = new List<Vector3>();
        // These methods are used to sync the data to the client models
        // They are called when the value stores in the realtime model changes.
        #region NormCore Callbacks
       

        protected override void OnRealtimeModelReplaced(NCM_AvatarModel previousModel, NCM_AvatarModel currentModel)
        {
            if (previousModel != null)
            {
                // Unregister from NormCore events
                previousModel.numberDidChange -= InitializeNumber;
                previousModel.teamDidChange -= InitializeTeam;
                previousModel.isSelectedDidChange -= OnSelectChanged;
                previousModel.isActivatedDidChange -= OnActivated;
                previousModel.isBallReceiverDidChange -= OnBallReceiverChanged;
                previousModel.isPlayerReferenceDidChange -= OnPlayerReferenceChanged;
                previousModel.isHighlightActivatedDidChange -= OnHighlightActivatedChanged;
            }


            if (currentModel != null)
            {
                // If this is a model that has no data set on it, populate it with the current data.
                if (currentModel.isFreshModel)
                {
                    currentModel.team = (int)m_team;
                    currentModel.number = m_teamNumber;
                    currentModel.isSelected = m_interactable.isSelected;
                    currentModel.isActivated = false;
                    currentModel.isPlayerReference = IsPositionReference;
                    currentModel.isBallReceiver = BallReceiver;
                    currentModel.isHighlightActivated = HighlightOn;
                    // Instantiate a Board Pin
                    m_boardSock.AIInstantiateCall(transform);

                    Activate(currentModel.isActivated);
                }

                // Register for NormCore events
            
                currentModel.teamDidChange += InitializeTeam;
                currentModel.numberDidChange += InitializeNumber;
                currentModel.isSelectedDidChange += OnSelectChanged;
                currentModel.isActivatedDidChange += OnActivated;
                currentModel.isBallReceiverDidChange += OnBallReceiverChanged;
                currentModel.isPlayerReferenceDidChange += OnPlayerReferenceChanged;
                currentModel.isHighlightActivatedDidChange += OnHighlightActivatedChanged;

                m_team = (ETEAM)model.team;
                m_teamNumber = model.number;
                BallReceiver = model.isBallReceiver;
                IsPositionReference = model.isPlayerReference;
                HighlightOn = model.isHighlightActivated;
                m_numberText.SetText(model.number.ToString());
                Activate(currentModel.isActivated);
            }

            //base.OnRealtimeModelReplaced(previousModel, currentModel);
        }

        void Start()
        {
            //  m_outline = OutlineableObject.GetComponent<Outlinable>();
        
          //  m_outline.enabled = true;
          //  m_outline.OutlineColor = Color.yellow;
        }
        void Update()

        {
            if (M_NCModel.isActivated)
            {
                


                if (NavMeshIsRunning & m_navMeshAgent.remainingDistance < 0.1f)
                {
                    Debug.Log("NAVMESH REMAIN DISTANCE IS " + m_navMeshAgent.remainingDistance);


                    if (state == "Init")
                    {
                        state = "";
                      gameObject.transform.eulerAngles = AvatarRotation[0];
                        NavMeshCount = true;

                    }
                   else if (state == "Final")
                    {
                        state = "";
                        NavMeshCount = true;
                     gameObject.transform.eulerAngles = AvatarRotation[1];
                      

             }

                    NavMeshIsRunning = false;



                }

                if( BallReceiver != prevBallReceiver)
                {
                    setBallReceiver(BallReceiver);
                    prevBallReceiver = BallReceiver;
                  

                }

                if(HighlightOn != prevHighlightOn)
                {
                    setHighlightStatus(HighlightOn);
                    prevHighlightOn = HighlightOn;
           
                }

                if (BallReceiver && !OutlineScript.enabled && !isScenarioRunning)
                {
                    OutlineScript.OutlineColor = Color.yellow;

                    OutlineScript.enabled = true;
                    HighlightOn = true;

                    if (HighlightOn != prevHighlightOn)
                    {
                        setHighlightStatus(HighlightOn);
                        prevHighlightOn = HighlightOn;

                    }
                }



                if (IsPositionReference != prevPositionReference)
                {
                    OutlineScript.OutlineColor = Color.green;
                    SetPlayerReference(IsPositionReference);
                    prevPositionReference = IsPositionReference;
                }

                if (IsPositionReference && !OutlineScript.enabled && !isScenarioRunning)
                {
                    OutlineScript.enabled = true;
                    HighlightOn = true;

                    if (HighlightOn != prevHighlightOn)
                    {
                        setHighlightStatus(HighlightOn);
                        prevHighlightOn = HighlightOn;

                    }
                }

                if(BallReceiver || IsPositionReference)
                {

                    if (isScenarioRunning) { OutlineScript.enabled = false; HighlightOn = false;


                        if (HighlightOn != prevHighlightOn)
                        {
                            setHighlightStatus(HighlightOn);
                            prevHighlightOn = HighlightOn;

                        }

                    }
                   // if (isReviewRunning) OutlineScript.enabled = true;
                }

                if(BallReceiver && isReviewRunning)
                {
                    OutlineScript.enabled = true;
                    HighlightOn = true;

                    if (HighlightOn != prevHighlightOn)
                    {
                        setHighlightStatus(HighlightOn);
                        prevHighlightOn = HighlightOn;

                    }
                }

                if (BallReceiver)
                {
                    if (isCreatingState) 
                    { OutlineScript.enabled = false;
                        HighlightOn = false;


                        if (HighlightOn != prevHighlightOn)
                        {
                            setHighlightStatus(HighlightOn);
                            prevHighlightOn = HighlightOn;

                        }
                    }
                }

                

            }
        }

        
       

        public void ChangeNetworkActivation(bool _toggle)
        {
            model.isActivated = _toggle;
        }

        private void OnActivated(NCM_AvatarModel model, bool toggle)
        {
            Activate(toggle);
        }

        internal void Activate(bool toggle)
        {
            // Disable Parent
            m_interactable.enabled = toggle;
            m_navMeshAgent.enabled = toggle;
          //  m_rigidBody.isKinematic = !toggle;
            m_interactable.colliders[0].enabled = toggle;
            m_meshRenderer.enabled = toggle;
            m_uiElementsParent.SetActive(toggle);

            // Set pin status
            m_linkedPin?.SetObjectStatus(toggle);
        }

        private void InitializeTeam(NCM_AvatarModel model, int value)
        {
            m_team = (ETEAM)model.team;

            m_linkedPin.SetupPin(m_team, m_teamNumber);
        }

        // Initialize team number and display it
        private void InitializeNumber(NCM_AvatarModel model, int value)
        {
            m_teamNumber = model.number;
            // Set teamnumber on number overhead display
            m_numberText.SetText(model.number.ToString());

            m_linkedPin.SetupPin(m_team, m_teamNumber);
        }

        internal void OnHoverChanged(bool value)
        {
            if (value)
            {
              OutlineScript.enabled = true;
                HighlightOn = true;
             //   m_outline.OutlineColor = Color.yellow;
                m_numberText.color = Color.yellow;

                LVL_SoundManager.PlayMusic("AIHover");
            }
            else
            {
              OutlineScript.enabled = false;
                HighlightOn = false;
                m_numberText.color = Color.white;
            }
        }

        private void OnHighlightActivatedChanged(NCM_AvatarModel model, bool value)
        {
            Debug.Log("OnHighlightActivatedChanged");


            if (value)
            {
                OutlineScript.enabled = true;
                Debug.Log("Outline enabled");
            }
            else
            {
                OutlineScript.enabled = false;
                Debug.Log("Outline disabled");
            }
        }
        private void OnSelectChanged(NCM_AvatarModel model, bool value)
        {
          //  m_rigidBody.isKinematic = value;
            m_navMeshAgent.isStopped = true;

            if (value)
            {
               OutlineScript.enabled = true;
                HighlightOn = true;
                //  m_outline.OutlineColor = Color.green;
                m_numberText.color = Color.green;
            }
            else
            {
              OutlineScript.enabled = false; HighlightOn = false;
                m_numberText.color = Color.white;

                m_linkedPin.UpdatePinPosition();
            }
        }



        #endregion

    


        //////THis script below toggle the highlights ona nd off
        ///
        /// 
        /// 

        #region highlights

        void INT_ILinkedPinObject.SetHighlight()
        {
            OutlineScript.enabled = true;
            HighlightOn = true;
            OutlineScript.OutlineColor = Color.yellow;
        }
        void INT_ILinkedPinObject.ResetHighlight()
        {
            OutlineScript.enabled = false;
            HighlightOn = false;
        }

        public void UnsetHighlight()
        {
            OutlineScript.enabled = false;
            HighlightOn = false;
        }
        public void UnlinkedSetHighlight()
        {
            OutlineScript.OutlineColor = Color.yellow;
            OutlineScript.enabled = true;
            HighlightOn = true;
            Color32 col = new Color32(236, 245, 0, 255);
            m_linkedPin.SetPinColour(col, true);

            
        }

        public void setHighlightStatus(bool toggle)
        {
            if(toggle == false)
            {
                OutlineScript.enabled = false;
                HighlightOn = false;
            }
            else
            {
                OutlineScript.enabled = true;
                HighlightOn = true;
            }
            model.isHighlightActivated = HighlightOn;
        }


        public void setBallReceiver(bool toggle)
        {
            if (toggle == false)
            {
          
                OutlineScript.enabled = false;
                HighlightOn = false;
            }
            else
            {
                OutlineScript.OutlineColor = Color.yellow;
                OutlineScript.enabled = true;
                HighlightOn = true;
             //   Color32 col = new Color32(231, 49, 203, 255);
             //   m_linkedPin.SetPinColour(col);

            }
            model.isBallReceiver = BallReceiver;
        }

        public void OnBallReceiverChanged(NCM_AvatarModel model, bool toggle)
        {
            Debug.Log("ON BALL RECEIVER CHANGED");
            BallReceiver = toggle;
           // setBallReceiver(toggle);
        }


        #endregion


        #region Set Player Outline on and off

        public void SetBallReceiverOutline(bool status)
        {
            //will only be turned on during review
            OutlineScript.enabled = status;
            HighlightOn = status;
        }
        public void SetPlayerReferenceOutline(bool status)
        {
            
            //will only be turned on during review and at the beginning of the stage
            OutlineScript.enabled = status;
            HighlightOn = status;
        }

        
        #endregion





        #region Player Reference
        public void OnPlayerReferenceChanged(NCM_AvatarModel model, bool toggle)
        {
            Debug.Log("ON POSITION RECEIVER CHANGED");
            IsPositionReference = toggle;
        }



        public void SetPlayerReference(bool PlayerReference)
        {
            if (PlayerReference == false)
            {

                OutlineScript.OutlineColor = Color.cyan;
                OutlineScript.enabled = false;
                HighlightOn = false;
                //   m_linkedPin.UnsetPinRing();


            }
            else
            {

                OutlineScript.OutlineColor = Color.cyan;
                OutlineScript.enabled = true; HighlightOn = true;
                //   m_linkedPin.SetPinRing();



            }
            model.isPlayerReference = PlayerReference;

          
        }



        #endregion



        /// <summary>
        /// This method is accessed by the spawnner to initialize the AI's
        /// team and team number
        /// </summary>
        public void SetTeamInfo(ETEAM team, int number)
        {
            model.team = (int)team;
            model.number = number;
        }


        #region BoadPinObject Interface methods

        public INT_IBoardLinkedPin m_linkedPin;

        
        void INT_ILinkedPinObject.LinkPin(INT_IBoardLinkedPin pin)
        {
            m_linkedPin = pin;
            m_linkedPin.SetupPin(m_team, m_teamNumber);
            m_linkedPin.SetObjectStatus(model.isActivated);
        }

        Transform INT_ILinkedPinObject.GetRelativeTransform()
        {
            return transform;
        }

        void INT_ILinkedPinObject.SetNavAgentDestination(Vector3 _destinationInXY)
        {
            // Unpause nav mesh agent
            m_navMeshAgent.isStopped = false;
          
            // Set the destination for the path agent
            m_navMeshAgent.SetDestination(_destinationInXY);


           
        }

        public void ResetPlayerReference()
        {

            Debug.Log("Reset Player Reference");
            IsPositionReference = false;
            //turn on and off indicator

            Color32 col = new Color32(255, 255, 255, 255);
            OutlineScript.enabled = false;
            HighlightOn = false;
            m_linkedPin.UnsetPinRing();


        }

        public void NonlinkedPlayerReference()
        {
            if (IsPositionReference == false)
            {

                m_linkedPin.UnsetPinRing();
            }
            else
            {


                m_linkedPin.SetPinRing();

            }
        }

        void INT_ILinkedPinObject.SetPlayerReference(bool status)
        {
            IsPositionReference = status;
     
            if (IsPositionReference == false)
            {
               
                m_linkedPin.UnsetPinRing();
            }
            else
            {
             
               
                m_linkedPin.SetPinRing();

            }
        }

   

        public void ResetBallReceiver()
        {
            Debug.Log("RESET BALL RECEIVER");
            BallReceiver = false;
            Color32 col = new Color32(255, 255, 255, 255);

            m_linkedPin.SetPinColour(col,false);
        }
        void INT_ILinkedPinObject.SetBallReceiver(bool status)
        {
            BallReceiver = status;
            Color32 col;
            if (BallReceiver == false)
            {

                col = new Color32(255,255,255,255);
                
                m_linkedPin.SetPinColour(col,true);
            }
            else
            {
                col = new Color32(231, 49, 203, 255);
                m_linkedPin.SetPinColour(col,false);
            }
        }
        
        void INT_ILinkedPinObject.SetInitPosition()
        {
          
            m_linkedPin.UpdateForLoad(AvatarPosition[0],AvatarRotation[0]);
           
           
            m_navMeshAgent.SetDestination(AvatarPosition[0]);
            m_navMeshAgent.isStopped = false;
            NavMeshCount = false;

            
            state = "Init";
            StartCoroutine(WaitForNavmeshToStart());
        }


        IEnumerator WaitForNavmeshToStart()
        {
            yield return new WaitForSeconds(2);
            NavMeshIsRunning = true;
        }
        void INT_ILinkedPinObject.SetFinalPosition()
        {
        
            m_linkedPin.UpdateForLoad(AvatarPosition[1], AvatarRotation[1]);
            m_navMeshAgent.SetDestination(AvatarPosition[1]);
            m_navMeshAgent.isStopped = false;
            Debug.Log("Setting final position");
            
            NavMeshCount = false;
            state = "Final";
            StartCoroutine(WaitForNavmeshToStart());
          
           // StartCoroutine(WaitForNavmeshToStart());
        }

        void INT_ILinkedPinObject.SetScenarioTransform(Vector3 _initPosi, Vector3 _initRot, Vector3 _finalPosi, Vector3 _finalRot)
        {
            AvatarPosition.Clear();
            AvatarRotation.Clear();
            AvatarPosition.Add(_initPosi);
            AvatarPosition.Add(_finalPosi);
            AvatarRotation.Add(_initRot);
            AvatarRotation.Add(_finalRot);
        }


        void INT_ILinkedPinObject.SetRelativeYRotation(float rotY)
        {
            m_rotationY = rotY;
            gameObject.transform.eulerAngles = new Vector3(0.0f, m_rotationY, 0.0f);
        }

        float INT_ILinkedPinObject.GetRelativeYRotation()
        {
            m_rotationY = gameObject.transform.eulerAngles.y; 
            return m_rotationY;
        }


     
       

        #endregion
    }
}

