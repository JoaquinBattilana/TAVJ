using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField _clientSourcePort;
    public TMP_InputField _clientDestIp;
    public TMP_InputField _clientDestPort;
    public TMP_InputField _serverPort;

    public void Start() {
        _clientSourcePort.text = 9001.ToString();
        _clientDestIp.text = "127.0.0.1";
        _clientDestPort.text = 9000.ToString();
        _serverPort.text = 9000.ToString();
        SceneManager.UnloadSceneAsync("Client");
        SceneManager.UnloadSceneAsync("Server");
    }
    public void PlayGame() {
        PlayerPrefs.SetInt("clientSourcePort", int.Parse(_clientSourcePort.text));
        PlayerPrefs.SetString("clientDestIp", _clientDestIp.text);
        PlayerPrefs.SetInt("clientDestPort", int.Parse(_clientDestPort.text));
        SceneManager.LoadScene("Client");
    }

    public void HostGame() {
        PlayerPrefs.SetInt("serverPort", int.Parse(_serverPort.text));
        SceneManager.LoadScene("Server");
    }
}
