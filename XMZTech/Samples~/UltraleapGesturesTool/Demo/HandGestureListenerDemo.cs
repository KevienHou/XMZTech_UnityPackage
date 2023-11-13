  
using UnityEngine;

namespace com.XMZTech.HandGesture.Demo
{  
    public class HandGestureListenerDemo : MonoBehaviour, IGestureListener
    {

        private bool isSwipLeft;
        private bool isSwipRight;
        public bool IsSwipLeft { get { if (isSwipLeft) { isSwipLeft = false; return true; } return false; } }
        public bool IsSwipRight { get { if (isSwipRight) { isSwipRight = false; return true; } return false; } }

        public void GestureInProgress(GestureEnum gesture, float progress)
        {

        }

        public bool GestureCancelled(GestureEnum gesture)
        {
            return true;
        }

        public bool GestureCompleted(GestureEnum gesture)
        {
            switch (gesture)
            {
                case GestureEnum.SwipLeft:
                    isSwipLeft = true;
                    break;
                case GestureEnum.SwipRight:
                    isSwipRight = true;
                    break;
            }
            return true;
        }


        public void GesturesInit(HandGestureManager mgr)
        {
            mgr.AddDetectedGestures(GestureEnum.SwipRight);
            mgr.AddDetectedGestures(GestureEnum.SwipLeft);
        }

    }
}