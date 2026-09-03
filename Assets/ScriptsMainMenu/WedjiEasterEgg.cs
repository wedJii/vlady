using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WedjiEasterEgg : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string targetScene = "poshalko";

    public void OnPointerClick(PointerEventData eventData) => Trigger();

    public void Trigger()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene);
    }
}
