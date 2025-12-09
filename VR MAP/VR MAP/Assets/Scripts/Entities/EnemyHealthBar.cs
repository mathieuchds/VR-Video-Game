using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform bar; 

    public void SetHealth(float ratio)
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (bar == null)
        {
            bar = transform;
        }
        ratio = Mathf.Clamp01(ratio);

        bar.localScale = new Vector3(ratio, 0.2f, 1f);
        
        bar.GetComponent<Renderer>().material.color =
            Color.Lerp(Color.red, Color.green, ratio);
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            transform.LookAt(cam.transform);
            transform.Rotate(0, 180f, 0); //la barre de vie est inversée?
        }
            
    }
}
