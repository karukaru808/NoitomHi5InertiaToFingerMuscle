using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace HI5
{
    public class NoitomHi5InertiaToFingerMuscle : MonoBehaviour
    {
        private const int ConstIndexHi5Hand = (int) Bones.Hand;
        private const int ConstIndexNumOfHi5Bones = (int) Bones.NumOfHI5Bones;
        private const int ConstIndexHandTypeBoth = (int) HandType.Both;
        private static readonly int[] ConstIndexThumbMuscle = {55, 75};
        private static readonly float[] ConstSpreadMuscle = {0.1f, 0.4f, 0.2f, 0f};
        private static readonly int[] ConstCoefficient = {1, -1};

        [SerializeField] private Animator animator;
        [SerializeField] private HandType handType = HandType.Both;
        [SerializeField, Range(0f, 1f)] private float handRotationWeight;

        private enum HandType
        {
            Left,
            Right,
            Both
        }

        private HumanPoseHandler _humanPoseHandler;
        private HumanPose _currentHumanPose;

        private Transform[] _handBones;
        private Quaternion[] _defaultHandRotation;
        private float[] _defaultThumbMuscle;

        private HI5_Source _hi5Source;

        private Vector3[] _leftHi5EulerAngles = new Vector3[ConstIndexNumOfHi5Bones];
        private Vector3[] _rightHi5EulerAngles = new Vector3[ConstIndexNumOfHi5Bones];

        private void OnEnable()
        {
            Connect();
        }

        private void Connect()
        {
            if (!HI5_Manager.IsConnected)
            {
                HI5_Manager.Connect();
            }

            _hi5Source = HI5_Manager.GetHI5Source();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Disconnect()
        {
            if (HI5_Manager.IsConnected)
            {
                HI5_Manager.DisConnect();
            }
        }

        private void Awake()
        {
            if (animator == null || !animator.isHuman)
            {
                Debug.LogError($"{nameof(NoitomHi5InertiaToFingerMuscle)}: 正しいAnimatorを指定してください。");
                enabled = false;
                return;
            }

            _humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            _humanPoseHandler.GetHumanPose(ref _currentHumanPose);

            _handBones = new Transform[ConstIndexHandTypeBoth];
            _defaultHandRotation = new Quaternion[ConstIndexHandTypeBoth];
            _defaultThumbMuscle = new float[ConstIndexHandTypeBoth * 2];
            for (var i = 0; i < ConstIndexHandTypeBoth; i++)
            {
                _handBones[i] = animator.GetBoneTransform(HumanBodyBones.LeftHand + i);
                _defaultHandRotation[i] = _handBones[i].localRotation;
                _defaultThumbMuscle[i] = _currentHumanPose.muscles[ConstIndexThumbMuscle[(int) HandType.Left] + i];
                _defaultThumbMuscle[i + 2] = _currentHumanPose.muscles[ConstIndexThumbMuscle[(int) HandType.Right] + i];
            }
        }

        private void Update()
        {
            if (_hi5Source == null || !HI5_Manager.IsConnected)
            {
                return;
            }

            if ((handType == HandType.Left || handType == HandType.Both) &&
                HI5_Manager.GetGloveStatus().IsLeftGloveAvailable)
            {
                ApplyHandMotion_Rotation(Hand.LEFT, _hi5Source, ref _leftHi5EulerAngles);
                ApplyFingerMotion(Hand.LEFT, _hi5Source, ref _leftHi5EulerAngles);
            }

            if ((handType == HandType.Right || handType == HandType.Both) &&
                HI5_Manager.GetGloveStatus().IsRightGloveAvailable)
            {
                ApplyHandMotion_Rotation(Hand.RIGHT, _hi5Source, ref _rightHi5EulerAngles);
                ApplyFingerMotion(Hand.RIGHT, _hi5Source, ref _rightHi5EulerAngles);
            }
        }

        private void ApplyHandMotion_Rotation(Hand hand, HI5_Source hi5Source, ref Vector3[] hi5EulerAngles)
        {
            hi5EulerAngles[ConstIndexHi5Hand] =
                HI5_DataTransform.ToUnityEulerAngles(hi5Source.GetReceivedRotation(ConstIndexHi5Hand, hand));
            SetHandRotation((int) hand, ref hi5EulerAngles);
        }

        private void SetHandRotation(int handIndex, ref Vector3[] hi5EulerAngles)
        {
            _handBones[handIndex].eulerAngles = hi5EulerAngles[ConstIndexHi5Hand];

            _handBones[handIndex].localRotation =
                Quaternion.Lerp(_defaultHandRotation[handIndex], _handBones[handIndex].localRotation,
                    handRotationWeight);
        }

        private void ApplyFingerMotion(Hand hand, HI5_Source hi5Source, ref Vector3[] hi5EulerAngles)
        {
            for (var i = (ConstIndexHi5Hand + 1); i < ConstIndexNumOfHi5Bones; i++)
            {
                hi5EulerAngles[i] = HI5_DataTransform.ToUnityEulerAngles(hi5Source.GetReceivedRotation(i, hand));
            }

            SetFingerMuscle((int) hand, ref hi5EulerAngles);
        }

        private void SetFingerMuscle(int handIndex, ref Vector3[] hi5EulerAngles)
        {
            _humanPoseHandler.GetHumanPose(ref _currentHumanPose);

            //親指
            _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex]] =
                Map(hi5EulerAngles[ConstIndexHi5Hand + 1].y - 180f,
                    0f, 45f * ConstCoefficient[handIndex],
                    _defaultThumbMuscle[handIndex * 2] - 0.5f, _defaultThumbMuscle[handIndex * 2] + 0.5f);

            _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] + 1] =
                Map(hi5EulerAngles[ConstIndexHi5Hand + 1].z,
                    0f, 40.1f * ConstCoefficient[handIndex],
                    _defaultThumbMuscle[handIndex * 2 + 1] + 0.1f, _defaultThumbMuscle[handIndex * 2 + 1] - 1f);

            _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] + 2] =
                Map(hi5EulerAngles[ConstIndexHi5Hand + 2].y - 180f,
                    -55f * ConstCoefficient[handIndex], 40f * ConstCoefficient[handIndex],
                    -1f, 1f);

            _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] + 3] =
                Map(hi5EulerAngles[ConstIndexHi5Hand + 3].y - 180f,
                    -80f * ConstCoefficient[handIndex], 0f,
                    -1f, 1f);

            //人差指から小指
            for (var i = (int) Bones.HandIndex1; i < ConstIndexNumOfHi5Bones; i += 4)
            {
                _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] - 2 + i] = Map(hi5EulerAngles[i].z,
                    -10f * ConstCoefficient[handIndex], 89f * ConstCoefficient[handIndex], 1.1f, -1f);

                _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] - 1 + i] =
                    ConstSpreadMuscle[(i - (int) Bones.HandIndex1) / 4] * ConstCoefficient[handIndex];

                _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] + i] = Map(hi5EulerAngles[i + 1].z,
                    0f, 110f * ConstCoefficient[handIndex], 1f, -1f);

                _currentHumanPose.muscles[ConstIndexThumbMuscle[handIndex] + 1 + i] = Map(hi5EulerAngles[i + 2].z,
                    0f, 80f * ConstCoefficient[handIndex], 1f, -1f);
            }

            _humanPoseHandler.SetHumanPose(ref _currentHumanPose);
        }

        private float Map(float value, float bBegin, float bEnd, float aBegin, float aEnd)
        {
            return aBegin + (aEnd - aBegin) * ((value - bBegin) / (bEnd - bBegin));
        }
    }
}