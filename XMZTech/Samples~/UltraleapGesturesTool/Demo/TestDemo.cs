using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace com.XMZTech.HandGesture.Demo
{
    public class TestDemo : MonoBehaviour
    {
        public HandGestureListenerDemo handGestureListenerDemo;

        public Text textShow;

        // Update is called once per frame
        void Update()
        {
            if (handGestureListenerDemo.IsSwipLeft)
            {
                ShowText("IsSwipLeft");
            }
            if (handGestureListenerDemo.IsSwipRight)
            {
                ShowText("IsSwipRight");
            }
            if (handGestureListenerDemo.IsSwipUp)
            {
                ShowText("IsSwipUp");
            }
            if (handGestureListenerDemo.IsSwipDown)
            {
                ShowText("IsSwipDown");
            }
            if (handGestureListenerDemo.IsPinchLeft)
            {
                ShowText("IsPinchLeft");
            }
            if (handGestureListenerDemo.IsPinchRight)
            {
                ShowText("IsPinchRight");
            }
            if (handGestureListenerDemo.IsPinchUp)
            {
                ShowText("IsPinchUp");
            }
            if (handGestureListenerDemo.IsPinchDown)
            {
                ShowText("IsPinchDown");
            }
        }

        private void ShowText(string str)
        {
            textShow.text = str;
            Invoke("Clear", 1);
        }

        private void Clear()
        {
            textShow.text = "";
        }


    }
}