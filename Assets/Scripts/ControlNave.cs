using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlNave : MonoBehaviour
{
    ControlCombustible controlCombustible;
    ControlVida controlVida;
    ControlTiempo controlTiempo;
    AudioClip damage_sound_clip;
    Rigidbody rigidBody;
    Transform transForm;
    AudioSource[] sonidos;

    GameObject grupoEfecto;
    GameObject unicoEfecto;
    ParticleSystem propulsion;
    
    private float velRot = 20f;
    private float velPro = 30f;
    private float actualVida = 20f;
    private float actualComb = 40f;
    private float valorSumaV = 4f;
    private float valorSumaC = 14f;
    public float forceAmount = 1;
    void Start() 
    {         

        grupoEfecto = GameObject.Find("Efectos"); 
        controlCombustible = GameObject.Find("BarraCombustible").GetComponent<ControlCombustible>();
        controlVida = GameObject.Find("BarraVida").GetComponent<ControlVida>();
        controlTiempo = GameObject.Find("Tiempo").GetComponent<ControlTiempo>();
        propulsion = GameObject.Find("Propulsion").GetComponent<ParticleSystem>();
        rigidBody = GetComponent<Rigidbody>();
        transForm = GetComponent<Transform>(); 
        sonidos = GetComponents<AudioSource>();   
        if (GameObject.FindGameObjectWithTag("MusicaTag") != null)
        {
            GameObject.FindGameObjectWithTag("MusicaTag").GetComponent<ControlMusica>().PlayMusic();
        }        
        rigidBody.sleepThreshold = 0;
        // damage_sound_clip = AudioClip.Create("damage_sound",audiosource.clip.samples,audiosource.clip.channels,audiosource.clip.frequency,true);
    }
    
    void Update() 
    {            
        ProcesarInput();
        ProcesarTiempo();                        
    }
    
    private void OnTriggerEnter(Collider other){      

        switch(other.gameObject.tag) {
            case "ColisionRecarga": 
                sonidos[1].Play();
                controlCombustible.setValue(actualComb + valorSumaC);
                unicoEfecto = Instantiate(grupoEfecto.transform.GetChild(0).gameObject, other.transform.position, other.transform.rotation);                
                unicoEfecto.GetComponent<ParticleSystem>().Play();
                Destroy(other.gameObject);                
                break;

            case "ColisionVida":
                sonidos[1].Play();               
                controlVida.setValue(actualVida + valorSumaV);
                unicoEfecto = Instantiate(grupoEfecto.transform.GetChild(1).gameObject, other.transform.position, other.transform.rotation);
                unicoEfecto.GetComponent<ParticleSystem>().Play();
                Destroy(other.gameObject);                
                break;                 
        } 
    }

    private void OnCollisionEnter(Collision collision) 
    {        
        switch(collision.gameObject.tag) {
            case "ColisionNivel":
                sonidos[3].Play();
                rigidBody.isKinematic = true; 
                unicoEfecto = Instantiate(grupoEfecto.transform.GetChild(3).gameObject, collision.transform.position, collision.transform.rotation);
                unicoEfecto.GetComponent<ParticleSystem>().Play();            
                controlTiempo.setOff();               
                StartCoroutine(EsperarSiguiente()); 
                break;
            
        }
    }

    private void OnCollisionStay(Collision collision) 
    {          
        switch(collision.gameObject.tag) {            
            case "ColisionPeligrosa":                                                                 
                if(!controlVida.getEstado() && (controlVida.getValue() <= 4 || controlCombustible.getValue() <= 0)) {
                    Destruccion();
                } else {
                    controlVida.getDamage(); 
                    actualVida = controlVida.getValue();                 
                }
                break;        
        }
    }
    
    private void ProcesarInput() {
        Propulsion();
        Rotacion();
    }

    private void ProcesarTiempo() {
        if(controlTiempo.getTiempo() <= 0) {
            controlVida.stopParpa();
            if(!controlVida.getEstadoAnim()) {
                Destruccion();
            }
        }
    }

    private void Destruccion() {
        controlTiempo.StartCoroutine(EsperarReinicio());   
        controlTiempo.explosion();
        controlVida.setValue(0f);                                                
        unicoEfecto = Instantiate(grupoEfecto.transform.GetChild(2).gameObject, transform.position, transform.rotation);
        unicoEfecto.GetComponent<ParticleSystem>().Play();
        actualVida = 0;         
        Destroy(gameObject);   
    }

    private IEnumerator EsperarSiguiente() {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator EsperarReinicio(){
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Propulsion() 
    {
        if (Input.GetKey(KeyCode.Space) && controlCombustible.getValue() > 0) 
        {
            // rigidBody.AddRelativeForce(Vector3.up * velPro);  
            rigidBody.AddRelativeForce(Vector3.up*forceAmount,ForceMode.Impulse);                                  
            controlCombustible.decrementValue(); 
            actualComb = controlCombustible.getValue();
            propulsion.Play();            
            if(!sonidos[0].isPlaying) 
            {
                sonidos[0].Play();
            }            
        } 
        else 
        {
            sonidos[0].Stop();
            propulsion.Stop();
        }        
       
    }

    private void Rotacion() {
        if(Input.GetKey(KeyCode.D)) 
        {                        
            rigidBody.AddRelativeTorque(Vector3.back * velRot, ForceMode.Acceleration);
        }
        else if(Input.GetKey(KeyCode.A)) 
        {                       
            rigidBody.AddRelativeTorque(Vector3.forward * velRot, ForceMode.Acceleration);
        }
    }
}













