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
        //public List<IGestureListener> allListeners = new List<IGestureListener>(); //目前不支持 多 listener 监听 。TODO:后期扩展多 Listener 监听
        private IGestureListener currListener; //目前不支持 多 listener 监听 。TODO:后期扩展多 Listener 监听
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
        /// 添加需要被检测的 手势
        /// </summary>
        /// <param name="gestureEnum"></param>
        public void AddDetectedGestures(GestureEnum gestureEnum)
        {
            var ges = new GestureData();
            ges.gesture = gestureEnum;
            playerGestures.Add(ges);
        }

        /// <summary>
        /// 设置 手势的基本信息
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
        /// 手势动作完成
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
        /// 长时程的动作 取消 才有意义
        /// </summary>
        /// <param name="gestureData"></param>
        private void SetGestureCancelled(ref GestureData gestureData)
        {
            gestureData.state = 0;
            gestureData.progress = 0f;
            gestureData.cancelled = true;
        }

        /// <summary>
        /// 重置所有动作  TODO：在支持多个Listener 的时候，要根据参数重置对应的 listener
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
        /// 重置Gesture 
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
        /// 很具Index 获取 对应的 gesturedata
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
        /// 实时更新的回调
        /// </summary>
        /// <param name="obj"></param>
        private void OnUpdateFrame(Frame obj)
        {
            CheckHandInTracked(obj);
            CheckForGestures();

        }

        /// <summary>
        /// 更新手的追踪状态
        /// </summary>
        /// <param name="obj"></param>
        private void CheckHandInTracked(Frame obj)
        {
            DetectHand(leftHandIndex, obj.GetHand(Chirality.Left));
            DetectHand(rightHandIndex, obj.GetHand(Chirality.Right));
        }

        /// <summary>
        /// 依据下标 类型，检测手的状态
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
        /// 检测所有的手势
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
        /// 检测手势
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
                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:

                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] && //手被追踪
                                    handPos[gestureData.hand].x < gestureData.handPos.x &&
                                    (gestureData.handPos.x - handPos[gestureData.hand].x) > 0.1f;//现在手的位置在之前手的左方10cm外  
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
                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
                            else if (handsTracked[rightHandIndex] && hands[rightHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, rightHandIndex, handPos[rightHandIndex]);
                            }

                            break;
                        case 1:

                            if ((timeStamp - gestureData.timeStamp) <= 0.5f)
                            {
                                bool isInPose = handsTracked[gestureData.hand] && //手被追踪
                                     gestureData.handPos.x < handPos[gestureData.hand].x &&
                                    (handPos[gestureData.hand].x - gestureData.handPos.x) > 0.1f;//现在手的位置在之前手的左方10cm外  
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].GetFistStrength() <= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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

                            //左手被追踪                               //左手张开
                            if (handsTracked[leftHandIndex] && hands[leftHandIndex].PinchStrength >= 0.85f)
                            {
                                SetGestureJoint(ref gestureData, timeStamp, leftHandIndex, handPos[leftHandIndex]);
                            }           //右手被追踪                               //右手张开
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