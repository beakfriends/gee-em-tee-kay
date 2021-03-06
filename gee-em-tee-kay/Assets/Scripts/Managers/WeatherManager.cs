using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherSettings
{
    public Color ambientLight;
    public Color sunlight;

    public Color skyboxColour;

    [Range(0.0f, 1.0f)]
    public float shadowStrength;

    [Range(0.0f, 2.0f)]
    public float ambientLightIntensity;

    [Range(-180, 180)]
    public float xRotation;

    [Range(-180, 180)]
    public float yRotation;

    public bool isRaining;

    public bool isThundering;

    public bool isWindy;
    
    public bool isWindowOpen;
}

public class WeatherManager : MonoBehaviour
{
    // ~ Begin debug
    [Range(1,5)]
    [SerializeField] private int debugDaySelector;
    // ~ End debug

    [SerializeField] private Vector2 lightningTimeRange;
    [SerializeField] private float timeBetweenThunderAndLightning;
    [SerializeField] private float lightningLerpTime;
    [SerializeField] private float lightningLerpOutTime;
    [SerializeField] private Vector2 lightningStrengthRange;
    [SerializeField] private Vector2 lightningAngleRange;
    [SerializeField] private Vector2 treeWindMinMax;
    [SerializeField] private List<WeatherSettings> weatherSettings = new List<WeatherSettings>();
    [SerializeField] private Light ambientLight;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light lightningObject;
    [SerializeField] private GameObject rainObject;
    [SerializeField] private GameObject windObject;
    [SerializeField] private GameObject normalLeaves;
    [SerializeField] private GameObject windyLeaves;
    [SerializeField] private Animator treeAnimator;
    [SerializeField] private AudioSource rainSounds;
    [SerializeField] private AudioSource thunderSound;
    [SerializeField] private AudioSource windSound;
    [SerializeField] private Transform window;
    [SerializeField] private Vector3 windowOpenPosition;
    [SerializeField] private Vector3 windowClosedPosition;
    [SerializeField] private Animator apartmentAnimator;

    private Coroutine lightningCoroutine;

    private int day = -1;

    void Start()
    {
        Global.dayManager.dayLoaded += UpdateWeather;

        day = -1;
    }

    void OnValidate()
    {
        // if (debugDaySelector > 0
        //     && ambientLight != null
        //     && sunLight != null
        //     && rainObject != null
        //     && windObject != null
        //     && lightningObject != null
        //     && normalLeaves != null
        //     && windyLeaves != null)
        // {
        //     UpdateWeather(debugDaySelector - 1);
        // }
        // else
        // {
        //     Debug.Log("Invalid OnValidate in WeatherManager!");
        //     return;
        // }
    }

    void UpdateWeather(int dayNo)
    {
        if (dayNo != day)
        {
            // Update ambient light
            ambientLight.color = weatherSettings[dayNo].ambientLight;
            ambientLight.intensity = weatherSettings[dayNo].ambientLightIntensity;

            // Update sunlight
            sunLight.color = weatherSettings[dayNo].sunlight;
            sunLight.shadowStrength = weatherSettings[dayNo].shadowStrength;

            sunLight.transform.eulerAngles = new Vector3(weatherSettings[dayNo].xRotation, weatherSettings[dayNo].yRotation, 0.0f);

            // Update skybox if game has started
            if (Global.cameraController != null)
            {
                Global.cameraController.SetBackgroundColour(weatherSettings[dayNo].skyboxColour);
            }

            // Update weather things
            if (weatherSettings[dayNo].isRaining)
            {
                rainObject.SetActive(true);

                StartCoroutine(AudioUtils.FadeIn(rainSounds, 2.0f));
            }
            else
            {
                rainObject.SetActive(false);

                StartCoroutine(AudioUtils.FadeOut(rainSounds, 0.5f));
            }

            if (weatherSettings[dayNo].isWindy)
            {
                windObject.SetActive(true);
                windyLeaves.SetActive(true);
                normalLeaves.SetActive(false);

                treeAnimator.SetLayerWeight(1, treeWindMinMax.y);

                StartCoroutine(AudioUtils.FadeIn(windSound, 2.0f, 0.2f));
            }
            else
            {
                windObject.SetActive(false);
                windyLeaves.SetActive(false);
                normalLeaves.SetActive(true);

                treeAnimator.SetLayerWeight(1, treeWindMinMax.x);
                
                StartCoroutine(AudioUtils.FadeOut(windSound, 0.5f));
            }

            if (weatherSettings[dayNo].isWindowOpen)
            {
                window.position = windowOpenPosition;
                apartmentAnimator.SetLayerWeight(1, 1.0f);
            }
            else
            {
                window.position = windowClosedPosition;
                apartmentAnimator.SetLayerWeight(1, 0.0f);
            }

            if (Global.hasStarted)
            {
                if (weatherSettings[dayNo].isThundering)
                {
                    lightningObject.enabled = true;
                    lightningObject.intensity = 0f;
                    lightningCoroutine = StartCoroutine(LightningStrikes());
                }
                else
                {
                    if (lightningCoroutine != null)
                    {
                        StopCoroutine(lightningCoroutine);
                    }

                    lightningObject.enabled = false;
                    lightningObject.intensity = 0f;
                }
            }
        }

        day = dayNo;
    }

    IEnumerator LightningStrikes()
    {
        while (Global.hasStarted)
        {
            yield return new WaitForSeconds(Random.Range(lightningTimeRange.x, lightningTimeRange.y));

            float timeCounter = 0f;

            float finalLighteningStrength = Random.Range(lightningStrengthRange.x, lightningStrengthRange.y);
            lightningObject.transform.eulerAngles = new Vector3(lightningObject.transform.eulerAngles.x, Random.Range(lightningAngleRange.x, lightningAngleRange.y),lightningObject.transform.eulerAngles.z);

            while (timeCounter < lightningLerpTime)
            {
                timeCounter += Time.deltaTime;

                lightningObject.intensity = Mathf.Lerp(0.0f, finalLighteningStrength, timeCounter/lightningLerpTime);

                yield return null;
            }

            timeCounter = lightningLerpOutTime;

            while (timeCounter > 0)
            {
                timeCounter -= Time.deltaTime;

                lightningObject.intensity = Mathf.Lerp(0.0f, finalLighteningStrength, timeCounter/lightningLerpOutTime);

                yield return null;
            }

            yield return new WaitForSeconds(timeBetweenThunderAndLightning - 1.0f);

            // Start thunder noise
            StartCoroutine(AudioUtils.FadeIn(thunderSound, 1.0f, 0.2f));

            yield return new WaitForSeconds(1.0f);
            Global.cameraController.ScreenShake(0.05f);
            yield return new WaitForSeconds(0.15f);
            Global.cameraController.ScreenShake(0.1f);
            yield return new WaitForSeconds(0.15f);
            Global.cameraController.ScreenShake(0.1f);
            yield return new WaitForSeconds(0.15f);
            Global.cameraController.ScreenShake(0.1f);
            yield return new WaitForSeconds(0.15f);
            Global.cameraController.ScreenShake(0.1f);
            yield return new WaitForSeconds(0.15f);
            Global.cameraController.ScreenShake(0.05f);

            StartCoroutine(AudioUtils.FadeOut(thunderSound, 2.0f));
        }
    }
}
