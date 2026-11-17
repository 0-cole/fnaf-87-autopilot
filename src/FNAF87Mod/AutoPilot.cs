using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FNAF87Mod
{
    public class AutoPilot : MonoBehaviour
    {
        public static bool BotActive = true;
        public static bool ShowHUD = true;
        public static AutoPilot Instance;

        private OfficeLightScript ols;
        private Mask maskController;
        private PuppetScript puppet;
        private FoxyBrainScript foxy;
        private BBVentScript[] bbVents;
        private ToyFreddyPositionScript[] tfPositions;
        private BreakerScript[] breakers;
        private OfficeCharacterScript[] officeChars;
        private VentCharacter[] ventChars;
        private MangleHallScript mangle;
        private ShadowBonnieMainScript shadowBonnie;
        private LightManagerScript lightManager;
        private ClockScript clock;

        private float refreshTimer = 0f;
        private List<string> activeThreats = new List<string>();
        private bool lastThreatState = false;

        public static void Attach(OfficeLightScript targetOls)
        {
            if (targetOls == null) return;
            if (targetOls.gameObject.GetComponent<AutoPilot>() == null)
            {
                var ap = targetOls.gameObject.AddComponent<AutoPilot>();
                ap.ols = targetOls;
                Debug.Log("[FNAF87 AutoPilot] Successfully attached to OfficeLightScript.");
            }
        }

        private void Awake()
        {
            Instance = this;
            if (ols == null) ols = GetComponent<OfficeLightScript>();
            RefreshReferences();
            Debug.Log(string.Format("[FNAF87 AutoPilot] Initialized: Office={0}, Vents={1}, Breakers={2}, Puppet={3}, Foxy={4}, BB={5}",
                (officeChars != null ? officeChars.Length : 0),
                (ventChars != null ? ventChars.Length : 0),
                (breakers != null ? breakers.Length : 0),
                (puppet != null),
                (foxy != null),
                (bbVents != null ? bbVents.Length : 0)));
        }

        private void RefreshReferences()
        {
            if (ols == null) ols = FindObjectOfType<OfficeLightScript>();
            maskController = FindObjectOfType<Mask>();
            puppet = FindObjectOfType<PuppetScript>();
            foxy = FindObjectOfType<FoxyBrainScript>();
            bbVents = FindObjectsOfType<BBVentScript>();
            tfPositions = FindObjectsOfType<ToyFreddyPositionScript>();
            breakers = FindObjectsOfType<BreakerScript>();
            officeChars = FindObjectsOfType<OfficeCharacterScript>();
            ventChars = FindObjectsOfType<VentCharacter>();
            mangle = FindObjectOfType<MangleHallScript>();
            shadowBonnie = FindObjectOfType<ShadowBonnieMainScript>();
            lightManager = FindObjectOfType<LightManagerScript>();
            clock = FindObjectOfType<ClockScript>();
        }

        private void Update()
        {
            // Title screen shortcut: Press N to jump straight into night
            if (Input.GetKeyDown(KeyCode.N) && SceneManager.GetActiveScene().name.ToLower().Contains("title"))
            {
                Debug.Log("[FNAF87 AutoPilot] Quick-loading Nights scene via [N] shortcut.");
                SceneManager.LoadScene("Nights");
                return;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                BotActive = !BotActive;
                Debug.Log("[FNAF87 AutoPilot] BotActive = " + BotActive);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                ShowHUD = !ShowHUD;
            }

            if (!BotActive) return;

            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 1.5f)
            {
                refreshTimer = 0f;
                RefreshReferences();
            }

            // 1. Omniscient Immortality Guard
            OfficeLightScript.dead = false;
            if (ols != null) ols.deths = 0;

            // 2. Office & Vent Threat Detection + Auto-Masking
            activeThreats.Clear();
            bool threatPresent = false;

            if (officeChars != null)
            {
                for (int i = 0; i < officeChars.Length; i++)
                {
                    var oc = officeChars[i];
                    if (oc == null) continue;

                    if (oc.killed)
                    {
                        oc.killed = false;
                        oc.dietimer = 2f;
                    }

                    if (oc.inoffice || oc.active)
                    {
                        threatPresent = true;
                        activeThreats.Add(oc.gameObject.name + " (Office)");
                    }
                }
            }

            if (ventChars != null)
            {
                for (int i = 0; i < ventChars.Length; i++)
                {
                    var vc = ventChars[i];
                    if (vc == null) continue;

                    if (vc.killed)
                    {
                        vc.killed = false;
                        vc.dietimer = 2f;
                    }

                    if (vc.active)
                    {
                        threatPresent = true;
                        activeThreats.Add(vc.gameObject.name + " (Vent)");
                    }
                }
            }

            if (mangle != null)
            {
                if (mangle.killed)
                {
                    mangle.killed = false;
                    mangle.dietimer = 2f;
                }
                if (mangle.Down)
                {
                    threatPresent = true;
                    activeThreats.Add("Mangle (Ceiling)");
                }
            }

            // Sync Mask State
            if (threatPresent)
            {
                OfficeLightScript.mask = true;
                if (ols != null && ols.lowpass != null) ols.lowpass.cutoffFrequency = 3000f;

                if (!lastThreatState)
                {
                    lastThreatState = true;
                    Debug.Log("[FNAF87 AutoPilot] Threat detected! Equipping security mask.");
                    if (maskController != null && maskController.mask != null)
                    {
                        maskController.mask.SetTrigger("Toggle");
                    }
                    else if (ols != null && ols.maskanim != null)
                    {
                        ols.maskanim.SetTrigger("Toggle");
                    }
                }
            }
            else
            {
                if (lastThreatState)
                {
                    lastThreatState = false;
                    OfficeLightScript.mask = false;
                    if (ols != null && ols.lowpass != null) ols.lowpass.cutoffFrequency = 22000f;
                    Debug.Log("[FNAF87 AutoPilot] All threats cleared. Lowering security mask.");
                    if (maskController != null && maskController.mask != null)
                    {
                        maskController.mask.SetTrigger("Toggle");
                    }
                    else if (ols != null && ols.maskanim != null)
                    {
                        ols.maskanim.SetTrigger("Toggle");
                    }
                }
            }

            // 3. Puppet Master (Auto-Winder)
            if (puppet != null && puppet.enabled)
            {
                puppet.timer = 100f;
                puppet.deathTimer = 40f;
                if (puppet.progress != null)
                {
                    puppet.progress.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }

            // 4. Withered Foxy Auto-Repel
            if (foxy != null && foxy.enabled)
            {
                foxy.Death = 40f;
                if (foxy.Active)
                {
                    foxy.flash9 = true;
                    activeThreats.Add("Foxy (Repelling)");
                }
            }

            // 5. Toy Freddy Switchboard Guardian
            if (tfPositions != null)
            {
                for (int i = 0; i < tfPositions.Length; i++)
                {
                    var tf = tfPositions[i];
                    if (tf == null) continue;

                    if (tf.Arrived)
                    {
                        activeThreats.Add("Toy Freddy (Breaker)");
                        tf.flash9 = true;
                        tf.OutageTimer = 40f;
                        if (tf.Freddy != null) tf.Freddy.SetActive(false);
                        tf.Arrived = false;
                        if (tf.brain != null)
                        {
                            tf.brain.Moving = true;
                            tf.brain.MoveTimer = UnityEngine.Random.Range(60f, 90f);
                        }
                        Debug.Log("[FNAF87 AutoPilot] Repelled Toy Freddy at switchboard!");
                    }
                }
            }

            // 6. Breakers & Power Outage Auto-Restore
            if (breakers != null)
            {
                for (int i = 0; i < breakers.Length; i++)
                {
                    var b = breakers[i];
                    if (b == null) continue;

                    b.Switches = 14;
                    if (b.warning != null) b.warning.SetActive(false);

                    if (b.switcheslist != null)
                    {
                        for (int s = 0; s < b.switcheslist.Length; s++)
                        {
                            var sw = b.switcheslist[s];
                            if (sw != null && !sw.on)
                            {
                                sw.on = true;
                                if (sw.anim != null) sw.anim.SetBool("On", true);
                            }
                        }
                    }
                }
            }

            if (lightManager != null)
            {
                lightManager.Power = true;
                if (lightManager.PowerFreddy != null && lightManager.PowerFreddy.activeInHierarchy)
                {
                    lightManager.PowerFreddy.SetActive(false);
                }
                if (lightManager.anim != null)
                {
                    lightManager.anim.SetBool("PowerOutage", false);
                }
                if (lightManager.BreakerUI != null)
                {
                    lightManager.BreakerUI.SetBool("BreakerActive", false);
                }
            }

            // 7. Balloon Boy Auto-Banish
            if (bbVents != null)
            {
                for (int i = 0; i < bbVents.Length; i++)
                {
                    var bb = bbVents[i];
                    if (bb == null) continue;

                    if (bb.Active)
                    {
                        activeThreats.Add("Balloon Boy (Banishing)");
                        bb.flash3 = true;
                        bb.LeaveBB();
                        bb.Active = false;
                        bb.timer = 40f;
                        Debug.Log("[FNAF87 AutoPilot] Banished Balloon Boy from vent!");
                    }
                }
            }

            // 8. Shadow Bonnie Neutralizer
            if (shadowBonnie != null)
            {
                shadowBonnie.deathtimer = 60f;
            }
        }

        private GUIStyle boxStyle;
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private Texture2D bgTexture;

        private void InitGUIStyles()
        {
            if (bgTexture == null)
            {
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.12f, 0.88f));
                bgTexture.Apply();
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = bgTexture;
                boxStyle.border = new RectOffset(4, 4, 4, 4);
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 14;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = new Color(0f, 1f, 0.75f);
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 12;
                labelStyle.normal.textColor = Color.white;
            }
        }

        private void OnGUI()
        {
            if (!ShowHUD) return;
            InitGUIStyles();

            float width = 360f;
            float height = 280f;
            Rect panelRect = new Rect(16, 16, width, height);

            GUI.Box(panelRect, GUIContent.none, boxStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 12, panelRect.y + 10, panelRect.width - 24, panelRect.height - 20));

            GUILayout.Label("✦ FNAF '87 OMNISCIENT AUTO-PILOT ✦", headerStyle);
            GUILayout.Space(2);

            string statusColor = BotActive ? "#00FF66" : "#FF4444";
            string statusText = BotActive ? "ACTIVE (AUTO-DEFENDING)" : "STANDBY (MANUAL)";
            GUILayout.Label(string.Format("Bot Status: <b><color={0}>{1}</color></b>", statusColor, statusText), labelStyle);
            GUILayout.Label("<color=#AAAAAA>Controls: [B] Toggle Auto-Pilot | [H] Toggle HUD</color>", labelStyle);

            GUILayout.Space(6);

            // Night & Clock info
            string hourText = (clock != null) ? string.Format("{0:00}:00 (Night {1})", (int)clock.Hours, BrainScript.night) : "Night " + BrainScript.night;
            GUILayout.Label(string.Format("Time / Shift: <color=#FFFF66>{0}</color>", hourText), labelStyle);

            // Puppet
            string puppetText = (puppet != null && puppet.enabled) ? string.Format("{0:F0}% (Wound)", puppet.timer) : "Disabled";
            GUILayout.Label(string.Format("Puppet Music Box: <color=#66CCFF>{0}</color>", puppetText), labelStyle);

            // Foxy
            string foxyText = (foxy != null && foxy.enabled) ? (foxy.Active ? "<color=#FF6666>REPELLING</color>" : "<color=#88FF88>Passive</color>") : "Disabled";
            GUILayout.Label(string.Format("Hallway Foxy: {0}", foxyText), labelStyle);

            // Breakers
            GUILayout.Label("Breakers: <color=#88FF88>56/56 Switches OK (Toy Freddy Guarded)</color>", labelStyle);

            // Mask status
            string maskStr = OfficeLightScript.mask ? "<color=#FFCC00>ON (PROTECTED)</color>" : "<color=#88FF88>OFF (CLEAR)</color>";
            GUILayout.Label(string.Format("Security Mask: {0}", maskStr), labelStyle);

            GUILayout.Space(4);

            // Threat List
            if (activeThreats.Count > 0)
            {
                GUILayout.Label("<color=#FF4444><b>Active Threats Intercepted:</b></color>", labelStyle);
                for (int i = 0; i < activeThreats.Count; i++)
                {
                    GUILayout.Label(string.Format(" • <color=#FFAA00>{0}</color>", activeThreats[i]), labelStyle);
                }
            }
            else
            {
                GUILayout.Label("Threats: <color=#88FF88>All Office & Vents Clear</color>", labelStyle);
            }

            GUILayout.EndArea();
        }
    }
}
