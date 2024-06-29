using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Connections;
using TMPro;
using UnityEngine.SceneManagement;

namespace UI
{
    public class Authentication : MonoBehaviour
    {
        public GameObject userMessage;
        public TMP_Text textMessage;
        
        public GameObject signInPanel;
        public GameObject signUpPanel;
        
        public TMP_InputField emailSignInInputField;
        public TMP_InputField passwordSignInInputField;

        public TMP_InputField emailSignUpInputField;
        public TMP_InputField nameSignUpInputField;
        public TMP_InputField passwordSignUpInputField;
        public TMP_InputField passwordDupSignUpInputField;
        
        public string mainMenu;
        
        // соединение с Firebase
        void Start()
        {
            HideMessage();
            FirebaseConnection.Instance.SomeMethod();
            ShowSignIn();
        }
        
        // отображение сообщения пользователю
        private void Message(string message)
        {
            textMessage.text = message;
            userMessage.SetActive(true);
            Invoke("HideMessage", 1f);
        }
        
        private void HideMessage()
        {
            userMessage.SetActive(false);
        }

        // логин пользователя
        public async void SignIn()
        {
            if (string.IsNullOrEmpty(emailSignInInputField.text) || string.IsNullOrEmpty(passwordSignInInputField.text))
            {
                Debug.LogError("Email пустой");
                Message("Email пустой");
                return;
            }
            
            if(await FirebaseConnection.Instance.SignIn(emailSignInInputField.text, passwordSignInInputField.text, message =>
               {
                   Message(message);
               }))
            {
                SceneManager.LoadScene(mainMenu);
            }
        }
        
        // показ регистрации
        public void ShowSignUp()
        {
            signUpPanel.SetActive(true);
            signInPanel.SetActive(false);
            Debug.Log("Переключено на регистрацию");
        }
        
        // регистрация пользователя
        public async void SignUp()
        {
            if (emailSignUpInputField == null || nameSignUpInputField == null || passwordSignUpInputField == null || passwordDupSignUpInputField == null)
            {
                Debug.LogError("Введены пустые поля");
                Message("Введены пустые поля");
                return;
            }

            if (passwordSignUpInputField.text != passwordDupSignUpInputField.text)
            {
                Debug.LogError("Пароли не совпадают");
                Message("Пароли не совпадают");
                return;
            }
            
            if(await FirebaseConnection.Instance.SignUp(emailSignUpInputField.text, nameSignUpInputField.text, passwordSignUpInputField.text, message =>
               {
                   Message(message);
               }))
            {
                ShowSignIn();
            }
        }
        
        // показ логина
        public void ShowSignIn()
        {
            signUpPanel.SetActive(false);
            signInPanel.SetActive(true);
            Debug.Log("Переключено на регистрацию");
        }
    }
}

