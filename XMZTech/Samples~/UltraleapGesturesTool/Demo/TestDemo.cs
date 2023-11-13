using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace com.XMZTech.HandGesture.Demo
{  
    public class TestDemo : MonoBehaviour
    {
        public HandGestureListenerDemo handGestureListenerDemo;
         
        // Update is called once per frame
        void Update()
        {
            if (handGestureListenerDemo.IsSwipLeft)
            {
                Debug.Log("IsSwipLeft");
            }
            if (handGestureListenerDemo.IsSwipRight)
            {
                Debug.Log("IsSwipRight");
            }

        }
    }
}