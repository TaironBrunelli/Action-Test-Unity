/// <summary>
/// This is the Base Character:
/// - Life, Speed, Shield; 
/// - Function to call Attack, Dash, Special;
/// - Use override to explicit characters;
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEditor;

public class HeroKnight_Char : Base_Character
{
    //Dash
    private bool isRunning;
    //-- Attack
    [Header("Attack 2")]
    public AttackInfo Attack_2;
    public AnimationClip attackAnim_2;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //-- Counter Erro Attack 2 Animation;
        if(attackAnim_2 != null)
            Attack_2.attackAnim_T = attackAnim_2.length;
        else
            Attack_2.attackAnim_T = 0.75f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    // -- Skeleton will always move forward the Player
    protected override void Move_AI()
    {
        base.Move_AI();
    }

    // -- Skeleton will always move forward the Player
    protected override void Attack_AI()
    {
        //-- Override to explicit characters;
        int rand;
        rand = Random.Range(0, 2);
        if (Time.time > FireRate)
        {
            FireRate = Time.time + enemyAttackCD;
            if (rand == 0)
                Attack(Attack_1);
            else
                Attack(Attack_2);
        }
    }

    //-- Skeleton may use the same base in both attacks, changing only the variables.
    public void OnAttack_2(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if(!isAttacking && !isAttacked)
                    {
                        Attack(Attack_2);
                        //-- Flip the attack collider spawn
                        if (m_SpriteRenderer.flipX == true && Attack_2.attackPoint.localPosition.x > 0)
                            Attack_2.attackPoint.localPosition *= -1;
                        else if (m_SpriteRenderer.flipX == false && Attack_2.attackPoint.localPosition.x < 0)
                            Attack_2.attackPoint.localPosition *= -1;
                    }
                break;

            case InputActionPhase.Started:
                break;

            case InputActionPhase.Canceled:
                //isAttacking = false;
                break;
        }
    }

    //-- Draw the collider on Scene
    private void OnDrawGizmosSelected() 
    {
        if (Attack_2.attackPoint == null)
            return;
        Gizmos.DrawWireCube(Attack_2.attackPoint.position, Attack_2.attackRange);
    }

    protected override void Dash()
    {
        //base.Dash(); //-- Run -> Skeleton's Dash
        if (!isRunning) //-- Prevent "Double Run".
        {
            isRunning = true;
            BaseAnimation.SetBool("isRunning", true);
            speed += .5f;
        }
    }

    private void StopDash()
    {
        if(isRunning)
        {
            BaseAnimation.SetBool("isRunning", false);
            speed -= .5f;
            isRunning = false;
        }
    }

    protected override void TookDamage()
    {
        StopDash();
    }
}
