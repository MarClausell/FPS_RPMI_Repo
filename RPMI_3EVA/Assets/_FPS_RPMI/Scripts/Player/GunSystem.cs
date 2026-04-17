using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam; //ref si disparamos desde el centro de la camara
    [SerializeField] Transform shootPoint; //ref si queremos disparar desde la punta del cañón
    [SerializeField] LayerMask impactLayer; //Layer con la que el Raycast interactua
    RaycastHit hit; //Almacén de informacion de los objetos a los que el Raycast puede impactar

    [Header("Weapon Parameters")]
    [SerializeField] int damage = 10; //Daño del arma por bala
    [SerializeField] float range = 100f; //Distancia de disparo
    [SerializeField] float spread = 0; //Radio de dispersion del arma
    [SerializeField] float shootingCooldown = 0.2f; //Tiempo entre disparos
    [SerializeField] float reloadTime = 1.5f; //Tiempo de recarga en segundos
    [SerializeField] bool allowButtonHold = false; //Si el disparo se ejecuta por clic (falso) o por mantener (true)

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30; //Cantidad max de balas/cargador
    [SerializeField] int bulletsPerTap = 1; //Cantidad de balas disparadas por cada ejecucion de disparo
    int bulletsLeft; //Cantidad de balas dentro del cargador actual

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect; //Ref al VFX de impacto de bala

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting; //indica si estamos disparando
    [SerializeField] bool canShoot; //indica si podemos disparar en x momento del juego
    [SerializeField] bool reloading; //indica si estamos en proceso de recarga

    #endregion

    private void Awake()
    {
        bulletsLeft = ammoSize; //Al iniciar la partida tenemos el cargador lleno
        canShoot = true; //Al iniciar la partida tenemos la posibilidad de disparar
    }

    

    // Update is called once per frame
    void Update()
    {
        //Condicion estricta de llamar a la rutina de disparo
        if (canShoot && shooting && !reloading && bulletsLeft > 0)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        //La corrutina se va a aencargar de medir el tiempo entre disparos y la gestión del gasto de balas
        //Ademas llamara al raycast de disparo que esta definido en Shoot()

        canShoot = false; //Llave de seguridad que hace que si estamos disparando no podamos disparar
        if (!allowButtonHold) shooting = false; //Cerrar el bucle de disparo por pulsacion
        for (int i = 0; i < bulletsPerTap; i++)
        {
            if (bulletsLeft <= 0) break; //Segunda prevencion de errores: si no me quedan balas no hago daño
            Shoot(); //Llamada al Raycast que define el disparo
            bulletsLeft--; //Resta 1 ala cantidad de balas del cargador actual
        }

        //Espera entre disparos 
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true; //Resetea la posibilidad de disparar
    }

    void Shoot()
    {
        //este es el metodo mas importante
        //AQUI SE DEFINE EL DISPARO POR RAYCAST = UTILIZABLE CON CUALQUIER MECANICA

        //Almacenar la direccion de disparo y modificarla en caso de haber spread
        Vector3 direction = fpsCam.transform.forward; //Se lanza rayo hacia delante de la camara
        //añadir dispersion aleatoria segun el valor de spread
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);

        //DECLARACION DEL RAYCAST
        //Physics.Raycast(Origen del rayo, dirección, almacén de la info del impacto, longitud del rayo, layer con la que impacta el rayo)
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, impactLayer)) //Mathf infiniti por range para balas infinitas
        {
            //AQUI PUEDO CODEAR TODOS LOS EFECTOS QUE QUIERO PARA MI INTERACCIÓN
            Debug.Log(hit.collider.name);
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    void Reload()
    {
        if (bulletsLeft < ammoSize && !reloading) StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true; //Estamos recargamdo, por lo tanto no podemos recargar
        //AQUI LLAMARAMOS A LA ANIMACION DE RECARGA
        yield return new WaitForSeconds(reloadTime); //Esperar tanto tiempo como dura la animacion de recarga
        bulletsLeft = ammoSize; //La cantidad de balas actuales se iguala a la maxima
        reloading = false; //termina la recarga, podemos volver a recargar
    }
   
    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (allowButtonHold)
        {
            shooting = context.ReadValueAsButton(); //Detecta constantemente si el boton de disparo esta apretado
        }
        else
        {
            if (context.performed) shooting = true; //Shooting solo es true por pulsación
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) Reload();
    }

    #endregion

}
