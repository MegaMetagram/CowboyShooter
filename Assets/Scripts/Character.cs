/*
@Authors - Patrick, Landon
@Description - Character class that defines behavior for all objects that can shoot. Basically an interface but with 
datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour
{
    //serialize needed to customize prefabs
    [SerializeField] protected int health;
    // in seconds
    [SerializeField] public float maxShootCooldown;    
    [SerializeField] public float shootCooldown;
    [SerializeField] protected float speed;

    protected AudioSource shootSfx;

    protected Weapon weapon;
    protected Rigidbody rigidbody;
    

    virtual protected void Shoot(GameObject obj)
    {
        Debug.LogWarning("You better implement the Shoot() method, or you'll be in a heap of trouble, partner.");
    }

    virtual protected void Death()
    {
        Debug.LogWarning("You better implement the Death() method, or you'll be in a heap of trouble, partner.");
    }

    virtual public void TakeDamage(int damage)
    {
        Debug.LogWarning("You better implement the TakeDamage() method, or you'll be in a heap of trouble, partner.");
    }
}
