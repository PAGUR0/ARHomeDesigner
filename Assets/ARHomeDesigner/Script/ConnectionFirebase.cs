using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Database;

namespace Connections{
    public class FirebaseConnection : MonoBehaviour
    {
        // проверка синглтона
        public void SomeMethod()
        {
            Debug.Log("Синглтон создан!");
        }
        
        // синглтон
        private static FirebaseConnection _instance;

        public static FirebaseConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(FirebaseConnection).ToString());
                    _instance = singleton.AddComponent<FirebaseConnection>();
                    DontDestroyOnLoad(singleton);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializationFirebase();
            }
        }
        
        // Данные пользователя
        private FirebaseUser _user;
        public FirebaseUser User
        {
            get { return _user; }
            private set { _user = value; }
        }
        public Dictionary<string, object> UserData { get; private set; } = new();

        // подключение к Firebase
        private FirebaseAuth _auth;
        private DatabaseReference _databaseReference;

        private void InitializationFirebase()
        {
            FirebaseApp.CheckDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    _auth = FirebaseAuth.DefaultInstance;
                    _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    Debug.Log("Firebase подключен");
                }
                else
                {
                    Debug.LogError("Нет соединения");
                }
            });
        }
        
        // логин пользователя
        public async Task<bool> SignIn(string email, string password, Action<string> callback)
        {
            try
            {
                var authResult = await _auth.SignInWithEmailAndPasswordAsync(email, password);
                User = authResult.User;

                Debug.Log($"Пользователь вошёл в аккаунт: {User.DisplayName} ({User.UserId})");
                callback?.Invoke("Вы вошли в аккаунт");
                return true;
            }
            catch (Firebase.FirebaseException ex)
            {
                Debug.LogError($"Ошибка входа: {ex.Message}");
                callback?.Invoke($"Ошибка входа: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Неизвестная ошибка: {ex.Message}");
                callback?.Invoke($"Неизвестная ошибка: {ex.Message}");
                return false;
            }
        }
        
        // Регистрация пользователя
        public async Task<bool> SignUp(string email, string username, string password, Action<string> callback)
        {
            try
            {
                var authResult = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
                FirebaseUser newUser = authResult.User;

                bool usernameSaved = await SaveUsernameToDatabase(newUser.UserId, username);
                if (usernameSaved)
                {
                    Debug.Log($"Пользователь зарегистрировался: {newUser.DisplayName} ({newUser.UserId})");
                    callback?.Invoke($"Регистрация выполнена");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Ошибка регистрации: {ex.Message}");
                callback?.Invoke($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }
        
        // Сохранение ника пользователя в базу данных
        private async Task<bool> SaveUsernameToDatabase(string userId, string username)
        {
            DatabaseReference userRef = _databaseReference.Child("users").Child(userId);
            try
            {
                await userRef.Child("username").SetValueAsync(username);
                Debug.Log("Никнейм сохранен " + username);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка сохранения никнейма: " + ex.Message);
                return false;
            }
        }
        
        // Изменение ника пользователя в базе данных
        public async Task<bool> UpdateUsernameInDatabase(string userId, string newUsername)
        {
            if (User == null)
            {
                Debug.LogError("Пользователь не вошёл в аккаунт");
                return false;
            }

            try
            {
                DatabaseReference userRef = _databaseReference.Child("users").Child(userId);

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "username", newUsername }
                };

                await userRef.UpdateChildrenAsync(updates);
                Debug.Log("Никнейм изменен: " + newUsername);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError("Ошибка изменения никнейма: " + ex);
                return false;
            }
        }

        // Смена пароля
        public async Task<bool> UpdatePasswordInDatabase(string oldPassword, string newPassword)
        {
            try
            {
                var credential = Firebase.Auth.EmailAuthProvider.GetCredential(User.Email, oldPassword);
                await _auth.CurrentUser.ReauthenticateAsync(credential);
                await _auth.CurrentUser.UpdatePasswordAsync(newPassword);
                Debug.Log("Пароль успешно изменен.");
                return true;
            }
            catch (Firebase.FirebaseException ex)
            {
                Debug.LogError($"Ошибка смены пароля: {ex.Message}");
                return false;
            }
        }

        // Загрузка данных пользователя из Realtime Database
        public async Task<bool> LoadUserDataFromRealtimeDB()
        {
            if (User == null)
            {
                Debug.LogError("Пользователь не вошел в аккаунт");
                return false;
            }

            string userId = User.UserId;
            try
            {
                DataSnapshot snapshot = await _databaseReference.Child("users").Child(userId).GetValueAsync();
                if (snapshot.Exists)
                {
                    UserData = snapshot.Value as Dictionary<string, object>;
                    Debug.Log("Данные пользователя загружены: " + JsonUtility.ToJson(UserData));
                    return true;
                }
                else
                {
                    Debug.LogError("Данные пользователя не найдены.");
                    return false;
                }
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("Ошибка загрузки данных пользователя: " + ex.Message);
                return false;
            }
        }
    }
}
