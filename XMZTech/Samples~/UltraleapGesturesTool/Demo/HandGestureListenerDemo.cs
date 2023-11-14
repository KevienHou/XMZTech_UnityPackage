
using UnityEngine;

namespace com.XMZTech.HandGesture.Demo
{
    public class HandGestureListenerDemo : MonoBehaviour, IGestureListener
    {

        private bool isSwipLeft;
        private bool isSwipRight;
        private bool isSwipUp;
        private bool isSwipDown;

        private bool isPinchLeft;
        private bool isPinchRight;
        private bool isPinchUp;
        private bool isPinchDown;



        public bool IsSwipLeft { get { if (isSwipLeft) { isSwipLeft = false; return true; } return false; } }
        public bool IsSwipRight { get { if (isSwipRight) { isSwipRight = false; return true; } return false; } }
        public bool IsSwipUp { get { if (isSwipUp) { isSwipUp = false; return true; } return false; } }
        public bool IsSwipDown { get { if (isSwipDown) { isSwipDown = false; return true; } return false; } }

        public bool IsPinchLeft { get { if (isPinchLeft) { isPinchLeft = false; return true; } return false; } }
        public bool IsPinchRight { get { if (isPinchRight) { isPinchRight = false; return true; } return false; } }
        public bool IsPinchUp { get { if (isPinchUp) { isPinchUp = false; return true; } return false; } }
        public bool IsPinchDown { get { if (isPinchDown) { isPinchDown = false; return true; } return false; } }



        public void GestureInProgress(GestureEnum gesture, float progress)
        { 
          //no need 
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
                case GestureEnum.SwipUp:
                    isSwipUp = true;
                    break;
                case GestureEnum.SwipDown:
                    isSwipDown = true;
                    break;
                case GestureEnum.PinchLeft:
                    isPinchLeft = true;
                    break;
                case GestureEnum.PinchRight:
                    isPinchRight = true;
                    break;
                case GestureEnum.PinchUp:
                    isPinchUp = true;
                    break;
                case GestureEnum.PinchDown:
                    isPinchDown = true;
                    break;
            }
            return true;
        }


        public void GesturesInit(HandGestureManager mgr)
        {
            mgr.AddDetectedGestures(GestureEnum.SwipRight);
            mgr.AddDetectedGestures(GestureEnum.SwipLeft);
            mgr.AddDetectedGestures(GestureEnum.SwipUp);
            mgr.AddDetectedGestures(GestureEnum.SwipDown);
            mgr.AddDetectedGestures(GestureEnum.PinchLeft);
            mgr.AddDetectedGestures(GestureEnum.PinchRight);
            mgr.AddDetectedGestures(GestureEnum.PinchUp);
            mgr.AddDetectedGestures(GestureEnum.PinchDown);

        }

    }
}