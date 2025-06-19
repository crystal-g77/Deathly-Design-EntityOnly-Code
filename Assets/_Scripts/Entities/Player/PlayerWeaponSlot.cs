using UnityEngine;

// a class to manage the weapon slots for the player
public class PlayerWeaponSlot : WeaponSlot
{
    [SerializeField]
    private PlayerLoadout.WeaponSlotId slotId;
    [SerializeField]
    private InputManager.AttackInput attackInput;

    private GameObject crosshair;
    private PlayerStats playerStats;
    private WeaponManager weaponManager;
    private InputManager inputManager;
    private bool isButtonHeld = false;

    new void Awake()
    {
        base.Awake();

        Transform crosshairTarget = Camera.main.transform.Find("CrosshairTarget");
        if (crosshairTarget)
        {
            crosshair = crosshairTarget.GetChild(0).gameObject;
        }

        playerStats = GetComponentInParent<PlayerStats>();
        weaponManager = WeaponManager.Instance;
        inputManager = InputManager.Instance;

        playerStats.OnDeath += onPlayerDeath;
        gameManager.OnGameOver += onGameOver;

        // register for attack button inputs depending on which attack input maps to this weapon slot
        inputManager.OnAttackTriggered[(int)attackInput] += onAttackTriggered;
        inputManager.OnAttackStarted[(int)attackInput] += onAttackStarted;
        inputManager.OnAttackEnded[(int)attackInput] += onAttackEnded;

        // delay equipping weapon until other classes have executed Start()
        Invoke(nameof(delayedStart), .001f);
    }

    new void Update()
    {
        if (gameManager.isGamePaused() || gameManager.isGameOver())
        {
            isButtonHeld = false;
            if (isAttacking())
            {
                setAttacking(false);
            }
            return;
        }

        base.Update();

        if (aimTargetTransform && currWeapon)
        {
            if (isButtonHeld && crosshair.activeSelf && !isAttacking()
                && gameManager.playerStats.checkStamina(getWeaponStaminaCost()))
            {
                gameManager.playerStats.useStamina(getWeaponStaminaCost());
                setAttacking(true);
            }
            else if (!currWeapon.useTriggered && !isButtonHeld && isAttacking())
            {
                setAttacking(false);
            }
            currWeapon.updateWeapon(Time.deltaTime, aimTargetTransform.position);
        }
    }

    void OnDestroy()
    {
        gameManager.OnGameOver -= onGameOver;
        playerStats.OnDeath -= onPlayerDeath;
        inputManager.OnAttackTriggered[(int)attackInput] -= onAttackTriggered;
        inputManager.OnAttackStarted[(int)attackInput] -= onAttackStarted;
        inputManager.OnAttackEnded[(int)attackInput] -= onAttackEnded;
    }

    // triggered by the weapon in the slot
    public override void onWeaponFire()
    {
        base.onWeaponFire();

        // the crosshair has been deactivated because the angle between the player forward and camera forward
        // is greater than the max aim angle
        if (!crosshair.activeSelf)
        {
            setAttacking(false);
            return;
        }

        if (!gameManager.playerStats.checkStamina(getWeaponStaminaCost()))
        {
            setAttacking(false);
            return;
        }

        gameManager.playerStats.useStamina(getWeaponStaminaCost());
    }

    // attack button pressed for this weapon slot
    private void onAttackTriggered()
    {
        if (gameManager.isGamePaused() || gameManager.isGameOver())
        {
            return;
        }

        // return if there is no weapon equipped or if weapon is a hold attack weapon vs a trigger
        if (!hasWeapon() || !currWeapon.useTriggered)
        {
            return;
        }

        if (!isAttacking() && gameManager.playerStats.checkStamina(getWeaponStaminaCost()))
        {
            gameManager.playerStats.useStamina(getWeaponStaminaCost());
            setAttacking(true);
        }
    }

    // begin a held attack
    private void onAttackStarted()
    {
        if (gameManager.isGamePaused() || gameManager.isGameOver())
        {
            return;
        }

        // return if there is no weapon equipped or if weapon is a trigger attack weapon vs a hold
        if (!hasWeapon() || currWeapon.useTriggered)
        {
            return;
        }

        isButtonHeld = true;
    }

    // end a held attack
    private void onAttackEnded()
    {
        if (gameManager.isGamePaused() || gameManager.isGameOver())
        {
            isButtonHeld = false;
            return;
        }

        if (!hasWeapon() || currWeapon.useTriggered)
        {
            return;
        }

        isButtonHeld = false;
    }

    // drop weapons when player dies
    private void onPlayerDeath(Vector3 direction, AudioSource audioSource)
    {
        dropWeapon(direction / 5f);
    }

    private void onGameOver(bool result)
    {
        if (result)
        {
            dropWeapon(Vector3.zero);
        }
    }

    private void delayedStart()
    {
        WeaponType weapon = PlayerLoadout.getWeapon(slotId);
        equipWeapon(new WeaponTypeUnion(weapon));
    }

    private new void equipWeapon(WeaponTypeUnion weaponType)
    {
        if (weaponType.hasWeaponType && weaponType.Weapon != WeaponType.None)
        {
            base.equipWeapon(weaponType);
            setAimTarget(crosshair.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Transform childTransform = transform.Find("AttackCollider");
        BoxCollider collider = childTransform.GetComponent<BoxCollider>();
        if (collider != null)
        {
            Matrix4x4 originalGizmosMatrix = Gizmos.matrix;

            // Set Gizmos matrix to match the transform (position, rotation, and scale) of the SkinnedMeshRenderer
            Gizmos.matrix = collider.transform.localToWorldMatrix;

            // Draw the bounds using the SkinnedMeshRenderer's local bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(collider.center, collider.size);

            // Restore the original Gizmos matrix
            Gizmos.matrix = originalGizmosMatrix;
        }
    }
}
