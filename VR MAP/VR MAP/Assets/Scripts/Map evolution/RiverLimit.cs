using UnityEngine;

public class RiverLimit : MonoBehaviour
{
    [SerializeField] private float respawnOffset = 5f; // Décalage augmenté pour éviter le bord
    [SerializeField] private string playerTag = "Joueur";
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string groundTag = "Ground";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter avec {other.gameObject.name}, tag: {other.tag}");

        if (other.CompareTag(enemyTag))
        {
            Debug.Log($"Ennemi détecté ({other.gameObject.name}), destruction...");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(playerTag) || other.CompareTag(bossTag))
        {
            Debug.Log($"Joueur ou Boss détecté ({other.gameObject.name}), recherche du sol le plus proche...");
            Transform closestGround = FindClosestGround(other.transform.position);
            if (closestGround != null)
            {
                Vector3 respawnPosition = closestGround.position + Vector3.up * respawnOffset;
                Debug.Log($"Réapparition à la position : {respawnPosition}");
                other.transform.position = respawnPosition;
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero; // Réinitialise la vitesse
                    Debug.Log("Vitesse du Rigidbody réinitialisée.");
                }
            }
            else
            {
                Debug.LogWarning("Aucun sol trouvé pour la réapparition !");
            }
        }
    }

    private Transform FindClosestGround(Vector3 position)
    {
        GameObject[] grounds = GameObject.FindGameObjectsWithTag(groundTag);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject ground in grounds)
        {
            float dist = Vector3.Distance(position, ground.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = ground.transform;
            }
        }
        Debug.Log(closest != null
            ? $"Sol le plus proche trouvé : {closest.gameObject.name} à {closest.position}"
            : "Aucun sol trouvé.");
        return closest;
    }
    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }
}
