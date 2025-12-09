using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    public int maxAmmoInMag = 10;
    public int maxAmmoInStorage = 30;
    public float shootCooldown = 0.5f;
    public float reloadCooldown = 0.5f;
    private float switchCooldown = 0.5f;
    public float shootRange = 100f;

    public ParticleSystem impactEffect;

    public int currentAmmoInMag;
    public int currentAmmoInStorage;
    public int damager;
    public bool canShoot = true;
    public bool canSwitch = true;
    private bool isReloading = false;
    private float shootTimer;

    public Transform cartridgeEjectionPoint;
    public GameObject cartridgePrefab;
    public float cartridgeEjectionForce = 5f;

    public Animator gun;
    public ParticleSystem muzzleFlash;
    public GameObject muzzleFlashLight;
    public AudioSource shoot;

    void Start()
    {
        currentAmmoInMag = maxAmmoInMag;
        currentAmmoInStorage = maxAmmoInStorage;
        canSwitch = true;
        muzzleFlashLight.SetActive(false);
    }

    void Update()
    {
        currentAmmoInMag = Mathf.Clamp(currentAmmoInMag, 0, maxAmmoInMag);
        currentAmmoInStorage = Mathf.Clamp(currentAmmoInStorage, 0, maxAmmoInStorage);

        if (Input.GetButtonDown("Fire1") && canShoot && !isReloading)
        {
            switchCooldown = shootCooldown;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            switchCooldown = reloadCooldown;
            Reload();
        }

        if (shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        if (currentAmmoInMag > 0 && shootTimer <= 0f)
        {
            canSwitch = false;
            shoot.Play();
            muzzleFlash.Play();
            muzzleFlashLight.SetActive(true);
            gun.SetBool("shoot", true);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootRange))
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            GameObject cartridge = Instantiate(cartridgePrefab, cartridgeEjectionPoint.position, cartridgeEjectionPoint.rotation);
            Rigidbody cartridgeRigidbody = cartridge.GetComponent<Rigidbody>();

            cartridgeRigidbody.AddForce(cartridgeEjectionPoint.right * cartridgeEjectionForce, ForceMode.Impulse);

            StartCoroutine(endAnimations());
            StartCoroutine(endLight());
            StartCoroutine(canswitchshoot());

            switchCooldown -= Time.deltaTime;

            currentAmmoInMag--;

            shootTimer = shootCooldown;
        }
        else
        {
            Debug.Log("Cannot shoot");
        }
    }

    void Reload()
    {
        switchCooldown -= Time.deltaTime;
        if (isReloading || currentAmmoInStorage <= 0)
            return;

        int bulletsToReload = maxAmmoInMag - currentAmmoInMag;

        if (bulletsToReload > 0)
        {
            gun.SetBool("reload", true);
            StartCoroutine(endAnimations());

            int bulletsAvailable = Mathf.Min(bulletsToReload, currentAmmoInStorage);

            currentAmmoInMag += bulletsAvailable;
            currentAmmoInStorage -= bulletsAvailable;

            Debug.Log("Reloaded " + bulletsAvailable + " bullets");

            StartCoroutine(ReloadCooldown());
        }
        else
        {
            Debug.Log("Cannot reload");
        }
    }

    IEnumerator ReloadCooldown()
    {
        isReloading = true;
        canShoot = false;
        canSwitch = false;

        yield return new WaitForSeconds(reloadCooldown);

        isReloading = false;
        canShoot = true;
        canSwitch = true;
    }

    IEnumerator endAnimations()
    {
        yield return new WaitForSeconds(.1f);
        gun.SetBool("shoot", false);
        gun.SetBool("reload", false);
    }

    IEnumerator endLight()
    {
        yield return new WaitForSeconds(.1f);
        muzzleFlashLight.SetActive(false);
    }

    IEnumerator canswitchshoot()
    {
        yield return new WaitForSeconds(shootCooldown);
        canSwitch = true;
    }
}