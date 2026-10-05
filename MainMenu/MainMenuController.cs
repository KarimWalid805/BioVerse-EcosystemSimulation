using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
  
    public void StartSimulation()
    {
       
        SceneManager.LoadScene("Simulation"); 
    }

      public void ExitGame()
    {
        Debug.Log("Exiting Simulation...");
        Application.Quit();
    }
}