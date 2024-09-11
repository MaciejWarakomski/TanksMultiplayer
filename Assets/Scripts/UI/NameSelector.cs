using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UI
{
    public class NameSelector : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button connectButton;
        [SerializeField] private int minNameLength = 1;
        [SerializeField] private int maxNameLength = 12;
        
        public const string PlayerNameKey = "PlayerName";
        
        private void Start()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            
            nameField.text = PlayerPrefs.GetString(PlayerNameKey, nameField.text);
            HandleNameChange();
        }

        public void HandleNameChange()
        {
            connectButton.interactable = 
                nameField.text.Length >= minNameLength && 
                nameField.text.Length <= maxNameLength;
        }

        public void Connect()
        {
            PlayerPrefs.SetString(PlayerNameKey, nameField.text);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}