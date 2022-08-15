using UnityEngine;
using System;
using System.Collections;

namespace BV
{

    public class FootIK : MonoBehaviour
    {

        public Animator anim;

        private Vector3 rightFootPosition, rightFootIkPosition;
        private Vector3 leftFootPosition, leftFootIkPosition;
        private Quaternion rightFootIkRotation, leftFootIkRotation;
        private float lastRightFootPositionY, lastLeftFootPositionY;

        [Header("Feet Grounded")]
        public bool enableFeetIk = true;
        [Range(0, 2)]
        [SerializeField]
        private float heightFromGroundRaycast = 1.14f;
        [Range(0, 2)]
        [SerializeField]
        private float raycastDownDistance = 1.5f;
        [SerializeField]
        private LayerMask environmentLayer;
        [Range(0, 1)]
        [SerializeField]
        private float pelvisOffset = 0f;
        [Range(0, 1)]
        [SerializeField]
        private float feetToIkPositionSpeed = 0.5f;

        public string leftFootAnimVariableName = "leftFootCurve";
        public string rightFootAnimVariableName = "rightFootCurve";

        public bool useProIkFeature = false;
        public bool showSolverDebug = true;


        void Start()
        {
            anim = gameObject.GetComponent<Animator>();
            if (anim == null)
                anim = gameObject.GetComponentInChildren<Animator>();
        }
        ///We are updating AdjustFR method and also find the position of each foot inside IkDolver
        private void FixedUpdate()
        {
            if (enableFeetIk == false)
            {
                return;
            }
            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            //find and to the raycast to the ground to fine position
            FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation);//handle the solver for right foot
            FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation);
        }

        private void OnAnimatorIK()
        {
            if (enableFeetIk == false)
            {
                return;
            }

            //right foot position and rotation -- utilise the pro features in here
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

            if (useProIkFeature)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
            }

            MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);


            //left foot position and rotation -- utilise the pro features in here
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

            if (useProIkFeature)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
            }

            MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
        }

        void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder,
        Quaternion rotationIkHolder, ref float lastFootPositionY)
        {
            Vector3 targetIkPosition = anim.GetIKPosition(foot);

            if (positionIkHolder != Vector3.zero)
            {
                targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
                positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
                targetIkPosition.y += yVariable;

                lastFootPositionY = yVariable;

                targetIkPosition = transform.TransformPoint(targetIkPosition);

                anim.SetIKRotation(foot, rotationIkHolder);
            }

            anim.SetIKPosition(foot, targetIkPosition);
        }

        public float MovePelvisHeight()
        {

            float lOffsetPosition = leftFootIkPosition.y;
            float rOffsetPosition = rightFootIkPosition.y;
            float totalOffset;

            if (Math.Abs(lOffsetPosition - rOffsetPosition) > 0.42f)
                totalOffset = (lOffsetPosition > rOffsetPosition) ? lOffsetPosition : rOffsetPosition;
            else
                totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

            return totalOffset;
        }

        private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions,
        ref Quaternion feetIkRotations)
        {
            //raycast handling section
            RaycastHit feetOutHit;

            if (showSolverDebug)
                Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);

            if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
            {
                //find our feet ik position from the sky position
                feetIkPositions = fromSkyPosition;
                feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
                feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

                return;
            }
            feetIkPositions = Vector3.zero;//it did`n work

        }

        private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
        {
            feetPositions = anim.GetBoneTransform(foot).position;
            feetPositions.y = transform.position.y + heightFromGroundRaycast;
        }
    }
}