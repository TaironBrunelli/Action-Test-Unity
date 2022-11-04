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

[RequireComponent(typeof(CircleCollider2D))] //-- Unity will create the required components
[RequireComponent(typeof(Rigidbody2D))] //-- Unity will create the required components

public class Base_Character : MonoBehaviour
{
    //-- Player
    [Header("Player Controller")]
    public bool isPlayer;

    [Space]
    [Header("Base")]
    //-- Life
    public int hitPoints; //-- HP
    public int shield; //-- Extra HP

    //-- Movement
    private Vector2 m_Move;
	public float speed;
	private float deltaTime;
	private float moveSpeed;

    //-- Attack
    [Header("Attack")]
    public LayerMask enemyLayers;    
    [System.Serializable]
    public struct AttackInfo
    {
        public AttackInfo(int BaseHitDamage, float BaseHitDelay, float DistBackOff, float AttackAnim_T, Transform AttackPoint, Vector2 AttackRange, int AttackClip)
        {
            baseHitDamage = BaseHitDamage; //-- Base damage;
            baseHitDelay = BaseHitDelay; //-- Delay to invoke the collider in attack;
            distBackOff = DistBackOff; //-- Distance 
            attackAnim_T = AttackAnim_T; //-- Take the Lenght of Attack 1 Clip
            attackPoint = AttackPoint; //-- Attack position;
            attackRange = AttackRange; //--  Collider size;
            attackClip = AttackClip; //-- Number of the attack;

        }
        public int baseHitDamage;
        public float baseHitDelay;
        public float distBackOff;
        public float attackAnim_T;
        public Transform attackPoint;
        public Vector2 attackRange;
        public int attackClip;

    }
    public AttackInfo Attack_1;

    [Space]
	[Header("Animation")]
    public SpriteRenderer m_SpriteRenderer;
	public Animator BaseAnimation;
    public AnimationClip attackAnim_1;
    //protected float attackAnim_T_1; //-- Take the Lenght of Attack 1 Clip
    public AnimationClip dashAnim;
    protected float dashAnim_T; //-- Take the Lenght of Dash Clip
    public AnimationClip damagedAnim;
    protected float damagedAnim_T; //-- Take the Lenght of Damaged Clip
    public AnimationClip deadAnim;
    protected float deadAnim_T; //-- Take the Lenght of Damaged Clip

    //-- Character State 
    //--> MAYBE change 'protected' to 'private';
    protected bool isInvulnerable; //-- call if the character has to be invulnerable/ will not receive damage;
    protected bool isAttacking; //-- call if the character is attacking something;
    protected bool isAttacked; //-- call if the character receive one attack;
    protected bool isDashing; //-- call if the character is dashing/Special move ability;
    private bool isDead; //-- Toogle if HP <= 0;

    //-- Character Condition
    private bool isSlow; //-- Speed * 0.5;
	private bool isConfused; //-- Speed * -1;

    //-- Enemy
    [Space]
    [Header("Enemy Controller")]
    private GameObject Target; //-- Get Player by tag;
    private Transform targetPlayer;
    public float minDistance; //-- Min Distance between Enemy and Player;
    public bool isAggressive; //-- True -> Enemy may follow (until minDistance) and attack the Player at sight;
    public float enemyAttackCD; //-- CD to Enemy attack;
    protected float FireRate; //-- Aux variable to CD to Enemy attack;
    public float aggroDistance; //-- Turn the AI into Aggressive mode;     


    protected virtual void Start()
    {
        if (isPlayer)
            gameObject.tag = "Player";
        else
            gameObject.tag = "Enemy";
        
        if(!isPlayer)
            targetPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        if (hitPoints == 0)
            hitPoints = 10;
        if (speed == 0) 
            speed = 0.2f;
        if (Attack_1.baseHitDamage == 0)
            Attack_1.baseHitDamage = 1;
        if (Attack_1.baseHitDelay == 0)
            Attack_1.baseHitDelay = 0.15f;
        if (Attack_1.distBackOff == 0)
            Attack_1.distBackOff = .2f;

        //-- Counter Erro Attack 1 Animation;
        if(attackAnim_1 != null)
            Attack_1.attackAnim_T = attackAnim_1.length;
        else
            Attack_1.attackAnim_T = 0.5f;
        //-- Counter Erro Dash Animation;
        if (dashAnim != null)
            dashAnim_T = dashAnim.length;
        else
            dashAnim_T = 0.4f;
        //-- Counter Erro Damaged Animation;    
        if (damagedAnim != null)
            damagedAnim_T = damagedAnim.length;
        else
            damagedAnim_T = 0.25f;
        //-- Counter Erro Damaged Animation;    
        if(deadAnim != null)
            deadAnim_T = deadAnim.length;
        else
            deadAnim_T = 0.75f;
    }

    protected virtual void Update()
    {
        //-- Player Move
        if(isPlayer && !isDead)
        {
            if (m_Move.sqrMagnitude > 0.01)
                Move(m_Move);
            else if (!isAttacking)
                BaseAnimation.SetInteger("Speed", 0);
        }        
        // -- Enemy Move
        if(!isPlayer && !isDead)
        {
            if(Vector2.Distance(transform.position, targetPlayer.position) <= aggroDistance)
                isAggressive = true;
            Move_AI();
            //-- Attack when stop move;
            if(Vector2.Distance(transform.position, targetPlayer.position) <= minDistance)
                Attack_AI();
        }
            
    }

    //-- Move -- Enemy AI
    protected virtual void Move_AI()
    {
        BaseAnimation.SetInteger("Speed", 0);
        moveSpeed = speed * Time.fixedDeltaTime;

		if(isSlow == true)
		{
			moveSpeed *= (0.5f); 
		}
		if(isConfused == true)
		{
			moveSpeed *= (-1); 
		}
        if(isAttacking == true)
		{
			moveSpeed *= (.3f); 
		}
        if(isAttacked == true)
		{
			moveSpeed *= (.1f); 
		}

        //Follow and attack the Player at sight;
        if(isAggressive)
        {
            //-- Stop move and enable Attack_AI;
            if(Vector2.Distance(transform.position, targetPlayer.position) > minDistance)
            {
                BaseAnimation.SetInteger("Speed", 1); //-- Can switch to Float, and use 'direction.sqrMagnitude', with this could use the 'Run' animation here too.
                transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, moveSpeed);
            }
            if(transform.position.x > targetPlayer.position.x)
            {
                transform.localScale = new Vector3 (-1,1,1);
                //m_SpriteRenderer.flipX = false;    
            }
            else
            {
                transform.localScale = new Vector3 (1,1,1);
                //m_SpriteRenderer.flipX = true;
            }
        }
        //-- Override to explicit characters;
    }

    //-- Move -- Player
    public void OnMove(InputAction.CallbackContext context)
    {
        m_Move = context.ReadValue<Vector2>();
    }    

    protected virtual void Move(Vector2 direction)
	{
        float xDirection = direction.x;
        float yDirection = direction.y;

        if(isAttacking == false)
        {
            if (xDirection < 0)
                m_SpriteRenderer.flipX = true;
            else
                m_SpriteRenderer.flipX = false;
        }
        
        BaseAnimation.SetInteger("Speed", 1); //-- Can switch to Float, and use 'direction.sqrMagnitude', with this could use the 'Run' animation here too.
        moveSpeed = speed * Time.fixedDeltaTime;

		if(isSlow == true)
		{
			moveSpeed *= (0.5f); 
		}
		if(isConfused == true)
		{
			xDirection *= (-1); 
            yDirection *= (-1);
		}
        if(isAttacking == true)
		{
			moveSpeed *= (.3f);
		}
        if(isAttacked == true)
		{
			moveSpeed *= (.1f);
		}

		transform.position = new Vector2(Mathf.Lerp(this.transform.position.x, this.transform.position.x + xDirection, moveSpeed),
				Mathf.Lerp(this.transform.position.y, this.transform.position.y + yDirection, moveSpeed));
	}

    //-- Attack -- Enemy AI
    protected virtual void Attack_AI()
    {
        //-- Override to explicit characters;
        if (Time.time > FireRate)
        {
            FireRate = Time.time + enemyAttackCD;
            Attack(Attack_1);
        }
    }

    //-- Attack 1 -- Player
    public void OnAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if(!isAttacking && !isAttacked && !isDead)
                    Attack(Attack_1);
                break;

            case InputActionPhase.Started:
                
                break;

            case InputActionPhase.Canceled:
                //isAttacking = false;
                break;
        }
    }

    protected virtual void Attack(AttackInfo Attack)
    {
        //-- Override to explicit characters;
        StartCoroutine(AttackAnimationTime(Attack.attackAnim_T, Attack.attackClip));
        StartCoroutine(AttackDamageCollider(Attack.baseHitDamage, Attack.baseHitDelay, Attack.distBackOff, Attack.attackPoint, Attack.attackRange));
    }

    //-- Create a collider2D on Attack + Damage;
    private IEnumerator AttackDamageCollider(int attackDamage, float hitDelay, float backOff, Transform attkPositon, Vector2 attkRange)
    {
        //-- Flip the attack collider spawn
        if (m_SpriteRenderer.flipX == true && Attack_1.attackPoint.localPosition.x > 0)
            Attack_1.attackPoint.localPosition *= -1;
        else if (m_SpriteRenderer.flipX == false && Attack_1.attackPoint.localPosition.x < 0)
            Attack_1.attackPoint.localPosition *= -1;
        //-- Wait the frame to invoke the collider    
        yield return new WaitForSeconds(hitDelay);
        //-- Make the collider + Attack + Damage
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attkPositon.position, attkRange, 0f, enemyLayers);
                    
        foreach (Collider2D enemy in hitEnemies)
        {
            //-- TakeDamage can receive attackDamage and distBackOff;
            enemy.GetComponent<Base_Character>().TakeDamage(attackDamage, backOff);
        }
    }

    private IEnumerator AttackAnimationTime(float t, int clipSet)
	{
		isAttacking = true;
        BaseAnimation.SetTrigger("isAttacking");
        BaseAnimation.SetInteger("Attack", clipSet);
		yield return new WaitForSeconds(t);
        BaseAnimation.SetInteger("Attack", 0);
		isAttacking = false;
	}

    //-- Draw the collider on Scene
    private void OnDrawGizmosSelected() 
    {
        if (Attack_1.attackPoint == null)
            return;
        Gizmos.DrawWireCube(Attack_1.attackPoint.position, Attack_1.attackRange);
    }

    //-- Dash -- Enemy AI
    protected virtual void Dash_AI()
    {
        //-- Override to explicit characters;
    }

    //-- Dash -- Player
    public void OnDash(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if(!isAttacked && !isDashing && !isDead)
                    Dash();
                break;

            case InputActionPhase.Started:
                break;

            case InputActionPhase.Canceled:
                break;
        }
    }

    protected virtual void Dash()
    {
        //-- Override to explicit characters;
        StartCoroutine(DashAnimationTime(dashAnim_T));
    }

    private IEnumerator DashAnimationTime(float t)
	{
		isDashing = true;
        BaseAnimation.SetTrigger("isDashing");
		yield return new WaitForSeconds(t);
		isDashing = false;
	}

    //-- Call when collider with Player/Enemy Attack;
    private void TakeDamage(int damageHit, float backOff)
    {
        if (!isInvulnerable)
        {
            hitPoints -= damageHit;
            if (hitPoints > 0)
            {
                //StopAllCoroutines(); //-- Cancel Attacks (animations);
                StartCoroutine(DamagedRedColor(damagedAnim_T));
                StartCoroutine(TakeDamageAnimationTime(damagedAnim_T));
            }
            else
            {
                isDead = true;
                isInvulnerable = true; //-- Cancel calling TakeDamage repeatedly;
                StartCoroutine(DeadAnimationTime(deadAnim_T));
                if(isPlayer)
                    PlayerDead();
                else
                    Drop(); //-- Enemy Drop;
            }
            TookDamage();
            BackOff(backOff);
            BaseAnimation.SetInteger("Attack", 0); //-- Cancel Attack if take damage;    
            if(!isPlayer)
                AggressiveMode();        
        }
    }

    //-- Blink Red to White when take a hit
    private IEnumerator DamagedRedColor (float t)
    {
        float endTime = Time.time + t;
        if (m_SpriteRenderer != null)
        {
            while(Time.time < endTime)
            {
                m_SpriteRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                m_SpriteRenderer.material.color = Color.white;
                yield return new  WaitForSeconds(0.02f);
            }
			m_SpriteRenderer.material.color = Color.white;
        }
    }

    private IEnumerator TakeDamageAnimationTime (float t)
    {
        isAttacked = true;
        BaseAnimation.SetTrigger("isAttacked");
		yield return new WaitForSeconds(t);
		isAttacked = false;
    }

    private IEnumerator DeadAnimationTime (float t)
    {
        BaseAnimation.SetBool("isDead", true);
        BaseAnimation.SetTrigger("isAttacked");
		yield return new WaitForSeconds(t);
		Destroy(gameObject);
    }

    //-- If took damage, turn the AI into Aggressive Mode ;
    protected virtual void AggressiveMode()
    {
        isAggressive = true;
    }

    protected virtual void TookDamage()
    {
        //-- Override to explicit characters;
    }

    //-- Move back the character after take damage;
    private void BackOff(float t)
    {
        if(m_SpriteRenderer.flipX)
            t *= -1f;
        else
            t *= 1f;
        transform.Translate(new Vector3(t, 0, 0));
        Debug.Log(t);
    }

    protected virtual void Drop()
    {
        //-- Override to explicit characters;
    }

    private void PlayerDead ()
    {
        //-- Destroy player obj, save points, restart the game...
    }
}
