using Leap;
using Leap.Unity;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace com.XMZTech.HandGesture
{

    public enum GestureEnum
    {
        None,
        SwipLeft,
        SwipRight,
        SwipUp,
        SwipDown,
        PinchLeft,
        PinchRight,
        PinchUp,
        PinchDown,
    }

    [Serializable]
    public struct GestureData
    {
        public int hand;
        public Vector3 handPos;
        public GestureEnum gesture;
        public int state;
        public float timeStamp;
        public bool complete;
        public bool cancelled;
        public float progress;
        public float startTrackingAtTime;
    }

    public interface IGestureListener
    {
        void GesturesInit(HandGestureManager mgr);
        void GestureInProgress(GestureEnum gesture, float progress);
        bool GestureCompleted(GestureEnum gesture);
        bool GestureCancelled(GestureEnum gesture);

    }



    public class HandGestureManager : MonoBehaviour
    {
        public LeapServiceProvider provider;
        public List<GestureData> playerGestures = new List<GestureData>();
        //public List<IGestureListener> allListeners = new List<IGestureListener>(); //Ŀǰ��֧�� �� listener ���� ��TODO:������չ�� Listener ����
        private IGestureListener currListener; //Ŀǰ��֧�� �� listener ���� ��TODO:������չ�� Listener ����
        [SerializeField] bool[] handsTracked;
        [SerializeField] Hand[] hands;
        [SerializeField] int leftHandIndex;
        [SerializeField] int rightHandIndex;
        [SerializeField] Vector3[] handPos;

        [SerializeField] float MinTimeBetweenSameGestures = 0.5f;

        private void Awake()
        {
            ManagerInit();
        }

        public void ManagerInit()
        {

            var interfaces = FindObjectsOfType<MonoBehaviour>().OfType<IGestureListener>();
            foreach (var item in interfaces)
            {
                item.GesturesInit(this);
                currListener = item;
                break;
            }
            if (currListener == null)
            {
                return;
            }




            provider = FindObjectOfType<LeapServiceProvider>();
            if (provider == null)
            {
                provider = new GameObject("LeapMotionProvider").AddComponent<LeapServiceProvider>();
                provider.editTimePose = TestHandFactory.TestHandPose.DesktopModeA;
            }

            provider.OnUpdateFrame += OnUpdateFrame;
            handsTracked = new bool[2];
            hands = new Hand[2];
            handPos = new Vector3[2];
            leftHandIndex = 0;
            rightHandIndex = 1;
        }

        /// <summary>
        /// �����Ҫ������ ����
        /// </summary>
        /// <param name="gestureEnum"></param>
        public void AddDetectedGestures(GestureEnum gestureEnum)
        {
            var ges = new GestureData();
            ges.gesture = gestureEnum;
            playerGestures.Add(ges);
        }

        /// <summary>
        /// ���� ���ƵĻ�����Ϣ
        /// </summary>
        /// <param name="gestureData"></param>
        /// <param name="timeStamp"></param>
        /// <param name="joint"></param>
        /// <param name="handPos"></param>
        private void SetGestureJoint(ref GestureData gestureData, float timeStamp, int joint, Vector3 handPos)
        {
            gestureData.hand = joint;
            gestureData.handPos = handPos;
            gestureData.timeStamp = timeStamp;
            gestureData.state++;
        }

        /// <summary>
        /// ���ƶ������
        /// </summary>
        /// <param name="gestureData"></param>
        /// <param name="timeStamp"></param>
        /// <param name="isInPose"></param>
        private void CheckGestureComplete(ref GestureData gestureData, float timeStamp, bool isInPose)
        {
            if (isInPose)
            {
                gestureData.timeStamp = timeStamp;
                gestureData.complete = true;
                gestureData.state++;
            }
        }

        /// <summary>
        /// ��ʱ�̵Ķ��� ȡ�� ��������
        /// </summary>
        /// <param name="gestureData"></param>
        private void SetGestureCancelled(ref GestureData gestureData)
        {
            gestureData.state = 0;
            gestureData.progress = 0f;
            gestureData.cancelled = true;
        }

        /// <summary>
        /// �������ж���  TODO����֧�ֶ��Listener ��ʱ��Ҫ���ݲ������ö�Ӧ�� listener
        /// </summary>
        private void ResetListenerGestures()
        {
            if (playerGestures.Count > 0)
            {
                int count = playerGestures.Count;
                for (int i = 0; i < count; i++)
                {
                    ResetGesture(playerGestures[i].gesture);
                }
            }
        }

        /// <summary>
        /// ����Gesture 
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        private bool ResetGesture(GestureEnum gesture)
        {
            var gesturesData = playerGestures;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;
            if (index < 0)
            {
                return false;
            }
            GestureData gestureData = gesturesData[index];
            gestureData.hand = -1;
            gestureData.state = 0;
            gestureData.progress = 0;
            gestureData.cancelled = false;
            gestureData.complete = false;
            gestureData.timeStamp = 0;
            gestureData.handPos = Vector3.zero;
            gestureData.startTrackingAtTime = Time.realtimeSinceStartup + MinTimeBetweenSameGestures;
            gesturesData[index] = gestureData;
            return true;
        }

        /// <summary>
        /// �ܾ�Index ��ȡ ��Ӧ�� gesturedata
        /// </summary>
        /// <param name="gesture"></param>
        /// <param name="gesturesData"></param>
        /// <returns></returns>
        private int GetGestureIndex(GestureEnum gesture, ref List<GestureData> gesturesData)
        {
            int listSize = gesturesData.Count;
            for (int i = 0; i < listSize; i++)
            {
                if (gesturesData[i].gesture == gesture)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// ʵʱ���µĻص�
        /// </summary>
        /// <param name="obj"></param>
        private void OnUpdateFrame(Frame obj)
        {
            CheckHandInTracked(obj);
            CheckForGestures();

        }

        /// <summary>
        /// �����ֵ�׷��״̬
        /// </summary>
        /// <param name="obj"></param>
        private void CheckHandInTracked(Frame obj)
        {
            DetectHand(leftHandIndex, obj.GetHand(Chirality.Left));
            DetectHand(rightHandIndex, obj.GetHand(Chirality.Right));
        }

        /// <summary>
        /// �����±� ���ͣ�����ֵ�״̬
        /// </summary>
        /// <param name="handIndex"></param>
        /// <param name="hand"></param>
        private void DetectHand(int handIndex, Hand hand)
        {
            hands[handIndex] = hand;
            handsTracked[handIndex] = hands[handIndex] != null;
            if (handsTracked[handIndex])
            {
                handPos[handIndex] = hands[handIndex].PalmPosition;
            }
            else
            {
                handPos[handIndex] = Vector3.zero;
            }
        }

        /// <summary>
        /// ������е�����
        /// </summary>
        private void CheckForGestures()
        {
            int listGestureSize = playerGestures.Count;
            float timestampNow = Time.realtimeSinceStartup;
            for (int i = 0; i < listGestureSize; i++)
            {
                GestureData gestureData = playerGestures[i];

                if (timestampNow >= gestureData.startTrackingAtTime)
                {
                    CheckForGesture(ref gestureData, Time.realtimeSinceStartup);
                }
                playerGestures[i] = gestureData;
                if (gestureData.complete)
                {
                    //foreach (IGestureListener listener in allListeners)
                    //{
                    //    if (listener != null && listener.GestureCompleted(gestureData.gesture))
                    //    {
                    //        ResetListenerGestures();
                    //    }
                    //}
                    currListener.GestureCompleted(gestureData.gesture);
                    ResetListenerGestures();
                }
                else if (gestureData.cancelled)
                {
                    //foreach (IGestureListener listener in allListeners)
                    //{
                    //    if (listener != null && listener.GestureCancelled(gestureData.gesture))
                    //    {
                    //        ResetGesture(gestureData.gesture);
                    //    }
                    //}

                    currListener.GestureCancelled(gestureData.gesture);
                    ResetGesture(gestureData.gesture);

                }
                else if (gestureData.progress >= 0.1f)
                {
                    //foreach (IGestureListener listener in allListeners)
                    //{
                    //    if (listener != null)
                    //    {
                    //        listener.GestureInProgress(gestureData.gesture, gestureData.progress);
                    //    }
                    //}
                    currListener.GestureInProgress(gestureData.gesture, gestureData.progress);
                }
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="gestureData"></param>
        /// <param name="timeStamp"></param>
        private void CheckForGesture(ref GestureData gestureData, float timeStamp)
        {
            if (gestureData.complete)
            {
                return;
            }
            switch (gestureData.gesture)
            {
                case GestureEnum.None:
                    break;
                case GestureEnum.SwipLeft:

                    switch (gestureData.state)
                    {
                        case 0:
                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:

                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] && //�ֱ�׷��
                                    handPos[gestureData.hand].x < gestureData.handPos.x &&
                                    (gestureData.handPos.x - handPos[gestureData.hand].x) > 0.1f;//�����ֵ�λ����֮ǰ�ֵ���10cm��  
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }

                    break;
                case GestureEnum.SwipRight:

                    switch (gestureData.state)
                    {
                        case 0:
                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:

                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] && //�ֱ�׷��
                                     gestureData.handPos.x < handPos[gestureData.hand].x &&
                                    (handPos[gestureData.hand].x - gestureData.handPos.x) > 0.1f;//�����ֵ�λ����֮ǰ�ֵ���10cm��  
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;
                case GestureEnum.SwipUp:

                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                     (handPos[gestureData.hand].y > gestureData.handPos.y) &&
                                   (handPos[gestureData.hand].y - gestureData.handPos.y) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }


                    break;
                case GestureEnum.SwipDown:

                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                     (gestureData.handPos.y > handPos[gestureData.hand].y) &&
                                   (gestureData.handPos.y - handPos[gestureData.hand].y) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }

                    break;
                case GestureEnum.PinchLeft:

                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }
                            break;

                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                    hands[gestureData.hand].PinchStrength > 0.85f &&
                                     (handPos[gestureData.hand].x < gestureData.handPos.x) &&
                                   (gestureData.handPos.x - handPos[gestureData.hand].x) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }



                    break;
                case GestureEnum.PinchRight:
                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }
                            break;

                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                    hands[gestureData.hand].PinchStrength > 0.85f &&
                                     (gestureData.handPos.x < handPos[gestureData.hand].x) &&
                                   (handPos[gestureData.hand].x - gestureData.handPos.x) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }

                    break;
                case GestureEnum.PinchUp:

                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                       hands[gestureData.hand].PinchStrength > 0.85f &&
                                     (handPos[gestureData.hand].y > gestureData.handPos.y) &&
                                   (handPos[gestureData.hand].y - gestureData.handPos.y) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }

                    break;
                case GestureEnum.PinchDown:

                    switch (gestureData.state)
                    {
                        case 0:

                            //���ֱ�׷��                               //�����ſ�
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //���ֱ�׷��                               //�����ſ�
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:
                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] &&
                                       hands[gestureData.hand].PinchStrength > 0.85f &&
                                     (gestureData.handPos.y > handPos[gestureData.hand].y) &&
                                   (gestureData.handPos.y - handPos[gestureData.hand].y) > 0.1f;
                                CheckGestureComplete(ref gestureData, timeStamp, isInPose);
                            }
                            else
                            {
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }



                    break;
            }
        }
    }
}