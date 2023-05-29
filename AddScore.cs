using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddScore : MonoBehaviour
{
    #region variables
    private int totalScore = 0;
    private bool completado = false;
    private bool sonando = false;

    public TextMeshProUGUI scoreTMP;
    public AudioSource moneda;
    public AudioSource meta;

    DatabaseReference reference;
    String connection = "https://bbdd-tfg-usuarios-default-rtdb.europe-west1.firebasedatabase.app/";
    FirebaseApp app;
    #endregion
    // Inicializa la conexión a la BBDD
    void Start()
    {
        AppOptions appOptions = new AppOptions();
        appOptions.DatabaseUrl = new Uri(connection);
        app = FirebaseApp.Create(appOptions, "bbdd-tfg-usuarios");
        reference = FirebaseDatabase.GetInstance(app, connection).RootReference;
    }

    private void Update()
    {
        // Si se completa el nivel, carga la pantalla de puntuaciones
        if (completado)
        {
            StartCoroutine(pausaYCarga(1.5f));
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        // Cuando colisiona una moneda, suma la puntuación y la destruye 
        if (other.gameObject.tag.Equals("Coin"))
        {
            Destroy(other.gameObject);
            string puntuacionSTR = scoreTMP.text;
            int puntuacionINT = Int32.Parse(puntuacionSTR);
            puntuacionINT += 10;
            totalScore += 10;
            scoreTMP.text = puntuacionINT.ToString();
            moneda.Play();
        }
        else if (other.gameObject.tag.Equals("BigCoin"))
        {
            Destroy(other.gameObject);
            string puntuacionSTR = scoreTMP.text;
            int puntuacionINT = Int32.Parse(puntuacionSTR);
            puntuacionINT += 50;
            totalScore += 50;
            scoreTMP.text = puntuacionINT.ToString();
            moneda.Play();
        }
        // Cuando colisiona con la meta, reproduce un sonido y acutaliza la puntuación en la BBDD
        else if (other.gameObject.tag.Equals("Finish"))
        {
            if (!sonando)
            {
                sonando = true;
                meta.Play();
                StartCoroutine(EsperarFinSonido(meta));
            }

            string username = PlayerPrefs.GetString("username");
            int score = Int32.Parse(scoreTMP.text);
            StartCoroutine(actualizarPuntuacion(username, score));
        }
    }

    // Actualiza la puntuación del usuario en la BBDD
    public IEnumerator actualizarPuntuacion(string username, int nuevaPuntuacion)
    {
        var userReference = reference.Child("users").Child("new_user_" + username).Child("score");
        yield return new WaitUntil(() => userReference != null);

        userReference.SetValueAsync(nuevaPuntuacion).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"Failed to update user score in Firebase: {task.Exception}\n For user {username}");
                return;
            }

            Debug.Log($"User score updated successfully for user {username}");
            completado = true;
        });
    }

    // Espera un cierto tiempo para asegurar la modificación y el sonido y carga la tabla de puntuaciones
    private IEnumerator pausaYCarga(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        meta.Play();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(4);
    }

    // Espera que termine la reproducción del sonido 
    private IEnumerator EsperarFinSonido(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        sonando = false;
    }
}