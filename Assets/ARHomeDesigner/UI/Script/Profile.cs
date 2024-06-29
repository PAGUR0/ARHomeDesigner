using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Connections;
using TMPro;
using UnityEngine.SceneManagement;

namespace UI
{
    public class Profile : MonoBehaviour
    {
        public TMP_InputField nameInputField;
        public TMP_InputField emailInputField;

        public GameObject profilePanel;
        public GameObject newPasswordPanel;

        public TMP_InputField oldPasswordInputField;
        public TMP_InputField newPasswordInputField;
        public TMP_InputField newPasswordDupInputField;
        
        public GameObject userMessage;
        public TMP_Text textMessage;

        public string mainMenu;

        async void Start()
        {
            ShowProfile();
            HideMessage();

            if (await FirebaseConnection.Instance.LoadUserDataFromRealtimeDB())
            {
                InsertData();
            }
        }

        private void InsertData()
        {
            nameInputField.text = FirebaseConnection.Instance.UserData["username"].ToString();
            emailInputField.text = FirebaseConnection.Instance.User.Email;
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

        // смена ника
        public async void newName()
        {
            if (nameInputField.interactable)
            {
                nameInputField.interactable = false;
                if (nameInputField.text != FirebaseConnection.Instance.UserData["username"].ToString())
                {
                    if (await FirebaseConnection.Instance.UpdateUsernameInDatabase(
                            FirebaseConnection.Instance.User.UserId, nameInputField.text))
                    {
                        Debug.Log("Имя изменено");
                    }
                    else
                    {
                        Debug.Log("Ошибка смены имени");
                        Message("Ошибка смены имени");
                        nameInputField.text = FirebaseConnection.Instance.UserData["username"].ToString();
                    }
                }
            }
            else
            {
                nameInputField.interactable = true;
            }
        }

        // смена пароля
        public async void newPassword()
        {
            if (newPasswordInputField.text != newPasswordDupInputField.text)
            {
                Debug.LogError("Пароли не совпадают");
                Message("Пароли не совпадают");
                return;
            }
            
            if (await FirebaseConnection.Instance.UpdatePasswordInDatabase(oldPasswordInputField.text,
                    newPasswordInputField.text))
            {
                Debug.Log("Пароль изменен");
                Message("Пароль изменен");
                ShowProfile();
            }
            else
            {
                Debug.Log("Ошибка смены пароля");
                Message("Ошибка смены пароля");
                emailInputField.text = FirebaseConnection.Instance.User.Email;
            }
        }

        // открыть профиль
        public void ShowProfile()
        {
            Debug.Log("Открыто окно профиль");
            profilePanel.SetActive(true);
            newPasswordPanel.SetActive(false);
        }

        // открыть создание нового пароля
        public void ShowNewPassword()
        {
            Debug.Log("Открыто окно смены пароля");
            profilePanel.SetActive(false);
            newPasswordPanel.SetActive(true);
        }

        // открыть главное меню
        public void openMainMenu()
        {
            SceneManager.LoadScene(mainMenu);
            Debug.Log("Открыто главное меню");
        }
    }
}

