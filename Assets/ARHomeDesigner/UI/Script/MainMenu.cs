using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public string camera;
        public string profile;

        // открытие профиля
        public void openProfile()
        {
            SceneManager.LoadScene(profile);
        }

        // открытие камеры
        public void openCamera()
        {
            SceneManager.LoadScene(camera);
        }
    }
}