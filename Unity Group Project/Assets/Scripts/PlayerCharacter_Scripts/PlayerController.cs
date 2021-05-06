﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //These are set in the editor
    public GameObject healthBar;
    private Vector3 hbarScale;
    public GameObject energyBar;
    private Vector3 ebarScale;
    public Text textBox;
    public GameObject aimReticle;
    public Animator aimAnimator;
    public GameObject bubbleEmotes;

    public Text resultStat1;
    public Text resultStat2;
    public Text resultStat3;
    public Text resultStat4;
    public Text resultStat5;
    public Text resultStat6;
    public Text resultStat7;

    public GameObject enemyCharacter;

    //These are set in the editor (for now)
    [SerializeField] float currentHealth;
    [SerializeField] float maxHealth;
    [SerializeField] float currentEnergy;
    [SerializeField] float maxEnergy;
    [SerializeField] float movementSpeed;
    [SerializeField] float weight;
    [SerializeField] float attackPower;

    //Time variables:
    public float timePassed;
    public float aimTimer;
    public float attackTimer;

    //Resource variables:
    public bool tookDamage;
    public bool usedEnergy;

    //Attack Variables:
    public bool isAttacking;
    public bool startedAttack;
    public bool endedAttack;
    public int attacksEarned;
    public bool allowDamageDecrement;


    //Aim Variables:
    public bool isAiming;
    public bool startedAim;
    public bool endedAim;
    public bool aimReticleActive;

    //Movement Variables:
    public Vector2 movement;
    public float hMovement;
    public float vMovement;
    public float jumpForce = 18.0f;
    public bool facingLeft;
    public bool facingRight;
    public bool isWaiting;
    public bool combatEnded;

    //Appearance Variables:
    public Color playerColor;
    public bool isVisible;

    //End Result Stat Variables:
    public float damageDelt;
    public float atkAccuracy;
    public float damageTaken;
    public float dodgeAccuracy;
    public float numTurns;
    public float mistakesMade;
    public float currencyEarned;

    public float totalAttacksEarned;
    public float totalAttacksMissed;
    public float enemyAttacksDodged;
    public float enemyAttacksPerformed;

    //These are set in the editor
    public Transform feet;
    public LayerMask groundLayers;

    //Initialized in Start()
    public Rigidbody2D rigid;
    public Animator playerAnimator;
    public InputManager inputManager;




    // Start is called before the first frame update
    void Start()
    {
        /*
        Transform childTransform = gameObject.transform.Find("HealthBar_Player");
        if (childTransform != null)
        {
            healthBar = childTransform.gameObject;
        }
        */

        if (healthBar != null && healthBar.tag == "ResourceBar")
        {
            hbarScale = healthBar.transform.localScale;
        }

        if (energyBar != null && energyBar.tag == "ResourceBar")
        {
            ebarScale = energyBar.transform.localScale;
        }


        if (textBox != null)
        {
            textBox.text = timePassed.ToString("F2");
            textBox.enabled = false;
        }

        if (aimReticle != null)
        {
            aimAnimator = aimReticle.GetComponent<Animator>();
            aimReticle.SetActive(false);
            aimReticleActive = false;
        }

        tookDamage = false;
        usedEnergy = false;
        isAttacking = false;
        isAiming = false;
        isWaiting = true;

        startedAttack = false;
        endedAttack = false;
        attackPower = 1.0f;
        attacksEarned = 0;
        allowDamageDecrement = false;
        startedAim = false;
        endedAim = false;

        movement = new Vector2(0, 0);
        hMovement = 0;
        vMovement = 0;
        movementSpeed = 5.0f;
        weight = 5.0f;
        facingLeft = false;
        facingRight = true;

        damageDelt = 0.0f;
        atkAccuracy = 0.0f;
        damageTaken = 0.0f;
        dodgeAccuracy = 0.0f;
        numTurns = 0.0f;
        mistakesMade = 0.0f;
        currencyEarned = 0.0f;

        totalAttacksEarned = 0.0f;
        totalAttacksMissed = 0.0f;
        enemyAttacksDodged = 0.0f;
        enemyAttacksPerformed = 0.0f;

        playerColor = GetComponent<SpriteRenderer>().color;
        isVisible = true;

        rigid = GetComponent<Rigidbody2D>();
        rigid.freezeRotation = true;
        inputManager = InputManager.instance;

        if (playerAnimator == null) { playerAnimator = gameObject.GetComponent<Animator>(); }
        if (isVisible == true) { playerAnimator.SetBool("isVisible", true); }
    }//End of Start




    // Update is called once per frame
    void Update()
    {
        //Player is attacked--
        if (tookDamage)
        {
            StartCoroutine(DamageCoroutine());
        }
        //Set health to persistent data
        PersistentData.Instance.SetHealth((int)currentHealth);

        //Player uses energy for abilities--
        if (usedEnergy)
        {
            AdjustEnergyBar();
            usedEnergy = false;
        }

        //Player is readying an attack--
        if (isAiming)
        {
            if (aimReticleActive == false)
            {
                aimReticle.SetActive(true);
                aimReticleActive = true;
                startedAim = true;
            }
            else if (startedAim)
            {
                startedAim = false;
                timePassed = 0.00f;
                textBox.enabled = true;
                playerAnimator.SetBool("isAiming", true);
                aimAnimator.SetBool("isAiming", true);
                playerAnimator.SetBool("isAttacking", false);
            }

            if (aimReticleActive)
            {
                timePassed += Time.deltaTime;
                textBox.text = timePassed.ToString("F2");
            }

            if (timePassed >= 0.8f && timePassed <= 1.1f
                && inputManager.GetKeyDown(KeyBindingActions.Select1))
            {
                //Locked-on aim
                Debug.Log("LOCKED-ON TARGET!!!");
                attacksEarned += 1;
                totalAttacksEarned += 1;
                if (totalAttacksEarned != 0 && totalAttacksMissed != 0)
                {
                    atkAccuracy = (totalAttacksEarned / (totalAttacksEarned + totalAttacksMissed));
                    resultStat2.text = atkAccuracy.ToString("F2");
                }
            }
            else if (timePassed >= 1.6f && timePassed <= 1.98f
			          && inputManager.GetKeyDown(KeyBindingActions.Select1))
            {
                //Locked-on aim again
                Debug.Log("LOCKED-ON TARGET!!!");
                attacksEarned += 1;
                totalAttacksEarned += 1;
                if (totalAttacksEarned != 0 && totalAttacksMissed != 0)
                {
                    atkAccuracy = (totalAttacksEarned / (totalAttacksEarned + totalAttacksMissed));
                    resultStat2.text = atkAccuracy.ToString("F2");
                }
                endedAim = true;
            }

            else if ((((timePassed >= 0.1f && timePassed < 0.8f)
                || (timePassed > 1.1f && timePassed < 1.6f)
                || (timePassed > 1.98f))
                && inputManager.GetKeyDown(KeyBindingActions.Select1))
                || (timePassed > 2.1f && attacksEarned == 0))
            {
                //Missed aim
                Debug.Log("NO TARGET IN SIGHTS...");
                isAiming = false;
                playerAnimator.SetBool("isAiming", false);
                aimAnimator.SetBool("isAiming", false);
                aimReticle.SetActive(false);
                aimReticleActive = false;
                textBox.enabled = false;
                timePassed = 0.0f;
                endedAim = true;
                if (attacksEarned == 1) { totalAttacksMissed += 1; }
                else if (attacksEarned == 0) { totalAttacksMissed += 2; }
                if (totalAttacksEarned != 0 && totalAttacksMissed != 0)
                {
                    atkAccuracy = (totalAttacksEarned / (totalAttacksEarned + totalAttacksMissed));
                    resultStat2.text = atkAccuracy.ToString("F2");
                }
            }
            else if (timePassed > 2.1f && attacksEarned > 0)
            {
                Debug.Log("AIM IS READY!!! Beginning attack...");
                endedAim = true;
            }

            if (endedAim)
            {
                endedAim = false;
                isAiming = false;
                aimAnimator.SetBool("isAiming", false);
                aimReticle.SetActive(false);
                aimReticleActive = false;
                textBox.enabled = false;
                isAttacking = true;
                startedAttack = true;
                timePassed = 0.0f;
            }
        }

        //Player is attacking an enemy--
        else if (isAttacking)
        {
            if (startedAttack)
            {
                startedAttack = false;
                timePassed = 0.00f;
                textBox.enabled = true;
                playerAnimator.SetBool("isAttacking", true);
                playerAnimator.SetBool("isAiming", false);
                if (attacksEarned > 0)
                {
                    allowDamageDecrement = true;
                }
            }

            timePassed += Time.deltaTime;
            textBox.text = timePassed.ToString("F2");

            if (attacksEarned == 1 || attacksEarned == 2)
            {
                if ((timePassed < 0.5f && timePassed >= 0.4f) && allowDamageDecrement)
                {
                    //Decrement enemy health by 1
                    if (enemyCharacter != null)
                    {
                        if (enemyCharacter.tag == "Enemy")
                        {
                            float enemyHealth = enemyCharacter.GetComponent<EnemyController>().GetCurrentHealth();
                            enemyCharacter.GetComponent<EnemyController>().SetCurrentHealth(enemyHealth - attackPower);
                            enemyCharacter.GetComponent<EnemyController>().DamageTaken(true);
                        }
                        damageDelt += attackPower;
                        resultStat1.text = damageDelt.ToString("F2");
                    }
                    allowDamageDecrement = false;
                    if (attacksEarned == 1) { endedAttack = true; }
                }
            }

            if ((timePassed >= 0.55f && timePassed < 0.8f) && allowDamageDecrement == false)
            {
                allowDamageDecrement = true;
            }

            if (attacksEarned == 2)
            {
                if (timePassed >= 0.8f && allowDamageDecrement)
                {
                    //Decrement enemy health by 2
                    if (enemyCharacter != null)
                    {
                        if (enemyCharacter.tag == "Enemy")
                        {
                            float enemyHealth = enemyCharacter.GetComponent<EnemyController>().GetCurrentHealth();
                            enemyCharacter.GetComponent<EnemyController>().SetCurrentHealth(enemyHealth - (attackPower * 2));
                            enemyCharacter.GetComponent<EnemyController>().DamageTaken(true);
                        }
                        damageDelt += (attackPower * 2);
                        resultStat1.text = damageDelt.ToString("F2");
                    }
                    allowDamageDecrement = false;
                    endedAttack = true;
                }

            }

            if (attacksEarned == 0)
            {
                endedAttack = true;
            }

            //***TO_DO NOTE:***
            //  - we need to make a coroutine or find some other way to have
            //      the "endedAttack" boolean flag remain true long enough to enter
            //      the enemy's attack phase
            if (endedAttack)
            {
                playerAnimator.SetBool("isAttacking", false);
                isAttacking = false;
                textBox.enabled = false;
                timePassed = 0.00f;
                attacksEarned = 0;
                StartCoroutine(WaitForAttackEndCoroutine());
                StartCoroutine(ResetEndedAttackCoroutine());
            }
        }

        //Player Movement--
        if (isWaiting == true)
        {
            //Do nothing
        }
        else if (isWaiting == false)
        {
            hMovement = Input.GetAxisRaw("Horizontal");

            if ((inputManager.GetKeyDown(KeyBindingActions.Jump1)
                || inputManager.GetKeyDown(KeyBindingActions.Jump2)) && IsGrounded())
            {
                Jump();
            }
        }

    }//End of Update


    void FixedUpdate()
    {
        if (isWaiting == true)
        {
            //Do nothing
            if (combatEnded)
            {
                combatEnded = false; //Only reset player animation to "Idle" once...
                playerAnimator.SetBool("isVisible", true);
                playerAnimator.SetBool("isWalking", false);
                numTurns += 1;
                resultStat5.text = numTurns.ToString("F2");
                enemyAttacksPerformed = enemyCharacter.GetComponent<EnemyController>().GetAttacksPerformed();
                enemyAttacksDodged = enemyCharacter.GetComponent<EnemyController>().GetAttacksMissed();
                dodgeAccuracy = (enemyAttacksDodged / enemyAttacksPerformed);
                resultStat4.text = dodgeAccuracy.ToString("F2");
            }
        }
        else if (isWaiting == false)
        {
            movement = new Vector2(hMovement * movementSpeed, rigid.velocity.y);
            rigid.velocity = movement;

            //Have the player face left when moving left--
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                if (facingRight)
                {
                    transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                    facingLeft = true;
                    facingRight = false;
                }
            }
            //Have the player face right when moving right--
            else if (Input.GetAxisRaw("Horizontal") > 0)
            {
                if (facingLeft)
                {
                    transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                    facingRight = true;
                    facingLeft = false;
                }
            }

            //Play the walking animation when moving on the ground--
            if (rigid.velocity.x > 0 && IsGrounded()) {
                if (facingLeft)
                {
                    transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                    facingRight = true;
                    facingLeft = false;
                }
                else
                {
                    playerAnimator.SetBool("isWalking", true);
                }
            }
            else if (rigid.velocity.x < 0 && IsGrounded())
            {
                if (facingRight)
                {
                    transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                    facingLeft = true;
                    facingRight = false;
                }
                else
                {
                    playerAnimator.SetBool("isWalking", true);
                }
            }
            //Stop the walking animation when not moving on the ground--
            else if (rigid.velocity.x == 0 && IsGrounded())
            {
                playerAnimator.SetBool("isWalking", false);
            }
        }
    }//End of FixedUpdate




    //Movement Related Methods:
    private void Jump()
    {
        movement = new Vector2(rigid.velocity.x, jumpForce);
        rigid.velocity = movement;
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (facingRight)
            {
                transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                facingLeft = true;
                facingRight = false;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            if (facingLeft)
            {
                transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                facingRight = true;
                facingLeft = false;
            }
        }
    }

    public bool IsGrounded()
    {
        Collider2D groundCheck = Physics2D.OverlapCircle(feet.position, 0.5f, groundLayers);
        if (groundCheck != null)
        {
            return true;
        }
        return false;
    }




    //Health Related Methods:
    public void SetCurrentHealth(float health) { currentHealth = health; }
    public float GetCurrentHealth() { return currentHealth; }

    public void DamageTaken(bool truthValue) { tookDamage = true; }

    IEnumerator DamageCoroutine()
    {
        enemyAttacksPerformed = enemyCharacter.GetComponent<EnemyController>().GetAttacksPerformed();
        enemyAttacksDodged = enemyCharacter.GetComponent<EnemyController>().GetAttacksMissed();
        float prevHealth = currentHealth;
        //Change player color to red
        AdjustHealthBar();
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.25f);
        GetComponent<SpriteRenderer>().color = playerColor;
        tookDamage = false;
        damageTaken += (prevHealth - currentHealth);
        dodgeAccuracy = (enemyAttacksDodged / enemyAttacksPerformed);
        resultStat4.text = dodgeAccuracy.ToString("F2");
        resultStat3.text = damageTaken.ToString("F2");
    }


    //Resource Related Methods:
    public void AdjustHealthBar()
    {
        float healthRatio = currentHealth / maxHealth;
        if (healthBar.tag == "ResourceBar")
        {
            if (healthRatio == 1)
            {
                healthBar.transform.localScale = hbarScale;
            }
            else if (healthRatio < 1 && healthRatio >= 0)
            {
                Vector3 barScale = hbarScale; //Player maximum health (represented by bar)
                healthRatio = barScale.x * healthRatio; //Player remaining health (represented by bar)
                float lostHealth = barScale.x - healthRatio;
                Vector3 adjustment = new Vector3(lostHealth, 0, 0);
                healthBar.transform.localScale = hbarScale - adjustment;
            }
        }
    }

    public void AdjustEnergyBar()
    {
        float energyRatio = currentEnergy / maxEnergy;
        if (energyBar.tag == "ResourceBar")
        {
            if (energyRatio == 1)
            {
                energyBar.transform.localScale = ebarScale;
            }
            else if (energyRatio < 1 && energyRatio >= 0)
            {
                Vector3 barScale = ebarScale; //Player maximum energy (represented by bar)
                energyRatio = barScale.x * energyRatio; //Player remaining energy (represented by bar)
                float lostEnergy = barScale.x - energyRatio;
                Vector3 adjustment = new Vector3(lostEnergy, 0, 0);
                energyBar.transform.localScale = ebarScale - adjustment;
            }
        }
    }




    //Attack Related Methods:
    public void TakeAim() { isAiming = true; }

    public bool AttackHasEnded() { return endedAttack; }

    IEnumerator WaitForAttackEndCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        endedAttack = true;
    }

    IEnumerator ResetEndedAttackCoroutine()
    {
        yield return new WaitForSeconds(2.5f);
        endedAttack = false;
    }


    //Movement Related Methods:
    public void MovementEnabled(bool enable)
    {
        if (enable == true)
        {
            isWaiting = false;
        }
        else if (enable == false)
        {
            combatEnded = true;
            isWaiting = true;
        }
    }


    //Emote Related Methods:
    public void StartEmote(string emoteName)
    {
        //Display emote bubble based on "emoteName" given
        if (emoteName == "Ellipses")
        {
            GameObject bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Ellipses").gameObject;
            bubbleEmote.SetActive(true);
            bubbleEmote.GetComponent<Animator>().SetBool("madeChoice", false);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", true);
        }
        else if (emoteName == "Exclaim")
        {
            GameObject bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Exclaim").gameObject;
            bubbleEmote.SetActive(true);
            bubbleEmote.GetComponent<Animator>().SetBool("hasEnded", false);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", true);
        }
        else if (emoteName == "Lightbulb")
        {
            GameObject bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Lightbulb").gameObject;
            bubbleEmote.SetActive(true);
            bubbleEmote.GetComponent<Animator>().SetBool("hasEnded", false);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", true);
        }
        else
        {
            Debug.Log("ERROR: Emote with name " + emoteName + " could not be found!!!");
        }
    }

    public void EndEmote(string emoteName)
    {
        GameObject bubbleEmote = null;
        //Hide emote bubble based on "emoteName" given
        if (emoteName == "Ellipses")
        {
            bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Ellipses").gameObject;
            bubbleEmote.GetComponent<Animator>().SetBool("madeChoice", true);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", false);
        }
        else if (emoteName == "Exclaim")
        {
            bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Exclaim").gameObject;
            bubbleEmote.GetComponent<Animator>().SetBool("hasEnded", true);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", false);
        }
        else if (emoteName == "Lightbulb")
        {
            bubbleEmote = bubbleEmotes.transform.FindChild("BubbleEmote_Lightbulb").gameObject;
            bubbleEmote.GetComponent<Animator>().SetBool("hasEnded", true);
            bubbleEmote.GetComponent<Animator>().SetBool("isVisible", false);
        }
        else
        {
            Debug.Log("ERROR: Emote with name " + emoteName + " could not be found!!!");
        }
            
    }


    //Sorting Related Methods:
    public void IncrementMistakesMade()
    {
        mistakesMade += 1;
        resultStat6.text = mistakesMade.ToString("F2");
    }


}//End of PlayerController