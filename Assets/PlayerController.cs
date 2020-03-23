using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0f,2f)]
    public float movementSpeed = 0.5f;
    [Range(1, 10)]
    [Tooltip("In how much segments the movement will be cut up. This mostly influences quality of collision checks.")]
    public int movementPrecision = 2;
    [Range(0f, 10f)]
    [Tooltip("You can snip with the space bar in-game.")]
    public float snippingRange = 1f;
    public InstancePool InstancePool;

    void Update()
    {
        float horizontalDelta = Input.GetAxis("Horizontal");
        float verticalDelta = Input.GetAxis("Vertical");

        if (horizontalDelta != 0 || verticalDelta != 0)
        {
            var movementSegment = new Vector3(horizontalDelta * movementSpeed / (float) movementPrecision, 0f, verticalDelta * movementSpeed / (float) movementPrecision);

            var segments = 0;
            while (segments != movementPrecision)
            {
                List<RaycastHit> collisionResults = new List<RaycastHit>(Physics.RaycastAll(transform.position, movementSegment, movementSpeed / movementPrecision));
                var wall = collisionResults.Find(x => x.transform.gameObject.tag == "Wall");
                if (wall.transform != null)
                {
                    break;
                }
                else
                {
                    transform.position += movementSegment;
                    segments += 1;
                }
            }

            if (segments >= 1)
            {
                var turnDirection = new Vector3(horizontalDelta, 0, verticalDelta);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(turnDirection), 0.20F);

                var activatedInstance = InstancePool.ActivateFromPool(transform.position);
                if (activatedInstance != null)
                {
                    activatedInstance.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0f, 360f), 0));
                }
            }
        }

        if (Input.GetButton("Snip"))
        {
            var collisionResults = Physics.SphereCastAll(transform.position, snippingRange, transform.rotation.eulerAngles);
            foreach (RaycastHit hit in collisionResults)
            {
                if (hit.transform.tag == "Flower")
                {
                    InstancePool.DeactivateFromPool(hit.transform.gameObject);
                }
            }
        }
    }
}
