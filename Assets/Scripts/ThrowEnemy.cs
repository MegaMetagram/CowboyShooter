using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowEnemy : Enemy
{
    [SerializeField] private GameObject tntPrefab;    
    [SerializeField] private Transform tntSpawn;    
    [SerializeField] private float defaultThrowHeight = 2f;
    [SerializeField] private float minDistFromWall = 0.5f;
    private Vector3 shootPos;    

    [SerializeField] private GameObject tntChild;
    [SerializeField] public AudioSource boomSfx;

    private bool canThrow = true;

    private void ThrowTNT()
    {        
        // tntChild is not a TNT prefab. 
        // This prevents weird link issues and makes the tnt the enemy holds stay in place
        // When throwing, the tnt held is hidden and a new tnt is created to appear to be throwing a tnt        
        tntChild.GetComponent<MeshRenderer>().enabled = false;                

        Vector3 tntVel;
        if(PlayerIsSighted())        
            tntVel = PlayerSightedVel(defaultThrowHeight);
        else        
            tntVel = VelToClearWall();        
        
        GameObject newTnt = Instantiate(tntPrefab, tntSpawn.position, Quaternion.identity);                
        newTnt.GetComponent<Rigidbody>().velocity = tntVel;
        //Debug.Log("newTnt vel is: " + newTnt.GetComponent<Rigidbody>().velocity);        
    }

    private Vector3 ShootPos()
    {        
        RaycastHit hit;
        Vector3 playerDirection = (player.transform.position - tntSpawn.position).normalized;
        bool hitWall = (Physics.Raycast(tntSpawn.position, playerDirection, out hit, sightRange));
        float distToWall = hit.distance;
        float distToPlayer = DistToPlayer();
        
        Vector3 shootPos = tntSpawn.position + playerDirection * -(sightRange-1-distToPlayer);
        if (distToWall < minDistFromWall)
            canThrow = false;
        else
            canThrow = true;

        return shootPos;
    }

    private Vector3 PlayerSightedVel(float height)
    {        
        Vector3 playerDirection = (player.transform.position - tntSpawn.position).normalized;

        Vector3 levelPlayerPos = player.transform.position;
        levelPlayerPos.y = tntSpawn.position.y;        
        float distToPlayer = Vector3.Distance(tntSpawn.position, player.transform.position);                                

        float halfTntHeight = tntPrefab.transform.localScale.z * tntPrefab.GetComponent<CapsuleCollider>().height / 2;
        
        float landTime = 2f * Mathf.Sqrt(height / (-Physics.gravity.y / 2));
        float yV0 = -Physics.gravity.y * Mathf.Sqrt(height / (-Physics.gravity.y / 2));
        float xV0 = (distToPlayer / landTime) * playerDirection.x;
        float zV0 = (distToPlayer / landTime) * playerDirection.z;        

        return new Vector3(xV0, yV0, zV0);
    }

    private Vector3 VelToClearWall()
    {
        RaycastHit hit;
        RaycastHit straightHit;        
        Vector3 playerDirection = (player.transform.position - tntSpawn.position).normalized;   
        
        Vector3 levelPlayerPos = player.transform.position;
        levelPlayerPos.y = tntSpawn.position.y;
        float distToLevelPlayer = Vector3.Distance(tntSpawn.position, levelPlayerPos);

        float distToPlayer = Vector3.Distance(tntSpawn.position, player.transform.position);                
        bool isValidThrow = false;
        // consider non-wall objects laters
        bool hitWall = (Physics.Raycast(tntSpawn.position, playerDirection, out hit, sightRange));
        bool hitStraightToWall = (Physics.Raycast(tntSpawn.position, tntSpawn.forward, out straightHit, sightRange));
        float distStraightToWall = Vector3.Distance(tntSpawn.position, straightHit.point);

        // y0= 9.8*sqrt(wallHeight/4.9). initial y velocity needed to max y position at wallHeight
        // landTime= 2*sqrt(wallHeight/4.9). time from throwing to hit player, assuming y diff is 0
        // x0= horDist/landTime. initial x velocity needed to reach player by time
        // check if y(dist/totalDist * landTime) >= wallTop

        float halfTntHeight = tntPrefab.transform.localScale.z * tntPrefab.GetComponent<CapsuleCollider>().height / 2;        
        // account for width of tnt
        float relativeWallHeight = hit.collider.bounds.max.y + halfTntHeight - tntSpawn.position.y;
        // increase heightDiff until valid throw or angle limit exceeded        
        float landTime = 2f * Mathf.Sqrt(relativeWallHeight / (-Physics.gravity.y/2));
        float yV0 = -Physics.gravity.y * Mathf.Sqrt(relativeWallHeight / (-Physics.gravity.y / 2));
        float xV0 = (distToPlayer / landTime) * playerDirection.x;
        float zV0 = (distToPlayer / landTime) * playerDirection.z;
        
        float distToWall = Vector3.Distance(tntSpawn.position, hit.point);
        float t = (distToWall / distToPlayer) * landTime;
        // how to get distToEndOfWall?

        bool aboveWallStart = (yV0 * t - (-Physics.gravity.y/2) * t * t > relativeWallHeight);       
        // get angle to throw to just pass wall
        float angle = Mathf.Atan(yV0 / xV0);
        float adjustedWallHeight = relativeWallHeight;

        while (!isValidThrow && angle <= 89f*Mathf.Rad2Deg)
        {
            angle += (15f * Mathf.Deg2Rad);
            adjustedWallHeight = distToWall * Mathf.Tan(angle) + halfTntHeight;

            landTime = 2f * Mathf.Sqrt(adjustedWallHeight / (-Physics.gravity.y / 2));
            yV0 = -Physics.gravity.y * Mathf.Sqrt(adjustedWallHeight / (-Physics.gravity.y/2));            
            xV0 = (distToPlayer / landTime) * playerDirection.x;            
            zV0 = (distToPlayer / landTime) * playerDirection.z;            

            distToWall = Vector3.Distance(tntSpawn.position, hit.point);
            t = (distToWall / distToPlayer) * landTime;
            // how to get distToEndOfWall?

            aboveWallStart = (yV0 * t - (-Physics.gravity.y/2 * t * t) > relativeWallHeight);
            if (aboveWallStart)
            {
                //Debug.Log("angle " + angle*Mathf.Rad2Deg + " was valid throw");
                isValidThrow = true;
            }
        }        

        return new Vector3(xV0, yV0, zV0);                
    }

    // Update is called once per frame
    void Update()
    {
        if (!switchingDest && agent.remainingDistance <= 0.01f)
        {
            //Debug.Log("got to dest, find new dest");
            switchingDest = true;
            FindNewDest(agent.destination);
            return;
        }
        else if (switchingDest)
        {
            destCooldown += Time.deltaTime;
            if (destCooldown >= maxDestCooldown)
            {
                switchingDest = false;
                destCooldown = 0f;
            }
            return;
        }

        playerNear = PlayerIsNearby();
        playerSighted = PlayerIsSighted();

        if (playerNear)
        {                     
            shootPos = ShootPos();
            if (canThrow)
                agent.destination = shootPos;

            if (attackCooldown >= maxAttackCooldown && canThrow)
            {
                ThrowTNT();                
                attackCooldown = 0f;
            }
        }
        else if (agent.destination == shootPos)
        {            
            FindNewDest(agent.destination);
        }

        if (attackCooldown >= maxAttackCooldown / 2)
            tntChild.GetComponent<MeshRenderer>().enabled = true;

        attackCooldown += Time.deltaTime;
    }
}
