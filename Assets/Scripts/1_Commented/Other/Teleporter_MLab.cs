using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter_MLab : MonoBehaviour
{
    public Transform secondTeleporter;
    public float cooldown = 3f;

    private bool active;

    private void Start()
    {
        active = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("entered");

        if (!active) return;

        if(other.gameObject.tag == "Player")
        {
            Teleport(other.transform);
            print("teleporting...");
        }
    }

    private void Teleport(Transform objToTeleport)
    {
        objToTeleport.position = secondTeleporter.position;
        secondTeleporter.GetComponent<Teleporter_MLab>().StartCooldown();
    }

    public void StartCooldown()
    {
        active = false;
        Invoke(nameof(ResetCooldown), cooldown);
    }
    private void ResetCooldown()
    {
        active = true;
    }
}
