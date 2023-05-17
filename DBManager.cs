using System;
using Firebase.Database;
using Firebase;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DBManager : MonoBehaviour
{
    string userId;
    DatabaseReference reference;
    String connection = "https://bbdd-tfg-usuarios-default-rtdb.europe-west1.firebasedatabase.app/";
    FirebaseApp app;
    private String userID;
    public TextMeshProUGUI lblUsers;
    public TextMeshProUGUI lblScores;
    public TextMeshProUGUI lblAlerta;
    public TMP_InputField txtUsername;
    public TMP_InputField txtPassword;
    public TMP_InputField txtPassword2;

    void Start()
    {
        AppOptions appOptions = new AppOptions();
        appOptions.DatabaseUrl = new Uri(connection);
        app = FirebaseApp.Create(appOptions, "bbdd-tfg-usuarios");
        reference = FirebaseDatabase.GetInstance(app, connection).RootReference;
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "ScoreTable")
        {
            GetAllUserInfo();
        }
    }

    public void createUser(String name, string password, int score)
    {
        User usuario = new User(name, password, score);
        String json = JsonUtility.ToJson(usuario);

        reference.Child("users").Child("new_user_" + name).SetRawJsonValueAsync(json);
        Debug.Log("usuario creado");
    }

    public IEnumerator comprobarUsuario(string username, Action<bool> onCallBack)
    {
        var strReference = reference.Child("users").OrderByChild("name").EqualTo(username);
        var strTask = strReference.GetValueAsync();
        yield return new WaitUntil(() => strTask.IsCompleted);

        if (strTask.Exception != null)
        {
            Debug.LogError($"Failed to retrieve user from Firebase: {strTask.Exception}\n From user {username}");
            onCallBack.Invoke(false);
            yield break;
        }

        var strSnapshot = strTask.Result;
        onCallBack.Invoke(strSnapshot.HasChildren);
    }

    public IEnumerator comprobarContraseña(string password, string username, Action<bool> onCallBack)
    {
        var userReference = reference.Child("users").Child("new_user_" + username);
        var userTask = userReference.GetValueAsync();
        yield return new WaitUntil(() => userTask.IsCompleted);

        if (userTask.Exception != null)
        {
            Debug.LogError($"Failed to retrieve user from Firebase: {userTask.Exception}\n From user {username}");
            onCallBack.Invoke(false);
            yield break;
        }

        var userSnapshot = userTask.Result;

        if (!userSnapshot.Exists)
        {
            Debug.LogError("El usuario no existe en la base de datos");
            onCallBack.Invoke(false);
            yield break;
        }
        else if (!userSnapshot.HasChild("password"))
        {
            Debug.LogError("La contraseña no existe en la base de datos");
            onCallBack.Invoke(false);
            yield break;
        }
        else if (string.IsNullOrEmpty(userSnapshot.Child("password").Value.ToString()))
        {
            Debug.LogError("El valor de la contraseña es nulo o vacío");
            onCallBack.Invoke(false);
            yield break;
        }

        else if (userSnapshot.Child("password").Value == null)
        {
            Debug.LogError("El valor de la contraseña es null");
            onCallBack.Invoke(false);
            yield break;
        }

        else if (userSnapshot.Child("password").Value.ToString().Equals(password))
        {
            Debug.Log("La contraseña es correcta");
            onCallBack.Invoke(true);
        }
    }


    public IEnumerator GetAllUsers(Action<List<User>> onCallBack)
    {
        var users = reference.Child("users").GetValueAsync();
        yield return new WaitUntil(predicate: () => users.IsCompleted);

        if (users != null)
        {
            DataSnapshot snapshot = users.Result;

            List<User> userList = new List<User>();
            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                string json = userSnapshot.GetRawJsonValue();
                User user = JsonUtility.FromJson<User>(json);
                userList.Add(user);
            }

            onCallBack.Invoke(userList);
        }
    }

    public void GetAllUserInfo()
    {
        StartCoroutine(GetAllUsers((List<User> users) =>
        {
            string allUsers = "";
            string allScores = "";

            foreach (User user in users)
            {
                allUsers += user.name + "\n";
                allScores += user.score.ToString() + "\n";
            }

            lblUsers.text = allUsers;
            lblScores.text = allScores;
        }));
    }

    public void Login()
    {
        StartCoroutine(comprobarCampos());
    }

    public void Register()
    {
        if (txtUsername != null && txtPassword != null && txtPassword2 != null)
        {
            if (!txtUsername.text.Equals("") && !txtPassword.text.Equals("") && !txtPassword2.text.Equals(""))
            {
                if (txtPassword.text.Equals(txtPassword2.text))
                {
                    StartCoroutine(comprobarUsuario(txtUsername.text, (userExists) =>
                    {
                        if (!userExists)
                        {
                            createUser(txtUsername.text, txtPassword.text, 0);
                            lblAlerta.color = Color.green;
                            lblAlerta.color = new Color(0, 255, 0);

                            lblAlerta.ForceMeshUpdate();
                            lblAlerta.text = "Usuario Creado!";
                            StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = "Cargando LogIn "; }));
                            StartCoroutine(Esperar(2.5f, () => { cargarLogin(); }));
                        }
                        else
                        {
                            lblAlerta.color = Color.red;
                            lblAlerta.color = new Color(255, 0, 0);
                            lblAlerta.ForceMeshUpdate();
                            lblAlerta.text = "Ese usuario ya existe";
                            txtUsername.text = "";
                        }
                    }));
                }
                else
                {
                    lblAlerta.color = Color.red;
                    lblAlerta.color = new Color(255, 0, 0);

                    lblAlerta.ForceMeshUpdate();
                    lblAlerta.text = "Las contraseñas no cuadran, escríbalas de nuevo";
                    txtPassword.text = "";
                    txtPassword2.text = "";
                }
            }
            else
            {   lblAlerta.color = Color.red;
                lblAlerta.color = new Color(255, 0, 0);

                lblAlerta.ForceMeshUpdate();
                lblAlerta.text = "Inserte datos";
            }
        }
        else
        {   lblAlerta.color = Color.red;
            lblAlerta.color = new Color(255, 0, 0);

            lblAlerta.ForceMeshUpdate();
            lblAlerta.text = "Inserte datos";
        }

        StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = ""; }));
    }

    private IEnumerator comprobarCampos()
    {
        bool approved = false;
        if (txtUsername != null && txtPassword != null)
        {
            if (!txtUsername.text.Equals("") && !txtPassword.text.Equals(""))
            {
                yield return StartCoroutine(comprobarUsuario(txtUsername.text, (userExists) =>
                {
                    if (userExists)
                    {
                        lblAlerta.color = Color.green;
                        lblAlerta.color = new Color(0, 255, 0);
                        StartCoroutine(comprobarContraseña(txtPassword.text, txtUsername.text, (matches) =>
                        {
                            if (matches)
                            {
                                approved = true;
                                lblAlerta.text = "Login correcto,\nCargando juego";
                            }
                            else
                            {
                                lblAlerta.color = Color.red;
                                lblAlerta.color = new Color(255, 0, 0);

                                lblAlerta.ForceMeshUpdate();
                                lblAlerta.text = "Contraseña incorrecta";
                                txtPassword.text = "";
                            }
                        }));
                    }
                    else
                    {
                        lblAlerta.color = Color.red;
                        lblAlerta.color = new Color(255, 0, 0);

                        lblAlerta.ForceMeshUpdate();
                        lblAlerta.text = "Usuario no encontrado";
                        txtUsername.text = "";
                    }
                }));
            }
            else
            {
                lblAlerta.color = Color.red;
                lblAlerta.color = new Color(255, 0, 0);

                lblAlerta.ForceMeshUpdate();
                lblAlerta.text = "Inserte datos";
            }
        }
        else
        {
            lblAlerta.color = Color.red;
            lblAlerta.color = new Color(255, 0, 0);
            lblAlerta.ForceMeshUpdate();
            lblAlerta.text = "Inserte datos";
        }

        StartCoroutine(Esperar(1, () => ProcesarAprobacion(approved)));
    }

    private void ProcesarAprobacion(bool approved)
    {
        // Continuar con la lógica de la aplicación
        if (approved)
        {
            // Hacer algo si el usuario ha sido aprobado
            PlayerPrefs.SetString("username", txtUsername.text);
            StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = "Login correcto,\nCargando Juego "; }));
            StartCoroutine(Esperar(2.5f, () => { cargarJuego(); }));
        }
        else
        {
            Debug.Log("Algo no va bien");
            lblAlerta.color = Color.red;
            lblAlerta.color = new Color(255, 0, 0);

            lblAlerta.ForceMeshUpdate();
            lblAlerta.text = "Contraseña incorrecta";
            txtPassword.text = "";
        }

        StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = ""; }));
    }


    public void loadGame(int escena)
    {
        SceneManager.LoadScene(escena);
    }

    public void cargarInicio()
    {
        loadGame(0);
    }

    public void cargarScoreTables()
    {
        loadGame(4);
    }

    public void cargarJuego()
    {
        loadGame(1);
    }

    public void cargarLogin()
    {
        loadGame(2);
    }

    public void cargarRegistro()
    {
        loadGame(3);
    }

    public static IEnumerator Esperar(float tiempo, Action onComplete)
    {
        yield return new WaitForSeconds(tiempo);
        onComplete();
    }
}