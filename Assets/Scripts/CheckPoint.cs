using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Animator anim;
    private bool isActivated = false;
    private Renderer render;

    void Awake()
    {
        render = GetComponent<Renderer>();
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            anim.SetTrigger("Activate");
            isActivated = true;
            Player player = collision.GetComponent<Player>();
            if (player != null && render.isVisible)
            {
                player.curHelath = player.maxHelath;
                GameManager.instance.UpdateHealth(player.curHelath, player.maxHelath);
            }
            GameManager.instance.SetCheckPoint(transform.position);
        }
    }

}
