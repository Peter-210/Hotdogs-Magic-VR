//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Valve.VR
{
    public class SteamVR_Behaviour_Skeleton : MonoBehaviour
    {
        [Tooltip("If not set, will try to auto assign this based on 'Skeleton' + inputSource")]
        /// <summary>The action this component will use to update the model. Must be a Skeleton type action.</summary>
        public SteamVR_Action_Skeleton skeletonAction;

        /// <summary>The device this action should apply to. Any if the action is not device specific.</summary>
        [Tooltip("The device this action should apply to. Any if the action is not device specific.")]
        public SteamVR_Input_Sources inputSource;

        /// <summary>The range of motion you'd like the hand to move in. With controller is the best estimate of the fingers wrapped around a controller. Without is from a flat hand to a fist.</summary>
        [Tooltip("The range of motion you'd like the hand to move in. With controller is the best estimate of the fingers wrapped around a controller. Without is from a flat hand to a fist.")]
        public EVRSkeletalMotionRange rangeOfMotion = EVRSkeletalMotionRange.WithoutController;

        /// <summary>The root Transform of the skeleton. Needs to have a child (wrist) then wrist should have children in the order thumb, index, middle, ring, pinky</summary>
        [Tooltip("This needs to be in the order of: root -> wrist -> thumb, index, middle, ring, pinky")]
        public Transform skeletonRoot;

        /// <summary>The transform this transform should be relative to</summary>
        [Tooltip("If not set, relative to parent")]
        public Transform origin;

        /// <summary>Whether or not to update this transform's position and rotation inline with the skeleton transforms or if this is handled in another script</summary>
        [Tooltip("Set to true if you want this script to update its position and rotation. False if this will be handled elsewhere")]
        public bool updatePose = true;

        /// <summary>Check this to not set the positions of the bones. This is helpful for differently scaled skeletons.</summary>
        [Tooltip("Check this to not set the positions of the bones. This is helpful for differently scaled skeletons.")]
        public bool onlySetRotations = false;

        /// <summary>
        /// How much of a blend to apply to the transform positions and rotations.
        /// Set to 0 for the transform orientation to be set by an animation.
        /// Set to 1 for the transform orientation to be set by the skeleton action.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Modify this to blend between animations setup on the hand")]
        public float skeletonBlend = 1f;

        /// <summary>This Unity event will fire whenever the position or rotation of the bones are updated.</summary>
        public SteamVR_Behaviour_SkeletonEvent onBoneTransformsUpdated;

        /// <summary>This Unity event will fire whenever the position or rotation of this transform is updated.</summary>
        public SteamVR_Behaviour_SkeletonEvent onTransformUpdated;

        /// <summary>This Unity event will fire whenever the position or rotation of this transform is changed.</summary>
        public SteamVR_Behaviour_SkeletonEvent onTransformChanged;

        /// <summary>This Unity event will fire whenever the device is connected or disconnected</summary>
        public SteamVR_Behaviour_Skeleton_ConnectedChangedEvent onConnectedChanged;

        /// <summary>This Unity event will fire whenever the device's tracking state changes</summary>
        public SteamVR_Behaviour_Skeleton_TrackingChangedEvent onTrackingChanged;


        /// <summary>This C# event will fire whenever the position or rotation of this transform is updated.</summary>
        public UpdateHandler onBoneTransformsUpdatedEvent;

        /// <summary>This C# event will fire whenever the position or rotation of this transform is updated.</summary>
        public UpdateHandler onTransformUpdatedEvent;

        /// <summary>This C# event will fire whenever the position or rotation of this transform is changed.</summary>
        public ChangeHandler onTransformChangedEvent;

        /// <summary>This C# event will fire whenever the device is connected or disconnected</summary>
        public DeviceConnectedChangeHandler onConnectedChangedEvent;

        /// <summary>This C# event will fire whenever the device's tracking state changes</summary>
        public TrackingChangeHandler onTrackingChangedEvent;

        /// <summary>Can be set to mirror the bone data across the x axis</summary>
        [Tooltip("Is this rendermodel a mirror of another one?")]
        public MirrorType mirroring;



        [Header("No Skeleton - Fallback")]


        [Tooltip("The fallback SkeletonPoser to drive hand animation when no skeleton data is available")]
        /// <summary>The fallback SkeletonPoser to drive hand animation when no skeleton data is available</summary>
        public SteamVR_Skeleton_Poser fallbackPoser;

        [Tooltip("The fallback action to drive finger curl values when no skeleton data is available")]
        /// <summary>The fallback SkeletonPoser to drive hand animation when no skeleton data is available</summary>
        public SteamVR_Action_Single fallbackCurlAction;

        /// <summary>
        /// Is the skeleton action bound?
        /// </summary>
        public bool skeletonAvailable { get { return skeletonAction.activeBinding; } }







        /// <summary>The current skeletonPoser we're getting pose data from</summary>
        protected SteamVR_Skeleton_Poser blendPoser;
        /// <summary>The current pose snapshot</summary>
        protected SteamVR_Skeleton_PoseSnapshot blendSnapshot = null;


        /// <summary>Returns whether this action is bound and the action set is active</summary>
        public bool isActive { get { return skeletonAction.GetActive(); } }


        /// <summary>An array of five 0-1 values representing how curled a finger is. 0 being straight, 1 being fully curled. 0 being thumb, 4 being pinky</summary>
        public float[] fingerCurls
        {
            get
            {
                if (skeletonAvailable)
                {
                    return skeletonAction.GetFingerCurls();
                }
                else
                {
                    //fallback, return array where each finger curl is just the fallback curl action value
                    float[] curls = new float[5];
                    for (int i = 0; i < 5; i++)
                    {
                        curls[i] = fallbackCurlAction.GetAxis(inputSource);
                    }
                    return curls;
                }
            }
        }

        /// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
        public float thumbCurl
        {
            get
            {
                if (skeletonAvailable)
                    return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.thumb);
                else
                    return fallbackCurlAction.GetAxis(inputSource);
            }
        }

        /// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
        public float indexCurl
        {
            get
            {
                if (skeletonAvailable)
                    return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.index);
                else
                    return fallbackCurlAction.GetAxis(inputSource);
            }
        }

        /// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
        public float middleCurl
        {
            get
            {
                if (skeletonAvailable)
                    return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.middle);
                else
                    return fallbackCurlAction.GetAxis(inputSource);
            }
        }

        /// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
        public float ringCurl
        {
            get
            {
                if (skeletonAvailable)
                    return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.ring);
                else
                    return fallbackCurlAction.GetAxis(inputSource);
            }
        }

        /// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
        public float pinkyCurl
        {
            get
            {
                if (skeletonAvailable)
                    return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.pinky);
                else
                    return fallbackCurlAction.GetAxis(inputSource);
            }
        }



        public Transform root { get { return bones[SteamVR_Skeleton_JointIndexes.root]; } }
        public Transform wrist { get { return bones[SteamVR_Skeleton_JointIndexes.wrist]; } }
        public Transform indexMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.indexMetacarpal]; } }
        public Transform indexProximal { get { return bones[SteamVR_Skeleton_JointIndexes.indexProximal]; } }
        public Transform indexMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.indexMiddle]; } }
        public Transform indexDistal { get { return bones[SteamVR_Skeleton_JointIndexes.indexDistal]; } }
        public Transform indexTip { get { return bones[SteamVR_Skeleton_JointIndexes.indexTip]; } }
        public Transform middleMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.middleMetacarpal]; } }
        public Transform middleProximal { get { return bones[SteamVR_Skeleton_JointIndexes.middleProximal]; } }
        public Transform middleMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.middleMiddle]; } }
        public Transform middleDistal { get { return bones[SteamVR_Skeleton_JointIndexes.middleDistal]; } }
        public Transform middleTip { get { return bones[SteamVR_Skeleton_JointIndexes.middleTip]; } }
        public Transform pinkyMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyMetacarpal]; } }
        public Transform pinkyProximal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyProximal]; } }
        public Transform pinkyMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyMiddle]; } }
        public Transform pinkyDistal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyDistal]; } }
        public Transform pinkyTip { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyTip]; } }
        public Transform ringMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.ringMetacarpal]; } }
        public Transform ringProximal { get { return bones[SteamVR_Skeleton_JointIndexes.ringProximal]; } }
        public Transform ringMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.ringMiddle]; } }
        public Transform ringDistal { get { return bones[SteamVR_Skeleton_JointIndexes.ringDistal]; } }
        public Transform ringTip { get { return bones[SteamVR_Skeleton_JointIndexes.ringTip]; } }
        public Transform thumbMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbMetacarpal]; } } //doesn't exist - mapped to proximal
        public Transform thumbProximal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbProximal]; } }
        public Transform thumbMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.thumbMiddle]; } }
        public Transform thumbDistal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbDistal]; } }
        public Transform thumbTip { get { return bones[SteamVR_Skeleton_JointIndexes.thumbTip]; } }
        public Transform thumbAux { get { return bones[SteamVR_Skeleton_JointIndexes.thumbAux]; } }
        public Transform indexAux { get { return bones[SteamVR_Skeleton_JointIndexes.indexAux]; } }
        public Transform middleAux { get { return bones[SteamVR_Skeleton_JointIndexes.middleAux]; } }
        public Transform ringAux { get { return bones[SteamVR_Skeleton_JointIndexes.ringAux]; } }
        public Transform pinkyAux { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyAux]; } }

        /// <summary>An array of all the finger proximal joint transforms</summary>
        public Transform[] proximals { get; protected set; }

        /// <summary>An array of all the finger middle joint transforms</summary>
        public Transform[] middles { get; protected set; }

        /// <summary>An array of all the finger distal joint transforms</summary>
        public Transform[] distals { get; protected set; }

        /// <summary>An array of all the finger tip transforms</summary>
        public Transform[] tips { get; protected set; }

        /// <summary>An array of all the finger aux transforms</summary>
        public Transform[] auxs { get; protected set; }

        protected Coroutine blendRoutine;
        protected Coroutine rangeOfMotionBlendRoutine;
        protected Coroutine attachRoutine;

        protected Transform[] bones;

        /// <summary>The range of motion that is set temporarily (call ResetTemporaryRangeOfMotion to reset to rangeOfMotion)</summary>
        protected EVRSkeletalMotionRange? temporaryRangeOfMotion = null;

        /// <summary>
        /// Get the accuracy level of the skeletal tracking data.
        /// <para/>* Estimated: Body part location can’t be directly determined by the device. Any skeletal pose provided by the device is estimated based on the active buttons, triggers, joysticks, or other input sensors. Examples include the Vive Controller and gamepads.
        /// <para/>* Partial: Body part location can be measured directly but with fewer degrees of freedom than the actual body part.Certain body part positions may be unmeasured by the device and estimated from other input data.Examples include Knuckles or gloves that only measure finger curl
        /// <para/>* Full: Body part location can be measured directly throughout the entire range of motion of the body part.Examples include hi-end mocap systems, or gloves that measure the rotation of each finger segment.
        /// </summary>
        public EVRSkeletalTrackingLevel skeletalTrackingLevel
        {
            get
            {
                if (skeletonAvailable)
                {
                    return skeletonAction.skeletalTrackingLevel;
                }
                else
                {
                    return EVRSkeletalTrackingLevel.VRSkeletalTracking_Estimated;
                }
            }
        }

        /// <summary>Returns true if we are in the process of blending the skeletonBlend field (between animation and bone data)</summary>
        public bool isBlending
        {
            get
            {
                return blendRoutine != null;
            }
        }

        /*
        public float predictedSecondsFromNow
        {
            get
            {
                return skeletonAction.predictedSecondsFromNow;
            }

            set
            {
                skeletonAction.predictedSecondsFromNow = value;
            }
        }
        */
        public SteamVR_ActionSet actionSet
        {
            get
            {
                return skeletonAction.actionSet;
            }
        }

        public SteamVR_ActionDirections direction
        {
            get
            {
                return skeletonAction.direction;
            }
        }
        
        

        protected virtual void Awake()
        {
            SteamVR.Initialize();

            AssignBonesArray();

            proximals = new Transform[] { thumbProximal, indexProximal, middleProximal, ringProximal, pinkyProximal };
            middles = new Transform[] { thumbMiddle, indexMiddle, middleMiddle, ringMiddle, pinkyMiddle };
            distals = new Transform[] { thumbDistal, indexDistal, middleDistal, ringDistal, pinkyDistal };
            tips = new Transform[] { thumbTip, indexTip, middleTip, ringTip, pinkyTip };
            auxs = new Transform[] { thumbAux, indexAux, middleAux, ringAux, pinkyAux };

            CheckSkeletonAction();
        }

        protected virtual void CheckSkeletonAction()
        {
            if (skeletonAction == null)
                skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>("Skeleton" + inputSource.ToString());
        }

        protected virtual void AssignBonesArray()
        {
            bones = skeletonRoot.GetComponentsInChildren<Transform>();
        }
        
        
                        //========================================================
        
                
          
          public static Quaternion[] RIGHT_HAND_CLENCH_ROTATIONS = new Quaternion[]
        {
            new Quaternion(0.00000f, 1.00000f, 0.00000f, 0.00000f),
            new Quaternion(-0.07861f, -0.92028f, 0.37930f, -0.05515f),
            new Quaternion(-0.27378f, -0.84719f, 0.09383f, 0.44555f),
            new Quaternion(-0.01773f, 0.07545f, -0.20281f, 0.97615f),
            new Quaternion(0.10271f, 0.00482f, -0.34802f, 0.93183f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.62692f, -0.43992f, -0.49968f, 0.40469f),
            new Quaternion(-0.01906f, 0.09276f, -0.48380f, 0.87004f),
            new Quaternion(0.02076f, -0.00368f, -0.54725f, 0.83671f),
            new Quaternion(0.00025f, 0.00930f, -0.45312f, 0.89140f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.52355f, -0.47735f, -0.46839f, 0.52786f),
            new Quaternion(-0.12381f, -0.01298f, -0.53242f, 0.83728f),
            new Quaternion(0.00759f, -0.00481f, -0.49292f, 0.87003f),
            new Quaternion(-0.00003f, 0.01177f, -0.43079f, 0.90237f),
            new Quaternion(0.00000f, 0.00000f, -0.04013f, 0.99919f),
            new Quaternion(-0.50191f, -0.44895f, -0.51036f, 0.53485f),
            new Quaternion(-0.06140f, 0.03816f, -0.42193f, 0.90374f),
            new Quaternion(-0.00006f, -0.00121f, -0.47700f, 0.87890f),
            new Quaternion(-0.00196f, 0.00852f, -0.46896f, 0.88317f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.50295f, -0.35525f, -0.60961f, 0.49921f),
            new Quaternion(-0.01194f, 0.06315f, -0.33928f, 0.93849f),
            new Quaternion(-0.00283f, 0.00521f, -0.44967f, 0.89318f),
            new Quaternion(0.00321f, 0.02625f, -0.33409f, 0.94217f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.38986f, 0.34595f, -0.47885f, 0.70642f),
            new Quaternion(0.25489f, -0.89103f, -0.35733f, -0.11581f),
            new Quaternion(0.44765f, -0.82353f, -0.32304f, -0.13054f),
            new Quaternion(0.47398f, -0.83482f, -0.24397f, -0.13747f),
            new Quaternion(0.41913f, -0.88093f, -0.21973f, 0.00343f)
        };

        public static Vector3[] RIGHT_HAND_CLENCH_POSITIONS = new Vector3[]
        {
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(-0.03f, 0.04f, 0.16f),
            new Vector3(-0.02f, 0.03f, 0.02f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.02f, 0.01f),
            new Vector3(0.07f, 0.01f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.02f, 0.02f),
            new Vector3(0.06f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.02f, 0.08f),
            new Vector3(-0.01f, 0.00f, 0.07f),
            new Vector3(-0.02f, -0.04f, 0.08f),
            new Vector3(-0.02f, -0.05f, 0.11f),
            new Vector3(-0.01f, -0.06f, 0.13f)
        };




        public static Quaternion[] RIGHT_HAND_IDLE_ROTATIONS = new Quaternion[]
        {
            new Quaternion(0.00000f, 1.00000f, 0.00000f, 0.00000f),
            new Quaternion(-0.07861f, -0.92028f, 0.37930f, -0.05515f),
            new Quaternion(-0.44103f, -0.64819f, 0.28290f, 0.55255f),
            new Quaternion(-0.03988f, 0.00944f, -0.09855f, 0.99429f),
            new Quaternion(-0.00192f, -0.01310f, 0.15125f, 0.98841f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.64375f, -0.42249f, -0.47870f, 0.42182f),
            new Quaternion(0.02399f, 0.04052f, -0.09275f, 0.99458f),
            new Quaternion(0.02884f, 0.00009f, 0.00505f, 0.99957f),
            new Quaternion(0.00130f, 0.01483f, -0.02255f, 0.99964f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.54749f, -0.45993f, -0.44162f, 0.54193f),
            new Quaternion(-0.15577f, 0.04514f, -0.09847f, 0.98184f),
            new Quaternion(0.01875f, -0.00542f, -0.07544f, 0.99696f),
            new Quaternion(-0.00183f, 0.01523f, -0.04262f, 0.99897f),
            new Quaternion(0.00000f, 0.00000f, -0.04013f, 0.99919f),
            new Quaternion(-0.51929f, -0.42663f, -0.49705f, 0.54887f),
            new Quaternion(-0.08413f, 0.05897f, -0.13683f, 0.98525f), 
            new Quaternion(-0.00200f, 0.00009f, -0.06431f, 0.99793f),
            new Quaternion(-0.00124f, 0.00826f, -0.09977f, 0.99498f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.51322f, -0.34430f, -0.60692f, 0.49971f),
            new Quaternion(-0.06862f, 0.06558f, -0.09225f, 0.99120f),
            new Quaternion(0.00045f, 0.00016f, -0.12404f, 0.99228f),
            new Quaternion(0.00825f, 0.05113f, -0.05970f, 0.99687f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(0.03404f, 0.60384f, 0.22049f, 0.76525f),
            new Quaternion(0.67412f, -0.60951f, -0.27108f, -0.31715f),
            new Quaternion(0.75808f, -0.54789f, -0.23602f, -0.26347f),
            new Quaternion(0.82156f, -0.49712f, -0.15118f, -0.23466f),
            new Quaternion(0.86616f, -0.46885f, -0.13690f, -0.10587f)
        };



        public static Vector3[] RIGHT_HAND_IDLE_POSITIONS = new Vector3[]
        {
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(-0.03f, 0.04f, 0.16f),
            new Vector3(-0.01f, 0.03f, 0.03f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.03f, 0.02f),
            new Vector3(0.07f, 0.01f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.02f, 0.02f),
            new Vector3(0.06f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.05f, 0.06f),
            new Vector3(-0.04f, -0.04f, 0.02f),
            new Vector3(-0.03f, -0.07f, 0.04f),
            new Vector3(-0.03f, -0.09f, 0.08f),
            new Vector3(-0.02f, -0.08f, 0.12f)
        };
        
    
    
        /////////////////////////////////////////////////
        /// left hand


        public static Vector3[] LEFT_HAND_CLENCH_POSITIONS = new Vector3[]
        {
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(-0.03f, 0.04f, 0.16f),
            new Vector3(-0.02f, 0.03f, 0.02f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.02f, 0.01f),
            new Vector3(0.07f, 0.01f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.01f, 0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.02f, 0.02f),
            new Vector3(0.06f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.09f),
            new Vector3(-0.01f, 0.02f, 0.10f),
            new Vector3(0.00f, 0.01f, 0.12f),
            new Vector3(0.00f, -0.01f, 0.13f),
            new Vector3(0.00f, -0.02f, 0.14f)
        };


        public static Quaternion[] LEFT_HAND_CLENCH_ROTATIONS = new Quaternion[]
        {
            new Quaternion(0.00000f, 1.00000f, 0.00000f, 0.00000f),
            new Quaternion(-0.07861f, -0.92028f, 0.37930f, -0.05515f),
            new Quaternion(-0.22570f, -0.83634f, 0.12641f, 0.48333f),
            new Quaternion(-0.01330f, 0.08290f, -0.43945f, 0.89434f),
            new Quaternion(0.00073f, -0.00120f, -0.58829f, 0.80865f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.61731f, -0.44919f, -0.51087f, 0.39517f),
            new Quaternion(-0.04185f, 0.11181f, -0.72633f, 0.67690f),
            new Quaternion(-0.00057f, 0.11520f, -0.81730f, 0.56458f),
            new Quaternion(-0.01076f, 0.02724f, -0.66611f, 0.74528f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.51420f, -0.48370f, -0.47835f, 0.52232f),
            new Quaternion(-0.09487f, -0.05423f, -0.72290f, 0.68225f),
            new Quaternion(0.00768f, -0.09770f, -0.76360f, 0.63821f),
            new Quaternion(-0.06367f, 0.00036f, -0.75306f, 0.65486f),
            new Quaternion(0.00000f, 0.00000f, -0.04013f, 0.99919f),
            new Quaternion(-0.48961f, -0.46400f, -0.52064f, 0.52337f),
            new Quaternion(-0.08827f, 0.01267f, -0.70854f, 0.70002f),
            new Quaternion(-0.00059f, -0.03983f, -0.74642f, 0.66428f),
            new Quaternion(-0.02712f, -0.00544f, -0.77882f, 0.62664f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.47977f, -0.37993f, -0.63020f, 0.47783f),
            new Quaternion(-0.09407f, 0.06263f, -0.69046f, 0.71449f),
            new Quaternion(0.00313f, 0.03776f, -0.71138f, 0.70178f),
            new Quaternion(-0.00809f, -0.00301f, -0.73619f, 0.67672f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.54887f, 0.11779f, -0.75784f, 0.33250f),
            new Quaternion(0.13244f, -0.87308f, -0.45493f, -0.11498f),
            new Quaternion(0.17098f, -0.92267f, -0.34508f, -0.01925f),
            new Quaternion(0.15012f, -0.95217f, -0.25831f, -0.06414f),
            new Quaternion(0.07684f, -0.97958f, -0.18577f, -0.00373f)
        };


        public static Vector3[] LEFT_HAND_IDLE_POSITIONS = new Vector3[]
        {
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(-0.03f, 0.04f, 0.16f),
            new Vector3(-0.01f, 0.03f, 0.03f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.03f, 0.02f),
            new Vector3(0.07f, 0.01f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.01f, 0.02f),
            new Vector3(0.07f, 0.00f, 0.00f),
            new Vector3(0.04f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.00f, -0.02f, 0.02f),
            new Vector3(0.06f, 0.00f, 0.00f),
            new Vector3(0.03f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(0.02f, 0.00f, 0.00f),
            new Vector3(-0.01f, 0.06f, 0.06f),
            new Vector3(-0.04f, -0.04f, 0.02f),
            new Vector3(-0.04f, -0.08f, 0.05f),
            new Vector3(-0.04f, -0.09f, 0.08f),
            new Vector3(-0.03f, -0.09f, 0.12f)
        };

        public static Quaternion[] LEFT_HAND_IDLE_ROTATIONS = new Quaternion[]
        {
            new Quaternion(0.00000f, 1.00000f, 0.00000f, 0.00000f),
            new Quaternion(-0.07861f, -0.92028f, 0.37930f, -0.05515f),
            new Quaternion(-0.24104f, -0.76422f, 0.45859f, 0.38413f),
            new Quaternion(0.08519f, 0.00005f, -0.28144f, 0.95579f),
            new Quaternion(0.00520f, -0.02148f, -0.15889f, 0.98705f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.64425f, -0.42213f, -0.47820f, 0.42198f),
            new Quaternion(0.08568f, 0.02357f, -0.19161f, 0.97744f),
            new Quaternion(0.04565f, 0.00437f, -0.09588f, 0.99434f),
            new Quaternion(-0.00205f, 0.02276f, -0.15681f, 0.98736f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.54672f, -0.46075f, -0.44252f, 0.54128f),
            new Quaternion(-0.17867f, 0.04782f, -0.24334f, 0.95214f),
            new Quaternion(0.02037f, -0.01006f, -0.21894f, 0.97547f),
            new Quaternion(-0.01046f, 0.02643f, -0.19180f, 0.98102f),
            new Quaternion(0.00000f, 0.00000f, -0.04013f, 0.99919f),
            new Quaternion(-0.51669f, -0.42989f, -0.49555f, 0.55014f),
            new Quaternion(-0.17290f, 0.11434f, -0.29727f, 0.93202f),
            new Quaternion(-0.00220f, -0.00044f, -0.22544f, 0.97425f),
            new Quaternion(-0.00472f, 0.01180f, -0.35618f, 0.93433f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(-0.52692f, -0.32674f, -0.58402f, 0.52394f),
            new Quaternion(-0.20060f, 0.15258f, -0.36498f, 0.89625f),
            new Quaternion(0.00186f, 0.00041f, -0.25202f, 0.96772f),
            new Quaternion(-0.01947f, 0.04834f, -0.26703f, 0.96228f),
            new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
            new Quaternion(0.20275f, 0.59427f, 0.24944f, 0.73724f),
            new Quaternion(0.62353f, -0.66381f, -0.29373f, -0.29033f),
            new Quaternion(0.67806f, -0.65929f, -0.26568f, -0.18705f),
            new Quaternion(0.73679f, -0.63476f, -0.14394f, -0.18304f),
            new Quaternion(0.75841f, -0.63934f, -0.12668f, -0.00366f)
        };
        
        
        
        //==========================================================
        
 
        //-------------------------------------------------------


        private bool isRightHand;
        
        public static bool isRightClenching = false;
        public static bool isLeftClenching = false;
        public static bool lockRightClench = false;
        public static bool lockLeftClench = false;
        
        //--------------------------------------------------------
        
        
        
        
        

        protected virtual void OnEnable()
        {
        
            CheckSkeletonAction();
            SteamVR_Input.onSkeletonsUpdated += SteamVR_Input_OnSkeletonsUpdated;

            if (skeletonAction != null)
            {
                skeletonAction.onDeviceConnectedChanged += OnDeviceConnectedChanged;
                skeletonAction.onTrackingChanged += OnTrackingChanged;
            }

            
            //my code
            ///////////////////////
            string objName = gameObject.name; 
            objName = objName.ToLower();

            if (objName.Contains("right"))
            {
                this.isRightHand = true;
            }
            else this.isRightHand = false;
           // Debug.Log("bones length:"+bones.Length);

            /////////////////////////////
        }

        
        
   //=================================     
        
        
        

        protected virtual void OnDisable()
        {
            SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;

            if (skeletonAction != null)
            {
                skeletonAction.onDeviceConnectedChanged -= OnDeviceConnectedChanged;
                skeletonAction.onTrackingChanged -= OnTrackingChanged;
            }
        }

        private void OnDeviceConnectedChanged(SteamVR_Action_Skeleton fromAction, bool deviceConnected)
        {
            if (onConnectedChanged != null)
                onConnectedChanged.Invoke(this, inputSource, deviceConnected);
            if (onConnectedChangedEvent != null)
                onConnectedChangedEvent.Invoke(this, inputSource, deviceConnected);
        }

        private void OnTrackingChanged(SteamVR_Action_Skeleton fromAction, ETrackingResult trackingState)
        {
            if (onTrackingChanged != null)
                onTrackingChanged.Invoke(this, inputSource, trackingState);
            if (onTrackingChangedEvent != null)
                onTrackingChangedEvent.Invoke(this, inputSource, trackingState);
        }

        protected virtual void SteamVR_Input_OnSkeletonsUpdated(bool skipSendingEvents)
        {
            UpdateSkeleton();
        }

        protected virtual void UpdateSkeleton()
        {
            if (skeletonAction == null)
                return;

            if (updatePose)
                UpdatePose();

            if (blendPoser != null && skeletonBlend < 1)
            {
                if (blendSnapshot == null) blendSnapshot = blendPoser.GetBlendedPose(this);
                blendSnapshot = blendPoser.GetBlendedPose(this);
            }

            if (rangeOfMotionBlendRoutine == null)
            {
                if (temporaryRangeOfMotion != null)
                    skeletonAction.SetRangeOfMotion(temporaryRangeOfMotion.Value);
                else
                    skeletonAction.SetRangeOfMotion(rangeOfMotion); //this may be a frame behind

                UpdateSkeletonTransforms();
            }
        }

        /// <summary>
        /// Sets a temporary range of motion for this action that can easily be reset (using ResetTemporaryRangeOfMotion).
        /// This is useful for short range of motion changes, for example picking up a controller shaped object
        /// </summary>
        /// <param name="newRangeOfMotion">The new range of motion you want to apply (temporarily)</param>
        /// <param name="blendOverSeconds">How long you want the blend to the new range of motion to take (in seconds)</param>
        public void SetTemporaryRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            if (rangeOfMotion != newRangeOfMotion || temporaryRangeOfMotion != newRangeOfMotion)
            {
                TemporaryRangeOfMotionBlend(newRangeOfMotion, blendOverSeconds);
            }
        }

        /// <summary>
        /// Resets the previously set temporary range of motion.
        /// Will return to the range of motion defined by the rangeOfMotion field.
        /// </summary>
        /// <param name="blendOverSeconds">How long you want the blend to the standard range of motion to take (in seconds)</param>
        public void ResetTemporaryRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            ResetTemporaryRangeOfMotionBlend(blendOverSeconds);
        }

        /// <summary>
        /// Permanently sets the range of motion for this component.
        /// </summary>
        /// <param name="newRangeOfMotion">
        /// The new range of motion to be set.
        /// WithController being the best estimation of where fingers are wrapped around the controller (pressing buttons, etc).
        /// WithoutController being a range between a flat hand and a fist.</param>
        /// <param name="blendOverSeconds">How long you want the blend to the new range of motion to take (in seconds)</param>
        public void SetRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            if (rangeOfMotion != newRangeOfMotion)
            {
                RangeOfMotionBlend(newRangeOfMotion, blendOverSeconds);
            }
        }

        /// <summary>
        /// Blend from the current skeletonBlend amount to full bone data. (skeletonBlend = 1)
        /// </summary>
        /// <param name="overTime">How long you want the blend to take (in seconds)</param>
        public void BlendToSkeleton(float overTime = 0.1f)
        {
            if (blendPoser != null)
                blendSnapshot = blendPoser.GetBlendedPose(this);
            blendPoser = null;
            BlendTo(1, overTime);
        }

        /// <summary>
        /// Blend from the current skeletonBlend amount to pose animation. (skeletonBlend = 0)
        /// Note: This will ignore the root position and rotation of the pose.
        /// </summary>
        /// <param name="overTime">How long you want the blend to take (in seconds)</param>
        public void BlendToPoser(SteamVR_Skeleton_Poser poser, float overTime = 0.1f)
        {
            if (poser == null)
                return;

            blendPoser = poser;
            BlendTo(0, overTime);
        }

        /// <summary>
        /// Blend from the current skeletonBlend amount to full animation data (no bone data. skeletonBlend = 0)
        /// </summary>
        /// <param name="overTime">How long you want the blend to take (in seconds)</param>
        public void BlendToAnimation(float overTime = 0.1f)
        {
            BlendTo(0, overTime);
        }

        /// <summary>
        /// Blend from the current skeletonBlend amount to a specified new amount.
        /// </summary>
        /// <param name="blendToAmount">The amount of blend you want to apply.
        /// 0 being fully set by animations, 1 being fully set by bone data from the action.</param>
        /// <param name="overTime">How long you want the blend to take (in seconds)</param>
        public void BlendTo(float blendToAmount, float overTime)
        {
            if (blendRoutine != null)
                StopCoroutine(blendRoutine);

            if (this.gameObject.activeInHierarchy)
                blendRoutine = StartCoroutine(DoBlendRoutine(blendToAmount, overTime));
        }


        protected IEnumerator DoBlendRoutine(float blendToAmount, float overTime)
        {
            float startTime = Time.time;
            float endTime = startTime + overTime;

            float startAmount = skeletonBlend;

            while (Time.time < endTime)
            {
                yield return null;
                skeletonBlend = Mathf.Lerp(startAmount, blendToAmount, (Time.time - startTime) / overTime);
            }

            skeletonBlend = blendToAmount;
            blendRoutine = null;
        }

        protected void RangeOfMotionBlend(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds)
        {
            if (rangeOfMotionBlendRoutine != null)
                StopCoroutine(rangeOfMotionBlendRoutine);

            EVRSkeletalMotionRange oldRangeOfMotion = rangeOfMotion;
            rangeOfMotion = newRangeOfMotion;

            if (this.gameObject.activeInHierarchy)
            {
                rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
            }
        }

        protected void TemporaryRangeOfMotionBlend(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds)
        {
            if (rangeOfMotionBlendRoutine != null)
                StopCoroutine(rangeOfMotionBlendRoutine);

            EVRSkeletalMotionRange oldRangeOfMotion = rangeOfMotion;
            if (temporaryRangeOfMotion != null)
                oldRangeOfMotion = temporaryRangeOfMotion.Value;

            temporaryRangeOfMotion = newRangeOfMotion;

            if (this.gameObject.activeInHierarchy)
            {
                rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
            }
        }

        protected void ResetTemporaryRangeOfMotionBlend(float blendOverSeconds)
        {
            if (temporaryRangeOfMotion != null)
            {
                if (rangeOfMotionBlendRoutine != null)
                    StopCoroutine(rangeOfMotionBlendRoutine);

                EVRSkeletalMotionRange oldRangeOfMotion = temporaryRangeOfMotion.Value;

                EVRSkeletalMotionRange newRangeOfMotion = rangeOfMotion;

                temporaryRangeOfMotion = null;

                if (this.gameObject.activeInHierarchy)
                {
                    rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
                }
            }
        }

        protected IEnumerator DoRangeOfMotionBlend(EVRSkeletalMotionRange oldRangeOfMotion, EVRSkeletalMotionRange newRangeOfMotion, float overTime)
        {
            float startTime = Time.time;
            float endTime = startTime + overTime;

            Vector3[] oldBonePositions;
            Quaternion[] oldBoneRotations;

            Vector3[] newBonePositions;
            Quaternion[] newBoneRotations;

            while (Time.time < endTime)
            {
                yield return null;
                float lerp = (Time.time - startTime) / overTime;

                if (skeletonBlend > 0)
                {
                    skeletonAction.SetRangeOfMotion(oldRangeOfMotion);
                    skeletonAction.UpdateValueWithoutEvents();
                    oldBonePositions = (Vector3[])GetBonePositions().Clone();
                    oldBoneRotations = (Quaternion[])GetBoneRotations().Clone();

                    skeletonAction.SetRangeOfMotion(newRangeOfMotion);
                    skeletonAction.UpdateValueWithoutEvents();
                    newBonePositions = GetBonePositions();
                    newBoneRotations = GetBoneRotations();

                    for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                    {
                        if (bones[boneIndex] == null)
                            continue;

                        if (SteamVR_Utils.IsValid(newBoneRotations[boneIndex]) == false || SteamVR_Utils.IsValid(oldBoneRotations[boneIndex]) == false)
                        {
                            continue;
                        }

                        Vector3 blendedRangeOfMotionPosition = Vector3.Lerp(oldBonePositions[boneIndex], newBonePositions[boneIndex], lerp);
                        Quaternion blendedRangeOfMotionRotation = Quaternion.Lerp(oldBoneRotations[boneIndex], newBoneRotations[boneIndex], lerp);

                        if (skeletonBlend < 1)
                        {
                            if (blendPoser != null)
                            {

                                SetBonePosition(boneIndex, Vector3.Lerp(blendSnapshot.bonePositions[boneIndex], blendedRangeOfMotionPosition, skeletonBlend));
                                SetBoneRotation(boneIndex, Quaternion.Lerp(GetBlendPoseForBone(boneIndex, blendedRangeOfMotionRotation), blendedRangeOfMotionRotation, skeletonBlend));
                            }
                            else
                            {
                                SetBonePosition(boneIndex, Vector3.Lerp(bones[boneIndex].localPosition, blendedRangeOfMotionPosition, skeletonBlend));
                                SetBoneRotation(boneIndex, Quaternion.Lerp(bones[boneIndex].localRotation, blendedRangeOfMotionRotation, skeletonBlend));
                            }
                        }
                        else
                        {
                            SetBonePosition(boneIndex, blendedRangeOfMotionPosition);
                            SetBoneRotation(boneIndex, blendedRangeOfMotionRotation);
                        }
                    }
                }

                if (onBoneTransformsUpdated != null)
                    onBoneTransformsUpdated.Invoke(this, inputSource);
                if (onBoneTransformsUpdatedEvent != null)
                    onBoneTransformsUpdatedEvent.Invoke(this, inputSource);

            }

            rangeOfMotionBlendRoutine = null;
        }

        //why does this exist
        protected virtual Quaternion GetBlendPoseForBone(int boneIndex, Quaternion skeletonRotation)
        {
            Quaternion poseRotation = blendSnapshot.boneRotations[boneIndex];
            return poseRotation;
        }

        
        //============================Code injection =======================================//
        
        
        public virtual void UpdateSkeletonTransforms()
        {
            
            
    //    Vector3[] bonePositions = GetBonePositions();
      //  Quaternion[] boneRotations = GetBoneRotations();
      
      Vector3[] bonePositions;// = GetBonePositions();
      Quaternion[] boneRotations;// = GetBoneRotations();


      ///////////////////code injection here ====================
      ///
    //  Debug.Log("update");
      if (isRightHand)
      {
          if (isRightClenching || lockRightClench)
          {
              bonePositions = RIGHT_HAND_CLENCH_POSITIONS;
              boneRotations = RIGHT_HAND_CLENCH_ROTATIONS;
          }
          else
          {
              bonePositions = RIGHT_HAND_IDLE_POSITIONS;
              boneRotations = RIGHT_HAND_IDLE_ROTATIONS;
          }
      }
      else
      {
          if (isLeftClenching || lockLeftClench)
          {
              bonePositions = LEFT_HAND_CLENCH_POSITIONS;
              boneRotations = LEFT_HAND_CLENCH_ROTATIONS;
          }
          else
          {
              bonePositions = LEFT_HAND_IDLE_POSITIONS;
              boneRotations = LEFT_HAND_IDLE_ROTATIONS;
          }
      }
      
      ///////////////////code injection end ====================
      
   //    Vector3[] bonePositions = LEFT_HAND_IDLE_POSITIONS;
   //    Quaternion[] boneRotations = LEFT_HAND_IDLE_ROTATIONS;
   
   // Debug.Log("========start==============");
   //
   // int index = 0;
   //    foreach (Vector3 v in bonePositions)
   //    {
   //        Debug.Log("vec:"+v +" "+index);
   //        index++;
   //    }
   //
   //    index = 0;
   //    foreach (Quaternion q in boneRotations)
   //    {
   //        Debug.Log("quad:"+q+" "+index);
   //    }
   //
   // Debug.Log("==========end==============");
   //

            if (skeletonBlend <= 0)
            {
                if (blendPoser != null)
                {
                    SteamVR_Skeleton_Pose_Hand mainPose = blendPoser.skeletonMainPose.GetHand(inputSource);
                    
                    
                    
               //     Debug.Log(inputSource.ToString());
                    for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                    {
                        if (bones[boneIndex] == null)
                            continue;

                        if ((boneIndex == SteamVR_Skeleton_JointIndexes.wrist && mainPose.ignoreWristPoseData) ||
                            (boneIndex == SteamVR_Skeleton_JointIndexes.root && mainPose.ignoreRootPoseData))
                        {
                            SetBonePosition(boneIndex, bonePositions[boneIndex]);
                            SetBoneRotation(boneIndex, boneRotations[boneIndex]);
                        }
                        else
                        {
                            Quaternion poseRotation = GetBlendPoseForBone(boneIndex, boneRotations[boneIndex]);

                            SetBonePosition(boneIndex, blendSnapshot.bonePositions[boneIndex]);
                            SetBoneRotation(boneIndex, poseRotation);
                        }
                    }
                }
                else
                {
                    for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                    {

                        Quaternion poseRotation = GetBlendPoseForBone(boneIndex, boneRotations[boneIndex]);

                        SetBonePosition(boneIndex, blendSnapshot.bonePositions[boneIndex]);
                        SetBoneRotation(boneIndex, poseRotation);

                    }
                }
            }
            else if (skeletonBlend >= 1)
            {
                for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                {
                    if (bones[boneIndex] == null)
                        continue;
                    
                    //note: if you add a child to the vrglove model you will get an index out of bounds exception here
                    //- Talon
                    SetBonePosition(boneIndex, bonePositions[boneIndex]);
                    SetBoneRotation(boneIndex, boneRotations[boneIndex]);
                }
            }
            else
            {
                
                //skeletonBlend is from 0-1
                for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                {
                    if (bones[boneIndex] == null)
                        continue;

                    if (blendPoser != null)
                    {
                        SteamVR_Skeleton_Pose_Hand mainPose = blendPoser.skeletonMainPose.GetHand(inputSource);

                        if ((boneIndex == SteamVR_Skeleton_JointIndexes.wrist && mainPose.ignoreWristPoseData) ||
                            (boneIndex == SteamVR_Skeleton_JointIndexes.root && mainPose.ignoreRootPoseData))
                        {
                            SetBonePosition(boneIndex, bonePositions[boneIndex]);
                            SetBoneRotation(boneIndex, boneRotations[boneIndex]);
                        }
                        else
                        {
                            //Quaternion poseRotation = GetBlendPoseForBone(boneIndex, boneRotations[boneIndex]);

                            SetBonePosition(boneIndex, Vector3.Lerp(blendSnapshot.bonePositions[boneIndex], bonePositions[boneIndex], skeletonBlend));
                            SetBoneRotation(boneIndex, Quaternion.Lerp(blendSnapshot.boneRotations[boneIndex], boneRotations[boneIndex], skeletonBlend));
                            //SetBoneRotation(boneIndex, GetBlendPoseForBone(boneIndex, boneRotations[boneIndex]));
                        }
                    }
                    else
                    {
                        if (blendSnapshot == null)
                        {
                            SetBonePosition(boneIndex, Vector3.Lerp(bones[boneIndex].localPosition, bonePositions[boneIndex], skeletonBlend));
                            SetBoneRotation(boneIndex, Quaternion.Lerp(bones[boneIndex].localRotation, boneRotations[boneIndex], skeletonBlend));
                        }
                        else
                        {
                            SetBonePosition(boneIndex, Vector3.Lerp(blendSnapshot.bonePositions[boneIndex], bonePositions[boneIndex], skeletonBlend));
                            SetBoneRotation(boneIndex, Quaternion.Lerp(blendSnapshot.boneRotations[boneIndex], boneRotations[boneIndex], skeletonBlend));
                        }
                    }
                }
            }


            if (onBoneTransformsUpdated != null)
                onBoneTransformsUpdated.Invoke(this, inputSource);
            if (onBoneTransformsUpdatedEvent != null)
                onBoneTransformsUpdatedEvent.Invoke(this, inputSource);
        }

  //================================      
        
        
        
        
        public virtual void SetBonePosition(int boneIndex, Vector3 localPosition)
        {
            if (onlySetRotations == false) //ignore position sets if we're only setting rotations
                bones[boneIndex].localPosition = localPosition;
        }

        public virtual void SetBoneRotation(int boneIndex, Quaternion localRotation)
        {
            bones[boneIndex].localRotation = localRotation;
        }

        /// <summary>
        /// Gets the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes
        /// </summary>
        /// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
        public virtual Transform GetBone(int joint)
        {
            if (bones == null || bones.Length == 0)
                Awake();

            return bones[joint];
        }


        /// <summary>
        /// Gets the position of the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes
        /// </summary>
        /// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
        /// <param name="local">true to get the localspace position for the joint (position relative to this joint's parent)</param>
        public Vector3 GetBonePosition(int joint, bool local = false)
        {
            if (local)
                return bones[joint].localPosition;
            else
                return bones[joint].position;
        }

        /// <summary>
        /// Gets the rotation of the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes
        /// </summary>
        /// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
        /// <param name="local">true to get the localspace rotation for the joint (rotation relative to this joint's parent)</param>
        public Quaternion GetBoneRotation(int joint, bool local = false)
        {
            if (local)
                return bones[joint].localRotation;
            else
                return bones[joint].rotation;
        }

        protected Vector3[] GetBonePositions()
        {
            if (skeletonAvailable)
            {
                Vector3[] rawSkeleton = skeletonAction.GetBonePositions();
                if (mirroring == MirrorType.LeftToRight || mirroring == MirrorType.RightToLeft)
                {
                    for (int boneIndex = 0; boneIndex < rawSkeleton.Length; boneIndex++)
                    {
                        rawSkeleton[boneIndex] = MirrorPosition(boneIndex, rawSkeleton[boneIndex]);
                    }
                }

                return rawSkeleton;
            }
            else
            {
                //fallback to getting skeleton pose from skeletonPoser
                if (fallbackPoser != null)
                {
                    return fallbackPoser.GetBlendedPose(skeletonAction, inputSource).bonePositions;
                }
                else
                {
                    Debug.LogError("Skeleton Action is not bound, and you have not provided a fallback SkeletonPoser. Please create one to drive hand animation when no skeleton data is available.", this);
                    return null;
                }
            }
        }

        protected static readonly Quaternion rightFlipAngle = Quaternion.AngleAxis(180, Vector3.right);
        protected Quaternion[] GetBoneRotations()
        {
            if (skeletonAvailable)
            {
                Quaternion[] rawSkeleton = skeletonAction.GetBoneRotations();
                if (mirroring == MirrorType.LeftToRight || mirroring == MirrorType.RightToLeft)
                {
                    for (int boneIndex = 0; boneIndex < rawSkeleton.Length; boneIndex++)
                    {
                        rawSkeleton[boneIndex] = MirrorRotation(boneIndex, rawSkeleton[boneIndex]);
                    }
                }

                return rawSkeleton;

            }
            else
            {
                //fallback to getting skeleton pose from skeletonPoser
                if (fallbackPoser != null)
                {
                    return fallbackPoser.GetBlendedPose(skeletonAction, inputSource).boneRotations;
                }
                else
                {
                    Debug.LogError("Skeleton Action is not bound, and you have not provided a fallback SkeletonPoser. Please create one to drive hand animation when no skeleton data is available.", this);
                    return null;
                }
            }
        }

        public static Vector3 MirrorPosition(int boneIndex, Vector3 rawPosition)
        {
            if (boneIndex == SteamVR_Skeleton_JointIndexes.wrist || IsMetacarpal(boneIndex))
            {
                rawPosition.Scale(new Vector3(-1, 1, 1));
            }
            else if (boneIndex != SteamVR_Skeleton_JointIndexes.root)
            {
                rawPosition = rawPosition * -1;
            }

            return rawPosition;
        }

        public static Quaternion MirrorRotation(int boneIndex, Quaternion rawRotation)
        {
            if (boneIndex == SteamVR_Skeleton_JointIndexes.wrist)
            {
                rawRotation.y = rawRotation.y * -1;
                rawRotation.z = rawRotation.z * -1;
            }

            if (IsMetacarpal(boneIndex))
            {
                rawRotation = rightFlipAngle * rawRotation;
            }

            return rawRotation;
        }

        protected virtual void UpdatePose()
        {
            if (skeletonAction == null)
                return;

            Vector3 skeletonPosition = skeletonAction.GetLocalPosition();
            Quaternion skeletonRotation = skeletonAction.GetLocalRotation();
            if (origin == null)
            {
                if (this.transform.parent != null)
                {
                    skeletonPosition = this.transform.parent.TransformPoint(skeletonPosition);
                    skeletonRotation = this.transform.parent.rotation * skeletonRotation;
                }
            }
            else
            {
                skeletonPosition = origin.TransformPoint(skeletonPosition);
                skeletonRotation = origin.rotation * skeletonRotation;
            }

            if (skeletonAction.poseChanged)
            {
                if (onTransformChanged != null)
                    onTransformChanged.Invoke(this, inputSource);
                if (onTransformChangedEvent != null)
                    onTransformChangedEvent.Invoke(this, inputSource);
            }

            this.transform.position = skeletonPosition;
            this.transform.rotation = skeletonRotation;

            if (onTransformUpdated != null)
                onTransformUpdated.Invoke(this, inputSource);
        }

        /// <summary>
        /// Returns an array of positions/rotations that represent the state of each bone in a reference pose.
        /// </summary>
        /// <param name="referencePose">Which reference pose to return</param>
        public void ForceToReferencePose(EVRSkeletalReferencePose referencePose)
        {
            bool temporarySession = false;
            if (Application.isEditor && Application.isPlaying == false)
            {
                temporarySession = SteamVR.InitializeTemporarySession(true);
                Awake();

#if UNITY_EDITOR
                //gotta wait a bit for steamvr input to startup //todo: implement steamvr_input.isready
                string title = "SteamVR";
                string text = "Getting reference pose...";
                float msToWait = 3000;
                float increment = 100;
                for (float timer = 0; timer < msToWait; timer += increment)
                {
                    bool cancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar(title, text, timer / msToWait);
                    if (cancel)
                    {
                        UnityEditor.EditorUtility.ClearProgressBar();

                        if (temporarySession)
                            SteamVR.ExitTemporarySession();
                        return;
                    }
                    System.Threading.Thread.Sleep((int)increment);
                }
                UnityEditor.EditorUtility.ClearProgressBar();
#endif

                skeletonAction.actionSet.Activate();

                SteamVR_ActionSet_Manager.UpdateActionStates(true);

                skeletonAction.UpdateValueWithoutEvents();
            }

            if (skeletonAction.active == false)
            {
                Debug.LogError("<b>[SteamVR Input]</b> Please turn on your " + inputSource.ToString() + " controller and ensure SteamVR is open.", this);
                return;
            }

            SteamVR_Utils.RigidTransform[] transforms = skeletonAction.GetReferenceTransforms(EVRSkeletalTransformSpace.Parent, referencePose);

            if (transforms == null || transforms.Length == 0)
            {
                Debug.LogError("<b>[SteamVR Input]</b> Unable to get the reference transform for " + inputSource.ToString() + ". Please make sure SteamVR is open and both controllers are connected.", this);
            }

            if (mirroring == MirrorType.LeftToRight || mirroring == MirrorType.RightToLeft)
            {
                for (int boneIndex = 0; boneIndex < transforms.Length; boneIndex++)
                {
                    bones[boneIndex].localPosition = MirrorPosition(boneIndex, transforms[boneIndex].pos);
                    bones[boneIndex].localRotation = MirrorRotation(boneIndex, transforms[boneIndex].rot);
                }
            }
            else
            {
                for (int boneIndex = 0; boneIndex < transforms.Length; boneIndex++)
                {
                    bones[boneIndex].localPosition = transforms[boneIndex].pos;
                    bones[boneIndex].localRotation = transforms[boneIndex].rot;
                }
            }

            if (temporarySession)
                SteamVR.ExitTemporarySession();
        }

        protected static bool IsMetacarpal(int boneIndex)
        {
            return (boneIndex == SteamVR_Skeleton_JointIndexes.indexMetacarpal ||
                boneIndex == SteamVR_Skeleton_JointIndexes.middleMetacarpal ||
                boneIndex == SteamVR_Skeleton_JointIndexes.ringMetacarpal ||
                boneIndex == SteamVR_Skeleton_JointIndexes.pinkyMetacarpal ||
                boneIndex == SteamVR_Skeleton_JointIndexes.thumbMetacarpal);
        }

        public enum MirrorType
        {
            None,
            LeftToRight,
            RightToLeft
        }

        public delegate void ActiveChangeHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource, bool active);
        public delegate void ChangeHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource);
        public delegate void UpdateHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource);
        public delegate void TrackingChangeHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource, ETrackingResult trackingState);
        public delegate void ValidPoseChangeHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource, bool validPose);
        public delegate void DeviceConnectedChangeHandler(SteamVR_Behaviour_Skeleton fromAction, SteamVR_Input_Sources inputSource, bool deviceConnected);
    }
}