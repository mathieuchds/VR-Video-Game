using UnityEngine;

public class GunShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject bombaPrefab;

    public Transform muzzle;
    public float shootForce = 500f;


    public void Shoot(float dmg)
    {

        GameObject bullet = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // met les dégats du joueur
        Projectile p = bullet.GetComponent<Projectile>();
        if (p != null)
            p.damage = dmg;

        // partie physique du projectile
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(muzzle.forward * shootForce);

        Destroy(bullet, 5f);
    }

    public void Throw()
    {
        GameObject bomba = Instantiate(bombaPrefab, muzzle.position, muzzle.rotation);

        // partie physique du projectile
        Rigidbody rb = bomba.GetComponent<Rigidbody>();
        rb.AddForce(muzzle.forward * shootForce);

        Destroy(bomba, 10f);
    }

    public void AddModule(string moduleName)
    {
        Transform module = transform.Find("gun_base/" + moduleName);

        if (module == null)
        {
            Debug.LogError("Module not found: " + moduleName);
            return;
        }

        module.gameObject.SetActive(true);
    }
}
