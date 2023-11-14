using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

namespace com.XMZTech.HandGesture.Demo
{
    public class TestDemo : MonoBehaviour
    {
        public HandGestureListenerDemo handGestureListenerDemo;

        public Text textShow;
        public float count = 1;

        public Transform cube;
        public bool isRot = false;
        public bool isMove = false;




        void Update()
        {
            if (handGestureListenerDemo.IsSwipLeft)
            {
                ShowText("IsSwipLeft");
                Rot(0);
            }
            if (handGestureListenerDemo.IsSwipRight)
            {
                ShowText("IsSwipRight");
                Rot(1);
            }
            if (handGestureListenerDemo.IsSwipUp)
            {
                ShowText("IsSwipUp");
                Rot(2);
            }
            if (handGestureListenerDemo.IsSwipDown)
            {
                ShowText("IsSwipDown");
                Rot(3);
            }
            if (handGestureListenerDemo.IsPinchLeft)
            {
                ShowText("IsPinchLeft");
                Move(0);
            }
            if (handGestureListenerDemo.IsPinchRight)
            {
                ShowText("IsPinchRight");
                Move(1);
            }
            if (handGestureListenerDemo.IsPinchUp)
            {
                ShowText("IsPinchUp");
                Move(2);
            }
            if (handGestureListenerDemo.IsPinchDown)
            {
                ShowText("IsPinchDown");
                Move(3);
            }

            if (count >= 0)
            {
                count -= Time.deltaTime;
            }
            else
            {
                Clear();
            }

            if (isRot)
            {
                if (step > 0)
                {
                    step--;

                    cube.Rotate(axis * spinSpeed, Space.World);
                }
                else
                {
                    isRot = false;
                }
            }

            if (isMove)
            {
                if (step > 0)
                {
                    step--;

                    cube.Translate(axis * 5 * Time.deltaTime, Space.World);
                }
                else
                {
                    isMove = false;
                }
            }
        }

        private void Move(int flag)
        {
            if (isMove)
            {
                return;
            }
            isMove = true;
            step = 10;
            switch (flag)
            {
                case 0: //左
                    axis = Vector3.left;
                    break;
                case 1: //右
                    axis = Vector3.right;
                    break;
                case 2: //上 
                    axis = Vector3.up;
                    break;
                case 3: //下 
                    axis = Vector3.down;
                    break;
            }
        }

        private void Rot(int flag)
        {
            if (isRot)
            {
                return;
            }
            isRot = true;
            step = 90 / spinSpeed;
            switch (flag)
            {
                case 0: //左
                    axis = Vector3.up;
                    rotFlag = spinSpeed;
                    break;
                case 1: //右
                    axis = Vector3.down;
                    rotFlag = -spinSpeed;
                    break;
                case 2: //上 
                    axis = Vector3.right;
                    rotFlag = spinSpeed;
                    break;
                case 3: //下 
                    axis = Vector3.left;
                    rotFlag = -spinSpeed;
                    break;
            }
        }

        public int spinSpeed = 5;
        public int step = 0;
        public int rotFlag = 0;

        Vector3 axis = Vector3.zero;

        private void ShowText(string str)
        {
            textShow.text = str;
            count = 1;
        }



        private void Clear()
        {
            textShow.text = "";
            count = 1000000;
        }


    }
}