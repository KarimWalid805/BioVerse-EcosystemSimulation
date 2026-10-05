using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationExit : MonoBehaviour
{
   
    public void ReturnToMainMenu()
    {
        
       
        SceneManager.LoadScene("MainMenu"); 
    }

   
  
}