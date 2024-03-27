﻿/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;
using Building = BattleTech.Building;

namespace CustAmmoCategories {
  public class LOSPseudoActor : AbstractActor {
    public float SpotterDistance = 0f;
    public Vector3[] pseudo_losSourcePositions = new Vector3[1] { Vector3.zero };
    public override PathingCapabilitiesDef PathingCaps => throw new NotImplementedException();
    public override float MaxWalkDistance => 0.0f;
    public override float MaxSprintDistance => 0.0f;
    public override bool CanSprint => throw new NotImplementedException();
    public override float MaxBackwardDistance => throw new NotImplementedException();
    public override float MaxMeleeEngageRangeDistance => throw new NotImplementedException();
    public override MovementCapabilitiesDef MovementCaps => throw new NotImplementedException();
    public override HardpointDataDef HardpointData => throw new NotImplementedException();
    public override float Radius => throw new NotImplementedException();
    public override DescriptionDef Description => throw new NotImplementedException();
    public override int SkillGunnery => throw new NotImplementedException();
    public override int SkillGuts => throw new NotImplementedException();
    public override int SkillPiloting => throw new NotImplementedException();
    public override int SkillTactics => throw new NotImplementedException();
    public override UnitRole StaticUnitRole => throw new NotImplementedException();
    public override float SensorSignatureFromDef => throw new NotImplementedException();
    public override float CurrentArmor => throw new NotImplementedException();
    public override float SummaryArmorMax => throw new NotImplementedException();
    public override float SummaryArmorCurrent => throw new NotImplementedException();
    public override float SummaryStructureMax => throw new NotImplementedException();
    public override float SummaryStructureCurrent => throw new NotImplementedException();
    public override float HealthAsRatio => throw new NotImplementedException();
    public override bool IsDead => throw new NotImplementedException();
    public override UnitType UnitType => throw new NotImplementedException();
    public override string UnitName => throw new NotImplementedException();
    public override string VariantName => throw new NotImplementedException();
    public override string Nickname => throw new NotImplementedException();
    public override Text GetActorInfoFromVisLevel(VisibilityLevel visLevel) {
      throw new NotImplementedException();
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1, float adjacentMultiplier = 1) {
      throw new NotImplementedException();
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      throw new NotImplementedException();
    }
    public override Vector3 GetImpactPosition(AbstractActor attacker, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation) {
      throw new NotImplementedException();
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      throw new NotImplementedException();
    }
    public override TagSet GetStaticUnitTags() {
      throw new NotImplementedException();
    }
    public override List<Text> GetWeaponNameStrings() {
      throw new NotImplementedException();
    }
    public override void InitGameRep(Transform parentTransform) {
      throw new NotImplementedException();
    }
    public override bool IsTargetPositionInFiringArc(ICombatant targetUnit, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition) {
      throw new NotImplementedException();
    }
    public override void ResolveWeaponDamage(WeaponHitInfo hitInfo) {
      throw new NotImplementedException();
    }
    public override void ResolveWeaponDamage(WeaponHitInfo hitInfo, Weapon weapon, MeleeAttackType meleeAttackType) {
      throw new NotImplementedException();
    }
    public override void TakeWeaponDamage(WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, float directStructureDamage, int hitIndex, DamageType damageType) {
      throw new NotImplementedException();
    }
  }
  [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSpotterRange")]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant) })]
  public static class LineOfSight_GetAdjustedSpotterRange {
    public static void Prefix(ref bool __runOriginal, ref float __result, LineOfSight __instance, ref AbstractActor source, ref ICombatant target) {
      if (source is LOSPseudoActor pseudo) { __result = pseudo.SpotterDistance; __runOriginal = false; source = null; target = null; }
    }
  }
  [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSensorRange")]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor) })]
  public static class LineOfSight_GetAdjustedSensorRange {
    public static void Prefix(ref bool __runOriginal, ref float __result, LineOfSight __instance, ref AbstractActor source, ref AbstractActor target) {
      if (source is LOSPseudoActor pseudo) { __result = pseudo.SpotterDistance; __runOriginal = false; source = null; target = null; }
    }
  }
  [HarmonyPatch(typeof(AbstractActor), "GetLongestRangeWeapon")]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class AbstractActor_GetLongestRangeWeapon {
    public static void Prefix(ref bool __runOriginal, ref Weapon __result, AbstractActor __instance, bool enabledWeaponsOnly, bool indirectFireOnly) {
      if (__instance is LOSPseudoActor pseudo) { __result = null; __runOriginal = false; }
    }
  }
  [HarmonyPatch(typeof(AbstractActor), "GetLOSSourcePositions")]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion) })]
  public static class AbstractActor_GetLOSSourcePositions {
    public static void Prefix(ref bool __runOriginal, ref Vector3[] __result, AbstractActor __instance, Vector3 position, Quaternion rotation) {
      if (__instance is LOSPseudoActor pseudo) { __result = pseudo.pseudo_losSourcePositions; __runOriginal = false; }
    }
  }
  public static class AreaOfEffectHelper {
    private static LOSPseudoActor f_pseudoLoSActor = null;
    public static LOSPseudoActor pseudoLOSActor {
      get {
        if (f_pseudoLoSActor == null) {
          f_pseudoLoSActor = new LOSPseudoActor();
          f_pseudoLoSActor.GUID = Guid.NewGuid().ToString();
        }
        return f_pseudoLoSActor;
      }
    }
    private static Dictionary<ICombatant, bool> isDropshipCache = new Dictionary<ICombatant, bool>();
    public static void Clear() { isDropshipCache.Clear(); }
    public static void TagAoEModifiers(this ICombatant target, out float range, out float damage) {
      range = 1f;
      damage = 1f;
      HBS.Collections.TagSet tags = target.Tags();
      foreach (var tag in tags) {
        if(CustomAmmoCategories.Settings.TagAoEDamageMult.TryGetValue(tag,out AoEModifiers mods)) {
          range *= mods.Range; damage *= mods.Damage;
        }
      }
    }
    public static bool isDropshipNotLanded(this ICombatant target) {
      try {
        if (isDropshipCache.TryGetValue(target, out bool result)) { return result; }
        Log.Combat?.TWL(0, "AreaOfEffectHelper.isDropshipNotLanded "+new Text(target.DisplayName));
        BattleTech.Building building = target as BattleTech.Building;
        if (building == null) {
          Log.M.WL(1, "not building");
          isDropshipCache.Add(target, false); return false;
        }
        ObstructionGameLogic logic = ObstructionGameLogic.GetObstructionFromBuilding(building, target.Combat.ItemRegistry);
        if (logic.IsDropship() == false) {
          Log.Combat?.WL(1, "not dropship");
          isDropshipCache.Add(target, false); return false;
        }
        DropshipGameLogic dropLogic = logic as DropshipGameLogic;
        if (dropLogic == null) {
          Log.Combat?.WL(1, "no dropship logic");
          isDropshipCache.Add(target, logic.IsDropship()); return logic.IsDropship();
        }
        Log.Combat?.WL(1, "drop ship current animation state:"+ dropLogic.currentAnimationState);
        return dropLogic.currentAnimationState != DropshipAnimationState.Landed;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CustomAmmoCategories.AttackSequence_logger.LogException(e);
        return false;
      }
    }
    public static WeaponHitInfo generateAoEWeaponHitInfo(ICombatant combatant,AbstractActor attacker, Weapon weapon,Dictionary<int, float> dmgInfo) {
      WeaponHitInfo hitInfo = new WeaponHitInfo();
      hitInfo.attackerId = attacker.GUID;
      hitInfo.targetId = combatant.GUID;
      hitInfo.numberOfShots = dmgInfo.Count;
      hitInfo.stackItemUID = -1;
      hitInfo.attackSequenceId = -1;
      hitInfo.attackGroupIndex = -1;
      hitInfo.attackWeaponIndex = -1;
      hitInfo.toHitRolls = new float[dmgInfo.Count];
      hitInfo.locationRolls = new float[dmgInfo.Count];
      hitInfo.dodgeRolls = new float[dmgInfo.Count];
      hitInfo.dodgeSuccesses = new bool[dmgInfo.Count];
      hitInfo.hitLocations = new int[dmgInfo.Count];
      hitInfo.hitPositions = new Vector3[dmgInfo.Count];
      hitInfo.hitVariance = new int[dmgInfo.Count];
      hitInfo.hitQualities = new AttackImpactQuality[dmgInfo.Count];
      hitInfo.secondaryTargetIds = new string[dmgInfo.Count];
      hitInfo.secondaryHitLocations = new int[dmgInfo.Count];
      hitInfo.attackDirections = new AttackDirection[dmgInfo.Count];
      int hitIndex = 0;
      foreach(var dmg in dmgInfo) {
        hitInfo.toHitRolls[hitIndex] = dmg.Value;
        hitInfo.dodgeRolls[hitIndex] = 0f;
        hitInfo.dodgeSuccesses[hitIndex] = false;
        hitInfo.hitLocations[hitIndex] = dmg.Key;
        int Location = hitInfo.hitLocations[hitIndex];
        string secTarget = string.Empty;
        //int secLocation = 0;
        hitInfo.hitPositions[hitIndex] = ImpactPositionHelper.GetHitPositionFast_Combatant(combatant, attacker.TargetPosition, Location, false);
          //combatant.GetImpactPosition(attacker, attacker.CurrentPosition, weapon, ref Location, ref hitInfo.attackDirections[hitIndex], ref secTarget, ref secLocation);
        hitInfo.hitVariance[hitIndex] = 0;
        hitInfo.hitQualities[hitIndex] = AttackImpactQuality.Solid;
        hitInfo.secondaryTargetIds[hitIndex] = string.Empty;
        hitInfo.secondaryHitLocations[hitIndex] = 0;
        ++hitIndex;
      }
      return hitInfo;
    }
    public static void AoEProcessing(ref WeaponHitInfo hitInfo) {
      if (hitInfo.isAdvanced() == false) {
        Log.Combat?.WL(1,"not advanced");
        return;
      }
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      Weapon weapon = advInfo.weapon;
      if (weapon.AOECapable() == false) { return; }
      float AOERange = weapon.AOERange();
      if(weapon.isOnlyDefferEffect()) { AOERange = 0f; }
      if (AOERange < CustomAmmoCategories.Epsilon) { return; };
      bool PhysicsAoE = weapon.PhysicsAoE();
      float PhysicsAoEHeight = weapon.PhysicsAoEHeight();
      int PhysicsAoELayers = LayerMask.GetMask("Terrain", "Obstruction", "Combatant", "NoCollision");
      int Combatant_layer = LayerMask.NameToLayer("Combatant");
      int NoCollision_layer = LayerMask.NameToLayer("NoCollision");
      Log.Combat?.TWL(0,$"AOE generation started {advInfo.Sequence.attacker.DisplayName} {weapon.defId} grp:{hitInfo.attackGroupIndex} index:{hitInfo.attackWeaponIndex} shots:{advInfo.hits.Count} PhysicsAoE:{PhysicsAoE}");
      if (advInfo.hits.Count == 0) { return; };
      if (advInfo.hits.Count != hitInfo.hitLocations.Length) {
        Log.Combat?.TWL(0, $"WARNING! advInfo count {advInfo.hits.Count} is not equal hitInfo length:{hitInfo.hitLocations.Length}. Any processing should be avoided", true);
        CustomAmmoCategories.AttackSequence_logger.LogError($"WARNING! advInfo count {advInfo.hits.Count} is not equal hitInfo length:{hitInfo.hitLocations.Length}. Any processing should be avoided\n"+Environment.StackTrace);
        return;
      }
      bool HasShells = weapon.HasShells();
      //bool DamagePerPallet = weapon.DamagePerPallet();
      float AoEDamage = weapon.AOEDamage();
      float AoEHeat = weapon.AOEHeatDamage();
      if ((AoEHeat < CustomAmmoCategories.Epsilon) && (AoEDamage < CustomAmmoCategories.Epsilon)) {
        AoEDamage = weapon.DamagePerShot;
        AoEHeat = weapon.HeatDamagePerShot;
      }
      if (AoEDamage < CustomAmmoCategories.Epsilon) { AoEDamage = 0.1f; };
      float AoEStability = weapon.AOEInstability();
      float FullAoEDamage = AoEDamage * advInfo.hits.Count;
      if (weapon.defId.IndexOf("Nuke", StringComparison.InvariantCultureIgnoreCase) >= 0) {
        Log.Combat?.WL(0,$"add nuke damage {FullAoEDamage}");
        PersistentMapClientHelper.FloatAdd("NukeDamage", FullAoEDamage);
      }else
      if (weapon.ammo().Id.IndexOf("Nuke", StringComparison.InvariantCultureIgnoreCase) >= 0) {
        Log.Combat?.WL(0, $"add nuke damage {FullAoEDamage}");
        PersistentMapClientHelper.FloatAdd("NukeDamage", FullAoEDamage);
      }
      Dictionary<ICombatant, Dictionary<int, float>> targetsHitCache = new Dictionary<ICombatant, Dictionary<int, float>>();
      Dictionary<ICombatant, float> targetsHeatCache = new Dictionary<ICombatant, float>();
      Dictionary<ICombatant, float> targetsStabCache = new Dictionary<ICombatant, float>();
      for(int hitIndex = 0; hitIndex < advInfo.hits.Count; ++hitIndex) {
        AdvWeaponHitInfoRec advRec = advInfo.hits[hitIndex];
        if (advRec == null) { continue; }
        if (advRec.interceptInfo.Intercepted) {
          Log.Combat?.WL(1,"intercepted missiles not generating AOE");
          continue;
        }
        if (advRec.fragInfo.separated) {
          Log.Combat?.WL(1, "separated frags not generating AOE");
          continue;
        }
        Vector3 hitPosition = advRec.hitPosition;
        List<ICombatant> combatants = new List<ICombatant>();
        List<ICombatant> allCombatants = advInfo.Sequence.attacker.Combat.GetAllCombatants();
        string IFFDef = weapon.IFFTransponderDef();
        if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
          HashSet<string> combatantsGuids = new HashSet<string>();
          List<AbstractActor> enemies = advInfo.Sequence.attacker.Combat.GetAllEnemiesOf(advInfo.Sequence.attacker);
          foreach (ICombatant combatant in enemies) {
            if (combatantsGuids.Contains(combatant.GUID) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
          foreach (ICombatant combatant in allCombatants) {
            if (combatant.GUID == advInfo.Sequence.attacker.GUID) { continue; }
            if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
            if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant, IFFDef) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
        }
        foreach (ICombatant target in combatants) {
          if (target.IsDead) { continue; };
          if (target.isDropshipNotLanded()) { continue; };
          if (CustomAmmoCategories.Settings.SpawnProtectionAffectsAOE) {
            if (target.isSpawnProtected()) { continue; }
            if (weapon.parent.isSpawnProtected()) { continue; }
          }
          ICombatant aoeTarget = target;
          if (target is ICustomMech cmech) {
            if (cmech.carrier != null) {
              Log.Combat?.WL(1, $"{target.DisplayName} attached to {cmech.carrier.DisplayName} using its position and LoS instead");
              aoeTarget = cmech.carrier;
              if (cmech.isMountedExternal == false) {
                Log.Combat?.WL(1, $"mounted internally. no AoE damage");
                continue;
              }
            }
          }
          Vector3 CurrentPosition = aoeTarget.CurrentPosition;
          if(advRec.isHit == false) { CurrentPosition += Vector3.up* aoeTarget.FlyingHeight(); } else {
            if (advRec.target.GUID != target.GUID) { CurrentPosition += Vector3.up * aoeTarget.FlyingHeight(); }
          }
          float distance = Vector3.Distance(CurrentPosition, hitPosition);
          Log.Combat?.WL(1, "testing combatant " + target.DisplayName + " " + target.GUID + " " + distance + "("+CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range+")/" + AOERange);
          if (CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range < CustomAmmoCategories.Epsilon) { CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range = 1f; }
          distance /= CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range;
          target.TagAoEModifiers(out float tagAoEModRange, out float tagAoEDamage);
          if (tagAoEModRange < CustomAmmoCategories.Epsilon) { tagAoEModRange = 1f; }
          if (tagAoEDamage < CustomAmmoCategories.Epsilon) { tagAoEDamage = 1f; }
          distance /= tagAoEModRange;
          if (distance > AOERange) { continue; }
          if(PhysicsAoE) {
            Vector3 raycastStart = hitPosition + Vector3.up * PhysicsAoEHeight;
            Vector3 raycastEnd = aoeTarget.TargetPosition;
            AreaOfEffectHelper.pseudoLOSActor.Combat = target.Combat;
            AreaOfEffectHelper.pseudoLOSActor.SpotterDistance = Vector3.Distance(raycastStart, raycastEnd) + 100f;
            AreaOfEffectHelper.pseudoLOSActor.pseudo_losSourcePositions[0] = raycastStart;
            AreaOfEffectHelper.pseudoLOSActor._team = target.Combat.LocalPlayerTeam;
            AreaOfEffectHelper.pseudoLOSActor._teamId = target.Combat.LocalPlayerTeamGuid;
            var lof = aoeTarget.Combat.LOS.GetLineOfFire(AreaOfEffectHelper.pseudoLOSActor, raycastStart, aoeTarget, aoeTarget.CurrentPosition, aoeTarget.CurrentRotation, out var collisionWorldPos);
            Log.Combat?.WL(2,$"{raycastStart}->{aoeTarget.DisplayName} LoF:{lof}");
            if (lof == LineOfFireLevel.LOFBlocked) { continue; }
            //var raycast = Physics.RaycastAll(new Ray(raycastStart, (raycastEnd - raycastStart).normalized), AOERange, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            //RaycastHit? phy_hit = new RaycastHit?();
            //float dist = float.PositiveInfinity;
            //foreach(var rhit in raycast) {
            //  float temp = Vector3.Distance(raycastStart, rhit.point);
            //  Log.Combat?.WL(3, $"raycast result {rhit.collider.gameObject.transform.name} layer:{LayerMask.LayerToName(rhit.collider.gameObject.layer)} distance:{temp}");
            //  if(phy_hit.HasValue == false) { phy_hit = rhit; dist = temp; continue; }
            //  if(temp < dist) { phy_hit = rhit; dist = temp; }
            //}
            //Vector3 direction = raycastEnd - raycastStart;
            //var ray = new Ray(raycastStart, direction);
            //GameObject debugLineGO = GameObject.Instantiate(WeaponRangeIndicators.Instance.LineTemplate.gameObject);
            //debugLineGO.transform.SetParent(WeaponRangeIndicators.Instance.transform);
            //debugLineGO.name = $"debugRaycast{weapon.parent.GUID}{target.GUID}";
            //debugLineGO.SetActive(true);
            //LineRenderer debugLine = debugLineGO.GetComponentInChildren<LineRenderer>(true);
            //debugLine.startWidth = 2.0f;
            //debugLine.endWidth = 2.0f;
            //debugLine.positionCount = 2;
            //debugLine.material = WeaponRangeIndicators.Instance.MaterialInRange;
            //debugLine.startColor = Color.red;
            //debugLine.endColor = Color.red;
            //debugLine.SetPosition(0, raycastStart);
            //debugLine.SetPosition(1, (ray.direction * direction.magnitude) + raycastStart);
            //if (Physics.Raycast(new Ray(raycastStart, raycastEnd - raycastStart),out var phy_hit, direction.magnitude, PhysicsAoELayers, QueryTriggerInteraction.Ignore)) {
            //  Log.Combat?.WL(2, $"raycast result start:{raycastStart} end:{(ray.direction * direction.magnitude)+raycastStart}/{raycastEnd} hit point:{phy_hit.point} distance:{phy_hit.distance} {phy_hit.collider.gameObject.transform.name} layer:{LayerMask.LayerToName(phy_hit.collider.gameObject.layer)}");
            //  if((phy_hit.collider.gameObject.layer != Combatant_layer) && (phy_hit.collider.gameObject.layer != NoCollision_layer)) { continue; }
            //  PilotableActorRepresentation unitRep = phy_hit.collider.GetComponentInParent<PilotableActorRepresentation>();
            //  Log.Combat?.WL(3, $"unit:{(unitRep == null ? "null" : unitRep.parentActor.PilotableActorDef.ChassisID)}");
            //  if(unitRep != null) {
            //    if(unitRep.parentActor != target) {
            //      Log.Combat?.WL(4, $"other unit blocks raycast");
            //      continue;
            //    } else {
            //      Log.Combat?.WL(4, $"target unit reached - AoE process normal");
            //    }
            //  }
            //};
          }
          if (targetsHitCache.ContainsKey(target) == false) { targetsHitCache.Add(target, new Dictionary<int, float>()); }
          if (targetsHeatCache.ContainsKey(target) == false) { targetsHeatCache.Add(target, 0f); }
          if (targetsStabCache.ContainsKey(target) == false) { targetsStabCache.Add(target, 0f); }
          //Dictionary<int, float> targetHitCache = targetsHitCache[target];
          float DamagePerShot = AoEDamage * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          float HeatDamagePerShot = AoEHeat * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          float StabilityPerShot = AoEStability * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          float distanceRatio = weapon.AoEDmgFalloffType((AOERange - distance) / AOERange);
          float targetAoEMult = target.AoEDamageMult();
          float targetHeatMult = target.IncomingHeatMult() * target.ScaleIncomingHeat();
          float targetStabMult = target.IncomingStabilityMult();
          float fullDamage = DamagePerShot * distanceRatio * targetAoEMult;
          float heatDamage = HeatDamagePerShot * distanceRatio * targetAoEMult * targetHeatMult;
          float stabDamage = StabilityPerShot * distanceRatio * targetAoEMult * targetStabMult;
          if (target.isHasHeat() == false) { fullDamage += heatDamage; heatDamage = 0f; }
          if (target.isHasStability() == false) { stabDamage = 0f; }
          targetsHeatCache[target] += heatDamage;
          targetsStabCache[target] += stabDamage;
          Log.Combat?.WL(1, "full damage " + fullDamage + " AoEDamage: "+ AoEDamage + " byTypeMod:"+ CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage + " tagAoEDamage:"+ tagAoEDamage + " distanceRatio:" + distanceRatio+ " targetAoEMult:"+ targetAoEMult);
          if (fullDamage < 0f) { fullDamage = 0f; };
          HashSet<int> reachableLocations = new HashSet<int>();
          Dictionary<int, float> SpreadLocations = null;
          Mech mech = target as Mech;
          Vehicle vehicle = target as Vehicle;
          ICustomMech custMech = target as ICustomMech;
          if(custMech != null) {
            List<int> hitLocations = custMech.GetAOEPossibleHitLocations(hitPosition);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            SpreadLocations = custMech.GetAOESpreadArmorLocations();
          } else
          if (mech != null) {
            List<int> hitLocations = mech.GetAOEPossibleHitLocations(hitPosition);//advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Mech);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            SpreadLocations = mech.GetAOESpreadArmorLocations();
          } else
          if (vehicle != null) {
            List<int> hitLocations = advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, vehicle);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            if (CustomAmmoCategories.VehicleLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            SpreadLocations = CustomAmmoCategories.VehicleLocations;
          } else {
            List<int> hitLocations = new List<int>() { 1 };
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            if (CustomAmmoCategories.OtherLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            SpreadLocations = CustomAmmoCategories.OtherLocations;
          }
          Log.Combat?.TWL(0, "SpreadLocations:"+(SpreadLocations==null?"null":"not null"));
          float locationsCoeff = 0f;
          foreach (var sLoc in SpreadLocations) {
            if (reachableLocations.Contains(sLoc.Key)) { locationsCoeff += sLoc.Value; }
          }
          Dictionary<int, float> AOELocationDamage = new Dictionary<int, float>();
          Log.Combat?.W(2,"Location spread:");
          foreach (var sLoc in SpreadLocations) {
            if (reachableLocations.Contains(sLoc.Key) == false) { continue; }
            if (sLoc.Value < CustomAmmoCategories.Epsilon) { continue; }
            AOELocationDamage.Add(sLoc.Key, sLoc.Value / locationsCoeff);
            string lname = sLoc.Key.ToString();
            if (mech != null) { lname = ((ArmorLocation)sLoc.Key).ToString(); } else
            if (vehicle != null) { lname = ((VehicleChassisLocations)sLoc.Key).ToString(); } else
              lname = ((BuildingLocation)sLoc.Key).ToString();
            Log.Combat?.W(1, lname+":"+ sLoc.Value / locationsCoeff);
          }
          Log.Combat?.WL(0,"");
          foreach (var hitLocation in AOELocationDamage) {
            float CurrentLocationDamage = fullDamage * hitLocation.Value;
            if (targetsHitCache[target].ContainsKey(hitLocation.Key)) {
              targetsHitCache[target][hitLocation.Key] += CurrentLocationDamage;
            } else {
              targetsHitCache[target].Add(hitLocation.Key, CurrentLocationDamage);
            }
            Log.Combat?.WL(2, "location " + hitLocation + " damage " + targetsHitCache[target][hitLocation.Key]);
          }
        }
      }
      Log.Combat?.WL(1, "consolidated AoE damage:");
      foreach (var targetHitCache in targetsHitCache) {
        Log.Combat?.WL(2, targetHitCache.Key.DisplayName+":"+targetHitCache.Key.GUID);
        foreach (var targetHit in targetHitCache.Value) {
          Log.Combat?.WL(3, "location:" + targetHit.Key+ ":"+targetHit.Value);
        }
      }
      int AOEHitsCount = advInfo.hits.Count;
      Dictionary<ICombatant, WeaponHitInfo> AoEHitInfos = new Dictionary<ICombatant, WeaponHitInfo>();
      foreach (var targetHitCache in targetsHitCache) {
        WeaponHitInfo aHitInfo = AreaOfEffectHelper.generateAoEWeaponHitInfo(targetHitCache.Key, advInfo.Sequence.attacker, advInfo.weapon, targetHitCache.Value);
        AoEHitInfos.Add(targetHitCache.Key, aHitInfo);
        AOEHitsCount += aHitInfo.numberOfShots;
      }
      FragWeaponHelper.Resize(ref hitInfo, AOEHitsCount);
      int hIndex = advInfo.hits.Count;
      int aoeStartIndex = advInfo.hits.Count - 1;
      foreach (var aoeHitInfo in AoEHitInfos) {
        for(int hi = 0; hi < aoeHitInfo.Value.numberOfShots; ++hi) {
          //AdvWeaponHitInfoRec advRec = new AdvWeaponHitInfoRec(advInfo);
          hitInfo.toHitRolls[hIndex] = 1f;
          hitInfo.locationRolls[hIndex] = 1f;
          hitInfo.dodgeRolls[hIndex] = 1f;
          hitInfo.dodgeSuccesses[hIndex] = false;
          hitInfo.hitPositions[hIndex] = aoeHitInfo.Value.hitPositions[hi];
          hitInfo.hitVariance[hIndex] = aoeHitInfo.Value.hitVariance[hi];
          hitInfo.hitQualities[hIndex] = aoeHitInfo.Value.hitQualities[hi];
          hitInfo.attackDirections[hIndex] = aoeHitInfo.Value.attackDirections[hi];
          int hitLocation = aoeHitInfo.Value.hitLocations[hi];
          if (aoeHitInfo.Key.GUID == hitInfo.targetId) {
            hitInfo.hitLocations[hIndex] = aoeHitInfo.Value.hitLocations[hi];
            hitInfo.secondaryTargetIds[hIndex] = aoeHitInfo.Value.secondaryTargetIds[hi];
            hitInfo.secondaryHitLocations[hIndex] = aoeHitInfo.Value.secondaryHitLocations[hi];
          } else {
            hitInfo.hitLocations[hIndex] = 65536;
            hitInfo.secondaryTargetIds[hIndex] = aoeHitInfo.Key.GUID;
            hitInfo.secondaryHitLocations[hIndex] = aoeHitInfo.Value.hitLocations[hi];
          }
          float Damage = 0f;
          float Heat = 0f;
          float Stability = 0f;
          if (targetsHitCache.ContainsKey(aoeHitInfo.Key)) {
            if (targetsHitCache[aoeHitInfo.Key].ContainsKey(hitLocation)) {
              Damage = targetsHitCache[aoeHitInfo.Key][hitLocation];
            }
          }
          if(hi == 0) {
            if (targetsHeatCache.ContainsKey(aoeHitInfo.Key)) {
              Heat = targetsHeatCache[aoeHitInfo.Key];
            }
            if (targetsHeatCache.ContainsKey(aoeHitInfo.Key)) {
              Stability = targetsStabCache[aoeHitInfo.Key];
            }
          }
          advInfo.AppendAoEHit(aoeStartIndex, FullAoEDamage, Damage, Heat, Stability, aoeHitInfo.Key, aoeHitInfo.Value.hitPositions[hi], hitLocation);
          ++hIndex;
        }
      }
    }
  }
}