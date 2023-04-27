using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera camera;

    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Health Related Stuff")] float startHealth = 100;
    private float health;

    [SerializeField] private Image healthBar;

    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out var hit, 100))
        {
            
            photonView.RPC("CreateHitEffect", RpcTarget.All, hit.point);
            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10f);
                
            }
        }
    }

    [PunRPC]
    public void TakeDamage(float damage, PhotonMessageInfo info)
    {
        health -= damage;
        healthBar.fillAmount = health / startHealth;

        if (health <= 0)
        {
            Die();
            Debug.Log($"{info.Sender.NickName} killed {info.photonView.Owner.NickName}");
        }
    }

    [PunRPC]
    public void CreateHitEffect(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.5f);
    }
    void Die()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        animator.SetBool("IsDead", true);
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        float respawnTime = 8f;
        GameObject respawnText = GameObject.Find("RespawnText");
        while (respawnTime > 0f)
        {
            yield return new WaitForSeconds(1f);
            respawnTime -= 1.0f;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            respawnText.GetComponent<TextMeshProUGUI>().text = "You a re killed. Reaspawn at: " + respawnTime.ToString(".00");
        }
        animator.SetBool("IsDead", false);
        respawnText.GetComponent<TextMeshProUGUI>().text = "";

        int randomPoint = Random.Range(-20, 20);
        transform.position = new Vector3(randomPoint, 0, randomPoint);
        transform.GetComponent<PlayerMovementController>().enabled = true;
        
        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;

    }
}
