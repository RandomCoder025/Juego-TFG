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
    #region variables
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
    #endregion 

    void Start()
    {
        // Inicializa la conexión a la BBDD
        AppOptions appOptions = new AppOptions();
        appOptions.DatabaseUrl = new Uri(connection);
        app = FirebaseApp.Create(appOptions, "bbdd-tfg-usuarios");
        reference = FirebaseDatabase.GetInstance(app, connection).RootReference;
        Scene currentScene = SceneManager.GetActiveScene();

        // Carga la información de los usuarios si se encuentra en la pantalla de la tabla de puntuación
        if (currentScene.name == "ScoreTable")
        {
            GetAllUserInfo();
        }
    }

    // Crea un nuevo usuario obteniendo sus datos por parámetro
    public void createUser(String name, string password, int score)
    {
        User usuario = new User(name, password, score);
        String json = JsonUtility.ToJson(usuario);

        reference.Child("users").Child("new_user_" + name).SetRawJsonValueAsync(json);
        Debug.Log("usuario creado");
    }

    // Comprueba si el usuario existe en la BBDD
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

    // Comprueba si la contraseña del usuario de la BBDD es la misma que la pasada por parámetro

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

    // Obntiene todos los usuarios de la BBDD y los guarda en una lista
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
             userList.Sort();
            userList.Reverse();
            onCallBack.Invoke(userList);
        }
    }

    // Muestra la información de todos los usuarios en la tabla de puntuación
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

    #region Log-In
    // Método que acciona el botón del Login
    public void Login()
    {
        StartCoroutine(comprobarCampos());
    }

   
    // Corrutina para verificar los campos de inicio de sesión. Muestra alertas para anunciar al usuario el resultado
    private IEnumerator comprobarCampos()
    {
        bool approved = false;
        if (txtUsername != null && txtPassword != null)
        {
            if (!txtUsername.text.Equals("") && !txtPassword.text.Equals(""))
            {
                // Lanza la corrutina pasando la información de los campos por parámetro 
                yield return StartCoroutine(comprobarUsuario(txtUsername.text, (userExists) =>
                {
                    if (userExists)
                    {
                        lblAlerta.color = Color.green;
                        lblAlerta.color = new Color(0, 255, 0);
                        // Lanza la corrutina pasando la información de los campos por parámetro 
                        StartCoroutine(comprobarContraseña(txtPassword.text, txtUsername.text, (matches) =>
                        {
                            if (matches)
                            {
                                approved = true;
                                lblAlerta.text = "Inicio de sesión correcto,\nCargando juego";
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

    // Procesa la aprobación de inicio de sesión
    private void ProcesarAprobacion(bool approved)
    {
        if (approved)
        {
            // El login es correcto, carga el juego
            PlayerPrefs.SetString("username", txtUsername.text);
            StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = "Inicio de sesión correcto,\nCargando Juego "; }));
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
 #endregion
    // Método para registrar un nuevo usuario
    public void Register()
    {
        if (txtUsername != null && txtPassword != null && txtPassword2 != null)
        {
            if (!txtUsername.text.Equals("") && !txtPassword.text.Equals("") && !txtPassword2.text.Equals(""))
            {
                if (txtPassword.text.Equals(txtPassword2.text))
                {
                    // Si no existe el usuario lo crea, si no, avisa al jugador y resetea los campos
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
                // Si los campos no son correctos, muestra alertas al usuario y resetea los campos
                else
                {
                    lblAlerta.color = Color.red;
                    lblAlerta.color = new Color(255, 0, 0);

                    lblAlerta.ForceMeshUpdate();
                    lblAlerta.text = "Las contraseñas no coinciden, escríbalas de nuevo";
                    txtPassword.text = "";
                    txtPassword2.text = "";
                }
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

        StartCoroutine(Esperar(2.5f, () => { lblAlerta.text = ""; }));
    }

    // Método para cargar una escena en base al número de la escena, pasado por parámetro
    public void loadGame(int escena)
    {
        SceneManager.LoadScene(escena);
    }

    // Métodos para cargar escenas específicas
    public void cargarInicio()
    {
        loadGame(0);
    }

    public void cargarLogin()
    {
        loadGame(1);
    }

    public void cargarJuego()
    {
        loadGame(2);
    }

    // Corrutina para esperar un cierto tiempo antes de realizar una acción
    private IEnumerator Esperar(float tiempo, Action accion)
    {
        yield return new WaitForSeconds(tiempo);
        accion();
    }
}
