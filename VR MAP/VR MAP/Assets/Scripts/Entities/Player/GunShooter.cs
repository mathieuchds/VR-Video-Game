using UnityEngine;

public class GunShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
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
}
