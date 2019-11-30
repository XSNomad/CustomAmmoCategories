﻿using BattleTech;
using CustAmmoCategories;
using Harmony;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomUnits {
  public class CustomHardpointDef {
    public CustomVector offset { get; set; }
    public CustomVector scale { get; set; }
    public CustomVector rotate { get; set; }
    public float prefireAnimationLength { get; set; }
    public float fireAnimationLength { get; set; }
    public string prefab { get; set; }
    public string shaderSrc { get; set; }
    public List<string> keepShaderIn { get; set; }
    public string positionSrc { get; set; }
    public List<string> emitters { get; set; }
    public string preFireAnimation { get; set; }
    public List<string> fireEmitterAnimation { get; set; }
    public CustomHardpointDef() {
      emitters = new List<string>();
      fireEmitterAnimation = new List<string>();
      keepShaderIn = new List<string>();
      offset = new CustomVector(false);
      scale = new CustomVector(true);
      rotate = new CustomVector(false);
      shaderSrc = string.Empty;
      positionSrc = string.Empty;
      prefireAnimationLength = 1f;
      fireAnimationLength = 1.5f;
    }
  };
  public class HadrpointAlias {
    public string name { get; set; }
    public string prefab { get; set; }
    public string location { get; set; }
  }
  public class CustomHardpoints {
    public List<CustomHardpointDef> prefabs { get; set; }
    public Dictionary<string, HadrpointAlias> aliases { get; set; }
    public CustomHardpoints() {
      prefabs = new List<CustomHardpointDef>();
      aliases = new Dictionary<string, HadrpointAlias>();
    }
  }
  [HarmonyPatch(typeof(HardpointDataDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class HardpointDataDef_FromJSON {
    public static bool Prefix(HardpointDataDef __instance, ref string json, ref CustomHardpoints __state) {
      __state = null;
      try {
        JObject definition = JObject.Parse(json);
        string id = (string)definition["ID"];
        Log.LogWrite(id + "\n");
        if (definition["CustomHardpoints"] != null) {
          __state = definition["CustomHardpoints"].ToObject<CustomHardpoints>();
          foreach (CustomHardpointDef chd in __state.prefabs) {
            CustomHardPointsHelper.Add(chd.prefab, chd);
          }
          foreach (var alias in __state.aliases) {
            alias.Value.name = alias.Key;
            CustomHardPointsHelper.Add(alias.Value.name, alias.Value.prefab);
          }
          definition.Remove("CustomHardpoints");
        }
        json = definition.ToString();
        return true;
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\nIN:" + json + "\n");
        return true;
      }
    }
    public static void Postfix(HardpointDataDef __instance, ref CustomHardpoints __state) {
      if (__state == null) { return; }
      foreach (var alias in __state.aliases) {
        int index = -1;
        for (int i = 0; i < __instance.HardpointData.Length; ++i) {
          if (__instance.HardpointData[i].location == alias.Value.location) { index = i; break; };
        }
        if (index == -1) { continue; }
        int hindex = 0;
        try {
          hindex = int.Parse(alias.Key.Substring(alias.Key.Length - 1)) - 1;
        } catch (Exception e) {
          Log.LogWrite(e.ToString() + "\n", true);
        }
        if (hindex < 0) { continue; }
        List<string> tmp = __instance.HardpointData[index].weapons[hindex].ToList();
        tmp.Add(alias.Key);
        __instance.HardpointData[index].weapons[hindex] = tmp.ToArray();
      }
      Log.LogWrite(JsonConvert.SerializeObject(__instance, Formatting.Indented) + "\n");
    }
  }
  public class HardPointAnimationController : BaseHardPointAnimationController {
    public CustomHardpointDef customHardpoint { get; set; }
    private bool PrefireCompleete { get; set; }
    public Animator animator { get; set; }
    public Weapon weapon { get; set; }
    private float PrefireCompleeteCounter { get; set; }
    private List<float> fireCompleeteCounters;
    private List<bool> fireCompleete;
    private float PrefireSpeed { get; set; }
    private float FireSpeed { get; set; }
    public override void PrefireAnimationSpeed(float speed) {
      if (animator == null) { return; }
      PrefireSpeed = speed;
      animator.SetFloat("prefire_speed", speed);
    }
    public override void FireAnimationSpeed(float speed) {
      if (animator == null) { return; }
      FireSpeed = speed;
      animator.SetFloat("fire_speed",speed);
    }
    public HardPointAnimationController() {
      fireCompleeteCounters = new List<float>();
      fireCompleete = new List<bool>();
      FireSpeed = 1f;
      PrefireSpeed = 1f;
    }
    public override bool isPrefireAnimCompleete() { return PrefireCompleete; }
    private void fireCompleeteAll(bool state) {
      for (int t = 0; t < fireCompleete.Count; ++t) { fireCompleete[t] = true; }
    }
    private void fireCompleeteCountersAll() {
      for (int t = 0; t < fireCompleeteCounters.Count; ++t) { fireCompleeteCounters[t] = 0f; }
    }
    public override void FireAnimation(int index) {
      if (animator == null) { fireCompleeteAll(true); return; }
      if (customHardpoint == null) { fireCompleeteAll(true); return; }
      if (fireCompleete.Count == 0) { return; };
      int realIndex = index % fireCompleete.Count;
      Log.LogWrite("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]HardPointAnimationController.FireAnimation(" + index + "/"+realIndex+"):" + customHardpoint.prefab + "\n");
      string animName = customHardpoint.fireEmitterAnimation[realIndex];
      if (string.IsNullOrEmpty(animName) == false) {
        animator.SetBool(animName,true);
        Log.LogWrite(1, "animName("+realIndex+"):" + animName + " true\n");
        fireCompleete[realIndex] = false;
        fireCompleeteCounters[realIndex] = (FireSpeed > 0.01f)?customHardpoint.fireAnimationLength / FireSpeed:0f;
      } else {
        Log.LogWrite(1, "animName(" + realIndex + "):" + animName + " false\n");
        fireCompleete[realIndex] = true;
        fireCompleeteCounters[realIndex] = 0f;
      }
      for (int t = 0; t < customHardpoint.fireEmitterAnimation.Count; ++t) {
        if (t == realIndex) { continue; }
        string tanimName = customHardpoint.fireEmitterAnimation[t];
        if (string.IsNullOrEmpty(tanimName) == false) {
          Log.LogWrite(1, "animName(" + t + "):" + tanimName + " - false\n");
          animator.SetBool(tanimName, false);
          fireCompleete[t] = true;
          fireCompleeteCounters[t] = 0f;
        }
      }
    }
    public override bool isFireAnimCompleete(int index) {
      if (fireCompleete.Count == 0) { return true; }
      return fireCompleete[index % fireCompleete.Count];
    }
    public override void PrefireAnimation() {
      if (animator == null) { PrefireCompleete = true; return; }
      if (customHardpoint == null) { PrefireCompleete = true; return; }
      if (string.IsNullOrEmpty(customHardpoint.preFireAnimation)) { PrefireCompleete = true; return; }
      PrefireCompleeteCounter = (PrefireSpeed > 0.01f) ? customHardpoint.prefireAnimationLength / PrefireSpeed : 0f;
      PrefireCompleete = false;
      Log.LogWrite("HardPointAnimationController.PrefireAnimation " + weapon.defId + "\n");
      if (animator != null) { animator.SetBool(customHardpoint.preFireAnimation, true); }
    }
    public override void PostfireAnimation() {
      if (animator == null) { PrefireCompleete = true; return; }
      if (customHardpoint == null) { PrefireCompleete = true; return; }
      if (string.IsNullOrEmpty(customHardpoint.preFireAnimation)) { PrefireCompleete = true; return; }
      PrefireCompleete = true;
      animator.SetBool(customHardpoint.preFireAnimation, false);
      for (int t = 0; t < customHardpoint.fireEmitterAnimation.Count; ++t) {
        string animName = customHardpoint.fireEmitterAnimation[t];
        if (string.IsNullOrEmpty(animName) == false) {
          Log.LogWrite(1, "animName(" + t + "):" + animName + " - false\n");
          animator.SetBool(animName, false);
          fireCompleete[t] = true;
          fireCompleeteCounters[t] = 0f;
        }
      }
    }
    public void Update() {
      if (PrefireCompleete == false) {
        if (PrefireCompleeteCounter > 0f) { PrefireCompleeteCounter -= Time.deltaTime; } else { PrefireCompleete = true; };
      }
      for(int t = 0; t < fireCompleeteCounters.Count; ++t) {
        if (fireCompleete[t] == false) { if (fireCompleeteCounters[t] > 0f) { fireCompleeteCounters[t] -= Time.deltaTime; } else {
            Log.LogWrite("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] animation " + t + " finished\n");
            fireCompleete[t] = true;
        }; };
      }
    }
    public void OnPrefireCompleete(int i) {
      Log.LogWrite("HardPointAnimationController.OnPrefireCompleete " + i + "  " + weapon.defId + "\n", true);
      PrefireCompleete = true;
    }
    public void Init(WeaponRepresentation weaponRep) {
      this.Init(weaponRep, CustomHardPointsHelper.Find(weapon.baseComponentRef.prefabName));
    }
    public void Init(WeaponRepresentation weaponRep, CustomHardpointDef hardpointDef) {
      Log.LogWrite(0, "HardPointAnimationController.Init " + weaponRep.name + "\n");
      weapon = weaponRep.weapon;
      PrefireCompleete = false;
      FireSpeed = 1f;
      PrefireSpeed = 1f;
      animator = weaponRep.gameObject.GetComponentInChildren<Animator>();
      if (animator == null) { PrefireCompleete = true; };
      customHardpoint = hardpointDef;
      Log.LogWrite(1, "customHardpoint: " + ((customHardpoint == null) ? "null" : customHardpoint.prefab) + "\n");
      if (animator != null) {
        Log.LogWrite(1, "clips(" + animator.runtimeAnimatorController.animationClips.Length + "):\n");
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
          Log.LogWrite(2, "clip:" + clip.name + "\n");
        }
      }
      if(customHardpoint != null) {
        fireCompleete.Clear();
        fireCompleeteCounters.Clear();
        foreach (string name in customHardpoint.fireEmitterAnimation) {
          fireCompleete.Add(true);
          fireCompleeteCounters.Add(0f);
        }
      }
      HardpointAnimatorHelper.RegisterHardpointAnimator(weapon, this);
    }
  }
  public static class CustomHardPointsHelper {
    private static Dictionary<string, string> hardPointAliases = new Dictionary<string, string>();
    private static Dictionary<string, CustomHardpointDef> CustomHardpointsDef = new Dictionary<string, CustomHardpointDef>();
    public static HardPointAnimationController hadrpointAnimator(this WeaponRepresentation weaponRep) {
      HardPointAnimationController result = weaponRep.gameObject.GetComponent<HardPointAnimationController>();
      if (result == null) { result = weaponRep.gameObject.AddComponent<HardPointAnimationController>(); result.Init(weaponRep); };
      return result;
    }
    public static void Add(string name, CustomHardpointDef def) {
      CustomHardpointsDef.Add(name, def);
    }
    public static void Add(string name, string prefab) {
      if (hardPointAliases.ContainsKey(name) == false) { hardPointAliases.Add(name, prefab); return; };
      hardPointAliases[name] = prefab;
    }
    public static CustomHardpointDef Find(string name) {
      if (CustomHardpointsDef.ContainsKey(name) == false) { return null; };
      return CustomHardpointsDef[name];
    }
    public static string Alias(string name) {
      if (hardPointAliases.ContainsKey(name) == false) { return name; };
      return hardPointAliases[name];
    }
  }
  [HarmonyPatch(typeof(MechHardpointRules))]
  [HarmonyPatch("GetComponentPrefabName")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechHardpointRules_GetComponentPrefabName {
    /*public static bool Prefix(HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames, ref string __result) {
      Log.LogWrite(0, "MechHardpointRules.GetComponentPrefabName hardpointDataDef:" + hardpointDataDef.ID + " weapon:" + componentRef.ComponentDefID + " prefabBase:" + prefabBase + " location: " + location + "\n");
      if (componentRef.HardpointSlot < 0) { __result = ""; return false; }
      WeaponDef def = componentRef.Def as WeaponDef;
      string desiredPrefabName = "";
      string lower = componentRef.Def.PrefabIdentifier.ToLower();
      string hardpointStr = "";
      if (def == null) { __result = ""; return false; }
      WeaponCategory category = def.Category;
      WeaponSubType weaponSubType = def.WeaponSubType;
      switch (category) {
        case WeaponCategory.Ballistic:
          hardpointStr = "_bh";
          break;
        case WeaponCategory.Energy:
          hardpointStr = "_eh";
          break;
        case WeaponCategory.Missile:
          hardpointStr = "_mh";
          break;
        case WeaponCategory.AntiPersonnel:
          hardpointStr = "_ah";
          break;
        case WeaponCategory.Melee:
          __result = "chrPrfWeap_generic_melee"; return false;
      }
      desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", (object)prefabBase, (object)location, (object)lower, (object)hardpointStr);
      Log.LogWrite(1, "desiredPrefabName:"+ desiredPrefabName + "\n");
      List<string> availableNames = new List<string>();
      HardpointDataDef._WeaponHardpointData weaponHardpointData = new HardpointDataDef._WeaponHardpointData("", new string[0][], new string[0], new string[0]);
      for (int index1 = 0; index1 < hardpointDataDef.HardpointData.Length; ++index1) {
        if (hardpointDataDef.HardpointData[index1].location == location) {
          weaponHardpointData = hardpointDataDef.HardpointData[index1];
          for (int index2 = 0; index2 < weaponHardpointData.weapons.Length; ++index2) {
            List<string> stringList = new List<string>((IEnumerable<string>)weaponHardpointData.weapons[index2]);
            bool flag = true;
            for (int index3 = 0; index3 < usedPrefabNames.Count; ++index3) {
              if (stringList.Contains(usedPrefabNames[index3])) {
                flag = false;
                break;
              }
            }
            if (flag)
              availableNames.AddRange((IEnumerable<string>)stringList);
          }
          break;
        }
      }
      if (category == WeaponCategory.AntiPersonnel) {
        availableNames.RemoveAll((Predicate<string>)(x => {
          if (!x.Contains(hardpointStr) && !x.Contains("_laser_eh") && !x.Contains("_flamer_eh"))
            return !x.Contains("_mg_bh");
          return false;
        }));
      } else {
        availableNames.RemoveAll((Predicate<string>)(x => !x.Contains(hardpointStr)));
      }
      //availableNames = availableNames.ConvertAll(d => d.ToLower());
      List<string> stringList1 = new List<string>((IEnumerable<string>)availableNames);
      Log.LogWrite(1, "availableNames(" + availableNames.Count + "):\n");foreach (var name in availableNames) { Log.LogWrite(2, name + "\n"); }
      stringList1.RemoveAll((Predicate<string>)(x => !x.Contains(desiredPrefabName)));
      if (stringList1.Count < 1) {
        desiredPrefabName = desiredPrefabName.ToLower();
        Log.LogWrite(1, "Can't find. "+desiredPrefabName+". Try lower case\n");
        stringList1.AddRange((IEnumerable<string>)availableNames);
        stringList1.RemoveAll((Predicate<string>)(x => !x.Contains(desiredPrefabName)));
      }

      if (stringList1.Count < 1) {
        Log.LogWrite(1, "Can't find fallback. Try default\n");
        switch (category) {
          case WeaponCategory.Ballistic:
            stringList1 = MechHardpointRules.GetFallbackBallisticNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.Energy:
            stringList1 = MechHardpointRules.GetFallbackEnergyNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.Missile:
            stringList1 = MechHardpointRules.GetFallbackMissileNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.AntiPersonnel:
            stringList1 = MechHardpointRules.GetFallbackSmallNames(availableNames, weaponSubType, lower, hardpointStr);
            break;
        }
      }
      Log.LogWrite(1, "stringList1(" + stringList1.Count + "):\n"); foreach (var name in stringList1) { Log.LogWrite(2, name + "\n"); }
      int num1 = int.MaxValue;
      for (int index = 0; index < stringList1.Count; ++index) {
        int num2 = int.Parse(stringList1[index].Substring(stringList1[index].Length - 1));
        if (num2 < num1) {
          desiredPrefabName = stringList1[index];
          num1 = num2;
        }
      }
      if (num1 < int.MaxValue) {
        usedPrefabNames.Add(desiredPrefabName);
        __result = desiredPrefabName; return false;
      }
      if (availableNames.Count > 0) {
        usedPrefabNames.Add(availableNames[0]);
        __result = availableNames[0]; return false;
      }
      if (weaponHardpointData.weapons.Length != 0 && weaponHardpointData.weapons[0].Length != 0) {
        if (!usedPrefabNames.Contains(weaponHardpointData.weapons[0][0]))
          usedPrefabNames.Add(weaponHardpointData.weapons[0][0]);
        __result = weaponHardpointData.weapons[0][0]; return false;
      }
      Log.LogWrite("GetComponentPrefabName failed to find a prefab name for unit " + prefabBase + " and component " + componentRef.Def.Description.Id + ", ideal match name was: " + desiredPrefabName + ", falling back to default item\n", true);
      __result = hardpointDataDef.HardpointData[0].weapons[0][0];
      return false;
    }*/
    public static void Postfix(HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames, ref string __result) {
      Log.WL(0, "MechHardpointRules.GetComponentPrefabName "+componentRef.ComponentDefID+" "+__result);
      if (string.IsNullOrEmpty(__result)) { return; }
      __result = CustomHardPointsHelper.Alias(__result);
      Log.WL(1, " custom hardpoint found: prefab replacing:"+__result);
    }
  }

  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("InitGameRep")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(Transform), typeof(string) })]
  public static class Weapon_InitGameRep {
    public static void printComponents(this GameObject obj, int level) {
      Component[] components = obj.GetComponents<Component>();
      Log.LogWrite(level, "object:" + obj.name + "\n");
      Log.LogWrite(level, "components(" + components.Length + "):\n");
      foreach (Component component in components) {
        Log.LogWrite(level + 1, component.name + ":" + component.GetType().ToString() + "\n");
      }
      Log.LogWrite(level, "childs(" + obj.transform.childCount + "):\n");
      for (int t = 0; t < obj.transform.childCount; ++t) {
        obj.transform.GetChild(t).gameObject.printComponents(level + 1);
      }
    }
    public static bool Prefix(Weapon __instance, string prefabName, Transform parentBone, string parentDisplayName, CombatGameState ___combat) {
      Log.LogWrite(0, "Weapon.InitGameRep: " + __instance.defId + ":" + prefabName + "\n");
      try {
        if (string.IsNullOrEmpty(prefabName)) { return true; }
        WeaponRepresentation component = null;
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(prefabName);
        GameObject prefab = null;
        if (customHardpoint != null) {
          prefab = ___combat.DataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if (prefab == null) {
            prefab = ___combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          }
        } else {
          Log.LogWrite(1, prefabName + " have no custom hardpoint\n", true);
          prefab = ___combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        }
        if (prefab == null) {
          Log.LogWrite(1, prefabName + " absent prefab. fallback\n", true);
          prefab = new GameObject("fake_hardpoint");
        }
        component = prefab.GetComponent<WeaponRepresentation>();
        if (component == null) {
          Log.LogWrite(1, prefabName + " have no WeaponRepresentation\n", true);
          component = prefab.AddComponent<WeaponRepresentation>();
          if (customHardpoint != null) {
            Log.LogWrite(1, "reiniting vfxTransforms\n");
            List<Transform> transfroms = new List<Transform>();
            for (int index = 0; index < customHardpoint.emitters.Count; ++index) {
              Transform[] trs = component.GetComponentsInChildren<Transform>();
              foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[index]) { transfroms.Add(tr); break; } }
            }
            Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
            for (int index = 0; index < transfroms.Count; ++index) {
              Log.LogWrite(2, transfroms[index].name + ":" + transfroms[index].localPosition + "\n");
            }
            if (transfroms.Count == 0) { transfroms.Add(prefab.transform); };
            component.vfxTransforms = transfroms.ToArray();
            if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
              Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
              GameObject shaderPrefab = ___combat.DataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
              if (shaderPrefab != null) {
                Log.LogWrite(1, "shader prefab found\n");
                Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
                if (shaderComponent != null) {
                  Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
                  MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                  foreach (MeshRenderer renderer in renderers) {
                    for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                      if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
                        Log.LogWrite(2, "keep original shader: "+ renderer.gameObject.transform.name + "\n");
                        continue;
                      }
                      Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
                      renderer.materials[mindex].shader = shaderComponent.material.shader;
                      renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
                    }
                  }
                }
                GameObject.Destroy(shaderPrefab);
              }
            }
          }
        }
        if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
          string str = string.Format("Null WeaponRepresentation for prefabName[{0}] parentBoneName[{1}] parentDisplayName[{2}]", (object)prefabName, (object)parentBone.name, (object)parentDisplayName);
          Log.LogWrite(1, str + "\n");
        }
        typeof(MechComponent).GetProperty("componentRep", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { component });
        //__instance.componentRep = (ComponentRepresentation)component;
        if (!((UnityEngine.Object)__instance.weaponRep != (UnityEngine.Object)null)) { return false; }
        __instance.weaponRep.Init(__instance, parentBone, true, parentDisplayName, __instance.Location);
        if (customHardpoint != null) {
          if (customHardpoint.offset.set) {
            Log.W("Altering position:" + prefab.transform.localPosition + " -> ");
            prefab.transform.localPosition += customHardpoint.offset.vector;
            Log.WL(prefab.transform.localPosition.ToString());
          }
          if (customHardpoint.scale.set) {
            Log.W("Altering scale:" + prefab.transform.localScale + " -> ");
            prefab.transform.localScale = customHardpoint.scale.vector;
            Log.WL(prefab.transform.localScale.ToString());
          }
          if (customHardpoint.rotate.set) {
            Log.W("Altering rotation:" + prefab.transform.localRotation.eulerAngles + " -> ");
            prefab.transform.localRotation = Quaternion.Euler(customHardpoint.rotate.vector);
            Log.WL(prefab.transform.localRotation.eulerAngles.ToString());
          }
        }
        HardPointAnimationController animComponent = __instance.weaponRep.GetComponent<HardPointAnimationController>();
        if (animComponent == null) {
          animComponent = __instance.weaponRep.gameObject.AddComponent<HardPointAnimationController>(); animComponent.Init(__instance.weaponRep, customHardpoint);
        }
        ParticleSystem[] pss = prefab.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          Log.WL("Starting ParticleSystem:"+ps.name);
          ps.Play();
        }
        return false;
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
        return true;
      }
    }
    public static void Postfix(Weapon __instance, string prefabName, Transform parentBone, string parentDisplayName) {
      try {
        Log.LogWrite(0, "Weapon.InitGameRep postfix: " + __instance.defId + "\n");
        Log.LogWrite(1, prefabName + ":" + parentBone.name + "\n");
        if (__instance == null) { return; }
        if (__instance.weaponRep == null) {
          Log.LogWrite(1, "null. creating empty fallback\n");
          GameObject prefab = new GameObject();
          WeaponRepresentation component = prefab.AddComponent<WeaponRepresentation>();
          typeof(MechComponent).GetProperty("componentRep", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { component });
          __instance.weaponRep.Init(__instance, parentBone, true, parentDisplayName, __instance.Location);
        }
        if (__instance.weaponRep.gameObject == null) { return; }
        __instance.weaponRep.gameObject.printComponents(1);
      }catch(Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }

}