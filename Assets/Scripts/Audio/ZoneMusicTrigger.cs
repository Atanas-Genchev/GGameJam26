using UnityEngine;

public class ZoneMusicTrigger : MonoBehaviour
{
    public AudioClip zoneMusic; // Песента, с която започва нивото

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Просто казваме на мениджъра да смени песента
            // Той сам ще се погрижи за Fade Out/Fade In
            if (MusicManager.Instance != null && zoneMusic != null)
            {
                MusicManager.Instance.PlayMusic(zoneMusic);
            }

            // Опция: Унищожаваме тригъра, за да не се вика пак
            Destroy(gameObject);
        }
    }
}